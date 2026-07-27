using System.IO.Compression;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;

namespace AdminNetUpdater;

/// <summary>
/// upgrader deploy — CLI 入口 + API 共用核心逻辑
/// </summary>
public static class DeployCommand
{
    private const string BackendPrefix = "Admin.NET.LvKong.Application";

    // ── CLI 入口 ──────────────────────────────────────────────────────────────

    /// <summary>CLI 调用入口，日志输出到控制台</summary>
    public static int Run(Config cfg)
    {

        // 1. 下载（如果配置了远程地址）
        if (!string.IsNullOrWhiteSpace(cfg.PackageDownloadUrl))
        {
            if (!DownloadPackage(cfg, out var downloadedPath,
                    progress: (pct, dl, total) =>
                    {
                        if (total > 0)
                            Console.Write($"\r  进度: {pct:F1}%  ({dl / 1048576.0:F1} / {total / 1048576.0:F1} MB)   ");
                        else
                            Console.Write($"\r  已下载: {dl / 1048576.0:F1} MB   ");
                    }))
                return 1;
            Console.WriteLine();
        }
        else
        {
            Log.Info("PackageDownloadUrl 未配置，跳过下载，直接使用本地包");
        }

        // 2. 执行 deploy 核心逻辑
        bool success = RunCore(cfg,
            log:  (level, msg) => LogToConsole(level, msg),
            zipOverride: null);

        return success ? 0 : 1;
    }

    // ── API 调用入口 ──────────────────────────────────────────────────────────

    /// <summary>
    /// API 调用入口：使用指定的 zip 文件执行 deploy，日志通过回调推送。
    /// </summary>
    /// <param name="cfg">配置</param>
    /// <param name="zipPath">要部署的 zip 文件路径（API 上传后的临时路径）</param>
    /// <param name="logCallback">日志回调：(level, message) — level: info/ok/warn/error/step/done</param>
    public static Task<bool> RunFromApi(Config cfg, string zipPath, Action<string, string> logCallback)
    {
        return Task.Run(() => RunCore(cfg, logCallback, zipPath));
    }

    // ── 核心逻辑 ──────────────────────────────────────────────────────────────

    private static bool RunCore(Config cfg, Action<string, string> log, string? zipOverride)
    {
        log("step", "Deploy — 开始部署");

        // 1. 确定要使用的 zip
        string? zipPath = zipOverride;
        if (zipPath == null)
        {
            var saveDir = cfg.ResolvedDownloadSaveDir;
            zipPath = FindLatestPackage(saveDir)
                   ?? FindLatestPackage(cfg.ResolvedDeployPackageDir);

            if (zipPath == null)
            {
                log("error", $"找不到任何 AdminNET_package_*.zip，已搜索: {saveDir}");
                return false;
            }
        }
        log("info", $"使用包: {Path.GetFileName(zipPath)}");

        var serviceDir  = cfg.ServiceDir;
        var serviceName = cfg.ServiceName;

        if (!Directory.Exists(serviceDir))
        {
            log("error", $"服务目录不存在: {serviceDir}");
            return false;
        }

        // 2. 解压
        var tempDir = Path.Combine(Path.GetTempPath(), $"AdminNET_deploy_{DateTime.Now:yyyyMMdd_HHmmss}");
        log("step", $"解压到临时目录: {tempDir}");
        try { ZipFile.ExtractToDirectory(zipPath, tempDir); }
        catch (Exception ex) { log("error", $"解压失败: {ex.Message}"); return false; }
        log("ok", "解压完成");

        var backendDir  = Path.Combine(tempDir, "backend");
        var frontendDir = Path.Combine(tempDir, "frontend");

        if (!Directory.Exists(backendDir))
        {
            log("error", "zip 包中缺少 backend/ 目录，包文件可能损坏");
            Cleanup(tempDir, log); return false;
        }
        if (!Directory.Exists(frontendDir))
        {
            log("error", "zip 包中缺少 frontend/ 目录，包文件可能损坏");
            Cleanup(tempDir, log); return false;
        }

        // 3. 停止服务
        log("step", $"停止服务: {serviceName}");
        if (!ServiceControl.Stop(serviceName, log)) { Cleanup(tempDir, log); return false; }

        try
        {
            // 4. 备份
            log("step", "备份原文件");
            var backupDir = CreateBackup(cfg, serviceDir, log);
            log("ok", $"备份目录: {backupDir}");

            // 5. 替换后端
            log("step", "替换后端文件");
            var backendFiles = Directory.GetFiles(backendDir);
            if (backendFiles.Length == 0)
            {
                log("warn", "backend/ 目录为空，跳过后端更新");
            }
            else
            {
                foreach (var src in backendFiles)
                {
                    File.Copy(src, Path.Combine(serviceDir, Path.GetFileName(src)), overwrite: true);
                    log("ok", $"  ← {Path.GetFileName(src)}");
                }
            }

            // 6. 更新前端 wwwroot
            log("step", "更新前端 wwwroot");
            var wwwrootDir = Path.Combine(serviceDir, "wwwroot");
            Directory.CreateDirectory(wwwrootDir);

            log("info", "清理 wwwroot（保留 upload/）...");
            FileHelper.CleanDirectory(wwwrootDir, keepDirNames: new[] { "upload" });
            log("ok", "清理完成");

            log("info", "复制新前端文件...");
            FileHelper.CopyDirectory(frontendDir, wwwrootDir);
            var count = Directory.GetFiles(frontendDir, "*", SearchOption.AllDirectories).Length;
            log("ok", $"复制完成，共 {count} 个文件");
        }
        catch (Exception ex)
        {
            log("error", $"部署过程中出错: {ex.Message}");
            log("warn", "文件替换可能不完整，请检查后手动启动服务");
            Cleanup(tempDir, log);
            ServiceControl.Start(serviceName, log);
            return false;
        }

        // 7. 启动服务
        log("step", $"启动服务: {serviceName}");
        if (!ServiceControl.Start(serviceName, log)) { Cleanup(tempDir, log); return false; }

        // 8. 清理临时目录
        Cleanup(tempDir, log);

        log("done", $"部署成功！服务 [{serviceName}] 已运行");
        return true;
    }

    // ── 下载（CLI 使用） ──────────────────────────────────────────────────────

    public static bool DownloadPackage(Config cfg, out string downloadedPath,
        Action<double, long, long>? progress = null)
    {
        downloadedPath = null!;
        var url     = cfg.PackageDownloadUrl;
        var saveDir = cfg.ResolvedDownloadSaveDir;
        Directory.CreateDirectory(saveDir);

        var urlFileName = Path.GetFileName(new Uri(url).LocalPath);
        if (string.IsNullOrWhiteSpace(urlFileName) || !urlFileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            urlFileName = $"AdminNET_package_{DateTime.Now:yyyyMMdd_HHmmss}.zip";

        var savePath = Path.Combine(saveDir, urlFileName);
        Log.Info($"  URL : {url}");
        Log.Info($"  保存: {savePath}");

        try
        {
            using var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            };
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(cfg.DownloadTimeoutSeconds) };

            if (!string.IsNullOrWhiteSpace(cfg.DownloadUsername))
            {
                var cred = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{cfg.DownloadUsername}:{cfg.DownloadPassword}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", cred);
            }

            using var response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            using var src  = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
            using var dest = File.Create(savePath);

            var buf  = new byte[81920];
            long dl  = 0;
            int  read;
            var  last = DateTime.Now;

            while ((read = src.Read(buf, 0, buf.Length)) > 0)
            {
                dest.Write(buf, 0, read);
                dl += read;
                if ((DateTime.Now - last).TotalSeconds >= 0.5 && progress != null)
                {
                    var pct = totalBytes > 0 ? dl * 100.0 / totalBytes : 0;
                    progress(pct, dl, totalBytes);
                    last = DateTime.Now;
                }
            }
            progress?.Invoke(100, dl, dl);

            Log.Ok($"下载完成: {savePath}  ({new FileInfo(savePath).Length / 1048576.0:F2} MB)");
            downloadedPath = savePath;
            return true;
        }
        catch (TaskCanceledException) { Log.Error($"下载超时（{cfg.DownloadTimeoutSeconds}s）"); return false; }
        catch (Exception ex)          { Log.Error($"下载失败: {ex.Message}"); return false; }
    }

    // ── 辅助方法 ──────────────────────────────────────────────────────────────

    public static string? FindLatestPackage(string dir)
    {
        if (!Directory.Exists(dir)) return null;
        return Directory.EnumerateFiles(dir, "AdminNET_package_*.zip")
            .OrderByDescending(File.GetLastWriteTime)
            .FirstOrDefault();
    }

    private static string CreateBackup(Config cfg, string serviceDir, Action<string, string> log)
    {
        var bakDir = Path.Combine(cfg.ResolvedBackupBaseDir, $"AppBak{DateTime.Now:yyyyMMdd}");
        Directory.CreateDirectory(bakDir);

        foreach (var f in Directory.EnumerateFiles(serviceDir)
            .Where(f => Path.GetFileName(f).StartsWith(BackendPrefix, StringComparison.OrdinalIgnoreCase)))
        {
            FileHelper.CopyFileTo(f, Path.Combine(bakDir, "backend"));
            log("info", $"  backed up: {Path.GetFileName(f)}");
        }

        var wwwSrc = Path.Combine(serviceDir, "wwwroot");
        if (Directory.Exists(wwwSrc))
        {
            FileHelper.CopyDirectory(wwwSrc, Path.Combine(bakDir, "wwwroot"));
            log("info", "  backed up: wwwroot/");
        }
        return bakDir;
    }

    private static void Cleanup(string tempDir, Action<string, string> log)
    {
        try { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); }
        catch { log("warn", $"临时目录清理失败: {tempDir}"); }
    }

    private static void LogToConsole(string level, string msg)
    {
        switch (level)
        {
            case "step": Log.Step(msg); break;
            case "ok":   Log.Ok(msg);   break;
            case "warn": Log.Warn(msg); break;
            case "error":Log.Error(msg);break;
            case "done": Log.Done(msg); break;
            default:     Log.Info(msg); break;
        }
    }
}
