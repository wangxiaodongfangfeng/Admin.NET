using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
#if WINDOWS
using Microsoft.Extensions.Hosting.WindowsServices;
#endif

namespace AdminNetUpdater;

/// <summary>
/// 内嵌 HTTP 服务器，提供：
///   GET  /              → 管理 UI（单页 HTML）
///   GET  /api/status    → 当前状态（服务是否在运行、上传的包列表）
///   POST /api/upload    → 上传 zip 更新包（multipart/form-data, field: file）
///   POST /api/deploy    → 触发 deploy，返回 task id
///   GET  /api/log/{id}  → SSE 实时日志流
///   DELETE /api/packages/{name} → 删除已上传的包
/// </summary>
public static class WebServer
{
    // 部署任务状态
    private static volatile bool   _deploying = false;
    private static volatile string _lastStatus = "idle"; // idle | running | success | failed
    private static string          _lastDeployedAt = "";

    // SSE 订阅者
    private static readonly ConcurrentDictionary<string, ConcurrentQueue<string?>> _subscribers = new();

    public static async Task Run(Config cfg, string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ── Windows 服务宿主支持 ──────────────────────────────────────────────
#if WINDOWS
        if (WindowsServiceHelpers.IsWindowsService())
        {
            builder.Host.UseWindowsService(options =>
            {
                options.ServiceName = cfg.UpdaterServiceName;
            });
        }
#endif

        // 只监听配置的端口
        var port = cfg.WebServerPort;
        builder.WebHost.UseUrls($"http://*:{port}");

        // 抑制 ASP.NET Core 控制台日志（服务模式下 Console 不可用）
        builder.Logging.ClearProviders();
#if WINDOWS
        if (!WindowsServiceHelpers.IsWindowsService())
            builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Warning);
#else
        builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Warning);
#endif

        var app = builder.Build();

        // ── 静态 UI ──────────────────────────────────────────────────────────
        app.MapGet("/", () => Results.Content(BuildHtml(cfg), "text/html; charset=utf-8"));

        // ── API: 状态 ─────────────────────────────────────────────────────────
        app.MapGet("/api/status", () =>
        {
            var packageDir = cfg.ResolvedDeployPackageDir;
            var packages   = DeployCommand.FindLatestPackage(packageDir) != null
                ? Directory.EnumerateFiles(packageDir, "AdminNET_package_*.zip")
                    .OrderByDescending(File.GetLastWriteTime)
                    .Select(f => new { name = Path.GetFileName(f), size = new FileInfo(f).Length, time = File.GetLastWriteTime(f).ToString("yyyy-MM-dd HH:mm:ss") })
                    .ToList<object>()
                : new List<object>();

            string svcStatus = ServiceControl.GetStatus(cfg.ServiceName);
#if WINDOWS
            // Windows 下 not_found 仍可能是 ServiceController 找不到
#endif

            return Results.Json(new
            {
                deployStatus    = _lastStatus,
                deploying       = _deploying,
                lastDeployedAt  = _lastDeployedAt,
                serviceStatus   = svcStatus,
                serviceName     = cfg.ServiceName,
                packages,
            });
        });

        // ── API: 上传包 ───────────────────────────────────────────────────────
        app.MapPost("/api/upload", async (HttpRequest req) =>
        {
            if (!req.HasFormContentType)
                return Results.BadRequest(new { error = "需要 multipart/form-data" });

            var form = await req.ReadFormAsync();
            var file = form.Files.GetFile("file");
            if (file == null || file.Length == 0)
                return Results.BadRequest(new { error = "未收到文件" });

            if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                return Results.BadRequest(new { error = "只支持 .zip 文件" });

            var saveDir  = cfg.ResolvedDeployPackageDir;
            Directory.CreateDirectory(saveDir);

            // 保留原始文件名，但加时间戳避免覆盖
            var baseName = Path.GetFileNameWithoutExtension(file.FileName);
            var saveName = $"{baseName}_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
            var savePath = Path.Combine(saveDir, saveName);

            await using var dest = File.Create(savePath);
            await file.CopyToAsync(dest);

            var sizeMb = new FileInfo(savePath).Length / 1048576.0;
            Console.WriteLine($"[Upload] {saveName}  ({sizeMb:F2} MB)");

            return Results.Json(new { success = true, fileName = saveName, sizeMb });
        });

        // ── API: 删除包 ───────────────────────────────────────────────────────
        app.MapDelete("/api/packages/{name}", (string name) =>
        {
            // 安全校验：只允许删除 packages 目录中的 zip
            if (name.Contains("..") || name.Contains('/') || name.Contains('\\'))
                return Results.BadRequest(new { error = "非法文件名" });

            var path = Path.Combine(cfg.ResolvedDeployPackageDir, name);
            if (!File.Exists(path))
                return Results.NotFound(new { error = "文件不存在" });

            File.Delete(path);
            return Results.Json(new { success = true });
        });

        // ── API: 触发部署 ─────────────────────────────────────────────────────
        app.MapPost("/api/deploy", ([FromBody] DeployRequest? req) =>
        {
            if (_deploying)
                return Results.Json(new { error = "正在部署中，请稍候" }, statusCode: 409);

            var taskId = Guid.NewGuid().ToString("N")[..8];
            var queue  = new ConcurrentQueue<string?>();
            _subscribers[taskId] = queue;

            // 确定使用哪个 zip
            string? zipPath = null;
            if (!string.IsNullOrWhiteSpace(req?.fileName))
            {
                var candidate = Path.Combine(cfg.ResolvedDeployPackageDir, req.fileName);
                if (!File.Exists(candidate))
                    return Results.Json(new { error = $"包 [{req.fileName}] 不存在" }, statusCode: 400);
                zipPath = candidate;
            }
            else
            {
                zipPath = DeployCommand.FindLatestPackage(cfg.ResolvedDeployPackageDir);
                if (zipPath == null)
                    return Results.Json(new { error = "找不到任何更新包，请先上传" }, statusCode: 400);
            }

            _deploying   = true;
            _lastStatus  = "running";

            // 后台执行 deploy
            var zipPathCaptured = zipPath;
            Task.Run(async () =>
            {
                void Emit(string level, string msg)
                {
                    var line = $"data: {JsonSerializer.Serialize(new { level, msg, time = DateTime.Now.ToString("HH:mm:ss") })}\n\n";
                    queue.Enqueue(line);
                    Console.Write($"[Deploy/{taskId}] [{level}] {msg}\n");
                }

                try
                {
                    var success = await DeployCommand.RunFromApi(cfg, zipPathCaptured, Emit);
                    _lastStatus     = success ? "success" : "failed";
                    _lastDeployedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    Emit(success ? "done" : "error", success ? "部署完成" : "部署失败");
                }
                finally
                {
                    _deploying = false;
                    queue.Enqueue(null); // null = 流结束信号
                }
            });

            return Results.Json(new { taskId });
        });

        // ── API: SSE 日志流 ───────────────────────────────────────────────────
        app.MapGet("/api/log/{taskId}", async (string taskId, HttpContext ctx) =>
        {
            if (!_subscribers.TryGetValue(taskId, out var queue))
            {
                ctx.Response.StatusCode = 404;
                return;
            }

            ctx.Response.Headers["Content-Type"]  = "text/event-stream";
            ctx.Response.Headers["Cache-Control"] = "no-cache";
            ctx.Response.Headers["Connection"]    = "keep-alive";
            ctx.Response.Headers["Access-Control-Allow-Origin"] = "*";

            var ct = ctx.RequestAborted;
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    if (queue.TryDequeue(out var line))
                    {
                        if (line == null) break;          // deploy 结束
                        await ctx.Response.WriteAsync(line, ct);
                        await ctx.Response.Body.FlushAsync(ct);
                    }
                    else
                    {
                        await Task.Delay(50, ct);
                    }
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                _subscribers.TryRemove(taskId, out _);
            }
        });

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n  Admin.NET Updater Web Server 已启动");
        Console.WriteLine($"  访问地址: http://localhost:{port}");
        Console.WriteLine($"  按 Ctrl+C 停止\n");
        Console.ResetColor();

        await app.RunAsync();
    }

    private record DeployRequest(string? fileName);

    // ── 内嵌 HTML UI ──────────────────────────────────────────────────────────

    private static string BuildHtml(Config cfg) => $$"""
<!DOCTYPE html>
<html lang="zh-CN">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Admin.NET Updater</title>
<style>
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
         background: #0f172a; color: #e2e8f0; min-height: 100vh; padding: 24px; }
  h1 { font-size: 22px; font-weight: 700; color: #38bdf8; margin-bottom: 4px; }
  .subtitle { color: #64748b; font-size: 13px; margin-bottom: 24px; }
  .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 16px; }
  @media(max-width:768px) { .grid { grid-template-columns: 1fr; } }
  .card { background: #1e293b; border-radius: 10px; padding: 20px; border: 1px solid #334155; }
  .card h2 { font-size: 15px; font-weight: 600; color: #94a3b8; margin-bottom: 14px;
             display: flex; align-items: center; gap: 8px; }
  .badge { display: inline-block; padding: 2px 8px; border-radius: 99px; font-size: 11px; font-weight: 600; }
  .badge-running { background: #1d4ed8; color: #bfdbfe; }
  .badge-stopped { background: #374151; color: #9ca3af; }
  .badge-success { background: #064e3b; color: #6ee7b7; }
  .badge-failed  { background: #7f1d1d; color: #fca5a5; }
  .badge-idle    { background: #374151; color: #9ca3af; }
  .badge-unknown { background: #374151; color: #9ca3af; }
  .badge-not_found { background: #7f1d1d; color: #fca5a5; }
  label { display: block; font-size: 13px; color: #94a3b8; margin-bottom: 6px; }
  .drop-zone { border: 2px dashed #334155; border-radius: 8px; padding: 32px; text-align: center;
               cursor: pointer; transition: all .2s; color: #64748b; font-size: 14px; }
  .drop-zone:hover, .drop-zone.dragover { border-color: #38bdf8; color: #38bdf8; background: #0f2340; }
  .drop-zone input { display: none; }
  .progress-bar { height: 4px; background: #1e3a5f; border-radius: 2px; margin-top: 10px; overflow: hidden; }
  .progress-fill { height: 100%; background: #38bdf8; border-radius: 2px; transition: width .3s; width: 0; }
  .pkg-list { margin-top: 10px; }
  .pkg-item { display: flex; align-items: center; justify-content: space-between;
              padding: 8px 10px; background: #0f172a; border-radius: 6px; margin-bottom: 6px;
              font-size: 13px; }
  .pkg-name { color: #e2e8f0; flex: 1; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
  .pkg-meta { color: #64748b; font-size: 11px; margin: 0 10px; white-space: nowrap; }
  .pkg-actions { display: flex; gap: 6px; }
  .btn { padding: 7px 16px; border: none; border-radius: 6px; cursor: pointer; font-size: 13px;
         font-weight: 500; transition: all .2s; }
  .btn:disabled { opacity: .4; cursor: not-allowed; }
  .btn-primary { background: #0ea5e9; color: #fff; }
  .btn-primary:hover:not(:disabled) { background: #0284c7; }
  .btn-danger  { background: #ef444420; color: #f87171; }
  .btn-danger:hover:not(:disabled) { background: #ef444440; }
  .btn-sm { padding: 4px 10px; font-size: 12px; }
  .log-panel { background: #0a0e1a; border-radius: 8px; padding: 14px;
               height: 360px; overflow-y: auto; font-family: 'Cascadia Code', 'Fira Code', monospace;
               font-size: 12.5px; line-height: 1.6; }
  .log-line { margin-bottom: 2px; }
  .log-time { color: #475569; margin-right: 8px; }
  .log-step  { color: #f59e0b; font-weight: 600; }
  .log-ok    { color: #34d399; }
  .log-warn  { color: #fbbf24; }
  .log-error { color: #f87171; }
  .log-done  { color: #34d399; font-weight: 700; }
  .log-info  { color: #94a3b8; }
  .status-row { display: flex; gap: 12px; margin-bottom: 14px; flex-wrap: wrap; }
  .status-item { font-size: 13px; }
  .status-label { color: #64748b; }
  .upload-status { margin-top: 8px; font-size: 13px; }
  .upload-status.ok   { color: #34d399; }
  .upload-status.err  { color: #f87171; }
</style>
</head>
<body>
<h1>🚀 Admin.NET Updater</h1>
<p class="subtitle">服务: {{cfg.ServiceName}} &nbsp;|&nbsp; 目录: {{cfg.ServiceDir}}</p>

<div class="grid">
  <!-- 状态卡片 -->
  <div class="card">
    <h2>📊 当前状态</h2>
    <div class="status-row">
      <div class="status-item">
        <span class="status-label">应用服务：</span>
        <span id="svcBadge" class="badge">-</span>
      </div>
      <div class="status-item">
        <span class="status-label">部署状态：</span>
        <span id="deployBadge" class="badge">-</span>
      </div>
      <div class="status-item">
        <span class="status-label">上次部署：</span>
        <span id="lastDeploy" style="font-size:13px;color:#e2e8f0">-</span>
      </div>
    </div>
    <h2 style="margin-top:10px">📦 已上传的包</h2>
    <div class="pkg-list" id="pkgList">
      <div style="color:#64748b;font-size:13px">加载中...</div>
    </div>
  </div>

  <!-- 上传卡片 -->
  <div class="card">
    <h2>⬆️ 上传更新包</h2>
    <label>选择或拖拽 .zip 文件到下方区域</label>
    <div class="drop-zone" id="dropZone" onclick="document.getElementById('fileInput').click()">
      <input type="file" id="fileInput" accept=".zip">
      <div id="dropText">点击选择文件 或 拖拽 .zip 到此处</div>
    </div>
    <div class="progress-bar" id="uploadProgressBar" style="display:none">
      <div class="progress-fill" id="uploadProgress"></div>
    </div>
    <div class="upload-status" id="uploadStatus"></div>
    <div style="margin-top:14px; display:flex; gap:10px; align-items:center">
      <button class="btn btn-primary" id="deployBtn" onclick="startDeploy()">🚀 开始更新</button>
      <span style="font-size:12px;color:#64748b">将使用列表中最新的包</span>
    </div>
  </div>
</div>

<!-- 日志卡片 -->
<div class="card">
  <h2>📋 部署日志
    <button class="btn btn-sm" style="background:#1e3a5f;color:#94a3b8;margin-left:auto"
            onclick="clearLog()">清空</button>
  </h2>
  <div class="log-panel" id="logPanel">
    <div style="color:#475569;font-size:12px">等待操作...</div>
  </div>
</div>

<script>
const logPanel   = document.getElementById('logPanel');
const deployBtn  = document.getElementById('deployBtn');
let   activeTask = null;
let   selectedPkg = null; // 当前选中要部署的包名

// ── 状态刷新 ──────────────────────────────────────────────────────────────────
async function refreshStatus() {
  try {
    const r = await fetch('/api/status');
    const d = await r.json();

    // 服务状态
    const sb = document.getElementById('svcBadge');
    sb.className = `badge badge-${d.serviceStatus}`;
    sb.textContent = { running:'运行中', stopped:'已停止', failed:'异常', not_found:'未安装', unknown:'未知' }[d.serviceStatus] ?? d.serviceStatus;

    // 部署状态
    const db = document.getElementById('deployBadge');
    db.className = `badge badge-${d.deployStatus}`;
    db.textContent = { idle:'空闲', running:'部署中', success:'成功', failed:'失败' }[d.deployStatus] ?? d.deployStatus;

    document.getElementById('lastDeploy').textContent = d.lastDeployedAt || '-';

    // 包列表
    const list = document.getElementById('pkgList');
    if (!d.packages.length) {
      list.innerHTML = '<div style="color:#64748b;font-size:13px">暂无上传的包</div>';
    } else {
      list.innerHTML = d.packages.map(p => `
        <div class="pkg-item" id="pkg_${p.name}">
          <span class="pkg-name" title="${p.name}">${p.name}</span>
          <span class="pkg-meta">${(p.size/1048576).toFixed(1)} MB &nbsp; ${p.time}</span>
          <div class="pkg-actions">
            <button class="btn btn-sm btn-primary" onclick="selectPkg('${p.name}')">选用</button>
            <button class="btn btn-sm btn-danger"  onclick="deletePkg('${p.name}')">删除</button>
          </div>
        </div>`).join('');
    }

    deployBtn.disabled = d.deploying;
    deployBtn.textContent = d.deploying ? '⏳ 部署中...' : '🚀 开始更新';
  } catch(e) {}
}

// ── 选择包 ────────────────────────────────────────────────────────────────────
function selectPkg(name) {
  selectedPkg = name;
  document.querySelectorAll('.pkg-item').forEach(el => {
    el.style.background = el.id === `pkg_${name}` ? '#0f2340' : '#0f172a';
    el.style.borderLeft  = el.id === `pkg_${name}` ? '3px solid #38bdf8' : '3px solid transparent';
  });
  appendLog('info', `已选择包: ${name}`);
}

// ── 删除包 ────────────────────────────────────────────────────────────────────
async function deletePkg(name) {
  if (!confirm(`确定删除 ${name}？`)) return;
  await fetch(`/api/packages/${encodeURIComponent(name)}`, { method: 'DELETE' });
  if (selectedPkg === name) selectedPkg = null;
  refreshStatus();
}

// ── 上传 ──────────────────────────────────────────────────────────────────────
function handleFile(file) {
  if (!file || !file.name.endsWith('.zip')) {
    setUploadStatus('err', '只支持 .zip 文件');
    return;
  }
  document.getElementById('dropText').textContent = `准备上传: ${file.name}`;
  const bar  = document.getElementById('uploadProgressBar');
  const fill = document.getElementById('uploadProgress');
  bar.style.display = 'block';
  fill.style.width  = '0%';
  setUploadStatus('', '上传中...');

  const xhr  = new XMLHttpRequest();
  const data = new FormData();
  data.append('file', file);

  xhr.upload.onprogress = e => {
    if (e.lengthComputable) fill.style.width = (e.loaded/e.total*100).toFixed(1) + '%';
  };
  xhr.onload = () => {
    fill.style.width = '100%';
    if (xhr.status === 200) {
      const res = JSON.parse(xhr.responseText);
      setUploadStatus('ok', `上传成功：${res.fileName}  (${res.sizeMb.toFixed(2)} MB)`);
      document.getElementById('dropText').textContent = '点击选择文件 或 拖拽 .zip 到此处';
      refreshStatus();
    } else {
      setUploadStatus('err', `上传失败: ${xhr.responseText}`);
    }
  };
  xhr.onerror = () => setUploadStatus('err', '上传出错');
  xhr.open('POST', '/api/upload');
  xhr.send(data);
}

document.getElementById('fileInput').addEventListener('change', e => handleFile(e.target.files[0]));

const dz = document.getElementById('dropZone');
dz.addEventListener('dragover',  e => { e.preventDefault(); dz.classList.add('dragover'); });
dz.addEventListener('dragleave', ()=> dz.classList.remove('dragover'));
dz.addEventListener('drop', e => {
  e.preventDefault();
  dz.classList.remove('dragover');
  handleFile(e.dataTransfer.files[0]);
});

function setUploadStatus(cls, msg) {
  const el = document.getElementById('uploadStatus');
  el.className = `upload-status ${cls}`;
  el.textContent = msg;
}

// ── 部署 ──────────────────────────────────────────────────────────────────────
async function startDeploy() {
  if (!confirm('确定要开始部署更新吗？\n部署期间服务将短暂停止。')) return;

  logPanel.innerHTML = '';
  deployBtn.disabled  = true;
  deployBtn.textContent = '⏳ 部署中...';

  const body = selectedPkg ? { fileName: selectedPkg } : {};
  const r = await fetch('/api/deploy', {
    method:  'POST',
    headers: { 'Content-Type': 'application/json' },
    body:    JSON.stringify(body),
  });
  const d = await r.json();
  if (!r.ok) {
    appendLog('error', d.error ?? '启动部署失败');
    deployBtn.disabled = false;
    deployBtn.textContent = '🚀 开始更新';
    return;
  }

  // 订阅 SSE 日志
  activeTask = d.taskId;
  const es = new EventSource(`/api/log/${d.taskId}`);
  es.onmessage = e => {
    const msg = JSON.parse(e.data);
    appendLog(msg.level, msg.msg, msg.time);
    if (msg.level === 'done' || msg.level === 'error') {
      es.close();
      deployBtn.disabled = false;
      deployBtn.textContent = '🚀 开始更新';
      refreshStatus();
    }
  };
  es.onerror = () => {
    es.close();
    deployBtn.disabled = false;
    deployBtn.textContent = '🚀 开始更新';
  };
}

// ── 日志输出 ──────────────────────────────────────────────────────────────────
function appendLog(level, msg, time) {
  const line = document.createElement('div');
  line.className = 'log-line';
  const t = time || new Date().toTimeString().slice(0,8);
  line.innerHTML = `<span class="log-time">${t}</span><span class="log-${level}">${escHtml(msg)}</span>`;
  logPanel.appendChild(line);
  logPanel.scrollTop = logPanel.scrollHeight;
}

function clearLog() { logPanel.innerHTML = ''; }
function escHtml(s) { return s.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;'); }

// 初始化 + 定时刷新
refreshStatus();
setInterval(refreshStatus, 3000);
</script>
</body>
</html>
""";
}
