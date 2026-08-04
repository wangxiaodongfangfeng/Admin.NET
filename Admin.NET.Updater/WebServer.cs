using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
#if WINDOWS
using Microsoft.Extensions.Hosting.WindowsServices;
#endif

namespace AdminNetUpdater;

/// <summary>
/// 内嵌 HTTP 服务器
///   GET  /                  → 管理 UI（从 wwwroot/index.html 读取）
///   GET  /api/status        → 服务状态 + 包列表
///   POST /api/upload        → 上传 zip（multipart/form-data, field: file）
///   POST /api/deploy        → 触发部署（需密码验证）
///   GET  /api/log/{taskId}  → SSE 实时日志流
///   DELETE /api/packages/{name} → 删除已上传的包
/// </summary>
public static class WebServer
{
    private static volatile bool   _deploying      = false;
    private static volatile string _lastStatus     = "idle";
    private static string          _lastDeployedAt = "";

    private static readonly ConcurrentDictionary<string, ConcurrentQueue<string?>> _subscribers = new();
    private static readonly ConcurrentDictionary<string, (int failures, DateTime lockUntil)>  _bruteGuard  = new();

    private const int MaxFailures = 3;
    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(5);

    // ── 启动 ──────────────────────────────────────────────────────────────────

    public static async Task Run(Config cfg, string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

#if WINDOWS
        if (WindowsServiceHelpers.IsWindowsService())
        {
            builder.Host.UseWindowsService(options =>
                options.ServiceName = cfg.UpdaterServiceName);
        }
#endif

        builder.WebHost.UseUrls($"http://*:{cfg.WebServerPort}");
        builder.Logging.ClearProviders();
#if WINDOWS
        if (!WindowsServiceHelpers.IsWindowsService())
            builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Warning);
#else
        builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Warning);
#endif

        var app = builder.Build();

        // ── GET / ─────────────────────────────────────────────────────────────
        app.MapGet("/", () =>
            Results.Content(BuildHtml(cfg), "text/html; charset=utf-8"));

        // ── GET /api/status ───────────────────────────────────────────────────
        app.MapGet("/api/status", () =>
        {
            var dir = cfg.ResolvedDeployPackageDir;
            var packages = Directory.Exists(dir)
                ? Directory.EnumerateFiles(dir, "AdminNET_package_*.zip")
                    .OrderByDescending(File.GetLastWriteTime)
                    .Select(f => new
                    {
                        name = Path.GetFileName(f),
                        size = new FileInfo(f).Length,
                        time = File.GetLastWriteTime(f).ToString("yyyy-MM-dd HH:mm:ss"),
                    })
                    .ToList<object>()
                : new List<object>();

            return Results.Json(new
            {
                deployStatus   = _lastStatus,
                deploying      = _deploying,
                lastDeployedAt = _lastDeployedAt,
                serviceStatus  = ServiceControl.GetStatus(cfg.ServiceName),
                serviceName    = cfg.ServiceName,
                packages,
            });
        });

        // ── POST /api/upload ──────────────────────────────────────────────────
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

            var baseName = Path.GetFileNameWithoutExtension(file.FileName);
            var saveName = $"{baseName}_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
            var savePath = Path.Combine(saveDir, saveName);

            await using var dest = File.Create(savePath);
            await file.CopyToAsync(dest);

            var sizeMb = new FileInfo(savePath).Length / 1048576.0;
            Console.WriteLine($"[Upload] {saveName}  ({sizeMb:F2} MB)");

            return Results.Json(new { success = true, fileName = saveName, sizeMb });
        });

        // ── DELETE /api/packages/{name} ───────────────────────────────────────
        app.MapDelete("/api/packages/{name}", (string name) =>
        {
            if (name.Contains("..") || name.Contains('/') || name.Contains('\\'))
                return Results.BadRequest(new { error = "非法文件名" });

            var path = Path.Combine(cfg.ResolvedDeployPackageDir, name);
            if (!File.Exists(path))
                return Results.NotFound(new { error = "文件不存在" });

            File.Delete(path);
            return Results.Json(new { success = true });
        });

        // ── POST /api/deploy ──────────────────────────────────────────────────
        app.MapPost("/api/deploy", ([FromBody] DeployRequest? req, HttpContext ctx) =>
        {
            if (_deploying)
                return Results.Json(new { error = "正在部署中，请稍候" }, statusCode: 409);

            // 密码验证
            var clientIp   = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var storedHash = cfg.DeployPasswordHash;

            if (!string.IsNullOrWhiteSpace(storedHash))
            {
                if (_bruteGuard.TryGetValue(clientIp, out var guard) && guard.lockUntil > DateTime.Now)
                {
                    var remaining = (int)(guard.lockUntil - DateTime.Now).TotalSeconds;
                    return Results.Json(
                        new { error = $"密码错误次数过多，请 {remaining} 秒后再试" },
                        statusCode: 429);
                }

                if (string.IsNullOrWhiteSpace(req?.password))
                    return Results.Json(new { error = "请输入部署密码", requirePassword = true }, statusCode: 401);

                if (!PasswordCommand.Verify(req.password, storedHash))
                {
                    _bruteGuard.AddOrUpdate(clientIp,
                        _ => (1, 1 >= MaxFailures ? DateTime.Now + LockDuration : DateTime.MinValue),
                        (_, old) =>
                        {
                            var n = old.failures + 1;
                            return (n, n >= MaxFailures ? DateTime.Now + LockDuration : DateTime.MinValue);
                        });

                    var cur  = _bruteGuard.TryGetValue(clientIp, out var g) ? g.failures : 1;
                    var left = MaxFailures - cur;
                    var msg  = left > 0
                        ? $"密码错误，还剩 {left} 次机会"
                        : $"密码错误次数过多，已锁定 {(int)LockDuration.TotalMinutes} 分钟";
                    return Results.Json(new { error = msg, requirePassword = true }, statusCode: 401);
                }

                _bruteGuard.TryRemove(clientIp, out _);
            }

            // 确定 zip
            string? zipPath;
            if (!string.IsNullOrWhiteSpace(req?.fileName))
            {
                zipPath = Path.Combine(cfg.ResolvedDeployPackageDir, req.fileName);
                if (!File.Exists(zipPath))
                    return Results.Json(new { error = $"包 [{req.fileName}] 不存在" }, statusCode: 400);
            }
            else
            {
                zipPath = DeployCommand.FindLatestPackage(cfg.ResolvedDeployPackageDir);
                if (zipPath == null)
                    return Results.Json(new { error = "找不到任何更新包，请先上传" }, statusCode: 400);
            }

            var taskId = Guid.NewGuid().ToString("N")[..8];
            var queue  = new ConcurrentQueue<string?>();
            _subscribers[taskId] = queue;

            _deploying  = true;
            _lastStatus = "running";

            var captured = zipPath;
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
                    var success     = await DeployCommand.RunFromApi(cfg, captured, Emit);
                    _lastStatus     = success ? "success" : "failed";
                    _lastDeployedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    Emit(success ? "done" : "error", success ? "部署完成" : "部署失败");
                }
                finally
                {
                    _deploying = false;
                    queue.Enqueue(null);
                }
            });

            return Results.Json(new { taskId });
        });

        // ── GET /api/log/{taskId}  SSE ────────────────────────────────────────
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
                        if (line == null) break;
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
            finally { _subscribers.TryRemove(taskId, out _); }
        });

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n  Admin.NET Updater Web Server 已启动");
        Console.WriteLine($"  访问地址: http://localhost:{cfg.WebServerPort}");
        Console.WriteLine($"  按 Ctrl+C 停止\n");
        Console.ResetColor();

        await app.RunAsync();
    }

    private record DeployRequest(string? fileName, string? password);

    // ── HTML 加载 ─────────────────────────────────────────────────────────────

    /// <summary>
    /// 从 wwwroot/index.html 读取页面并替换模板变量。
    /// 查找顺序：exe 目录 → 当前工作目录。
    /// </summary>
    private static string BuildHtml(Config cfg)
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "wwwroot", "index.html"),
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html"),
        };

        var path = candidates.FirstOrDefault(File.Exists);
        if (path == null)
        {
            return "<html><body style='font-family:sans-serif;padding:2rem;background:#0f172a;color:#f87171'>"
                 + "<h2>⚠ 找不到 wwwroot/index.html</h2>"
                 + "<p>请确保 wwwroot/index.html 与 upgrader 可执行文件在同一目录。</p>"
                 + "</body></html>";
        }

        return File.ReadAllText(path)
            .Replace("{{SERVICE_NAME}}", HtmlEncode(cfg.ServiceName))
            .Replace("{{SERVICE_DIR}}",  HtmlEncode(cfg.ServiceDir));
    }

    private static string HtmlEncode(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}
