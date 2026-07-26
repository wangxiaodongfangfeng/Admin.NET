using System.IO.Compression;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;

namespace AdminNetUpdater;

/// <summary>
/// upgrader deploy
///
/// 流程：
///   1. 若配置了 PackageDownloadUrl，从远程下载最新 zip
///   2. 找到本地最新 zip
///   3. 停止 Windows 服务
///   4. 备份原服务目录
///   5. 替换后端 dll/pdb/xml
///   6. 清理 wwwroot（保留 upload/），复制新前端
///   7. 启动 Windows 服务
/// </summary>
public static class DeployCommand
{
    private const string BackendPrefix = "Admin.NET.LvKong.Application";
    private static readonly TimeSpan ServiceTimeout = TimeSpan.FromSeconds(60);

    public static int Run(Config cfg)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Log.Error("deploy 命令仅支持 Windows 平台（需要操作 Windows 服务）");
            return 1;
        }

        Log.Step("Deploy — 开始部署");

        // 1. 下载更新包（如果配置了远程地址）────────────────────────────────
        if (!string.IsNullOrWhiteSpace(cfg.PackageDownloadUrl))
        {
            if (!DownloadPackage(cfg, out var downloadedPath))
                return 1;
            Log.Ok($"已下载: {downloadedPath}");
        }
        else
        {
            Log.Info("PackageDownloadUrl 未配置，跳过下载，直接使用本地包");
        }

        // 2. 找本地最新 zip ──────────────────────────────────────────────────
        var saveDir = cfg.ResolvedDownloadSaveDir;
        var zipPath = FindLatestPackage(saveDir);
        if (zipPath == null)
        {
            // 也搜 DeployPackageDir（两者可能不同）
            zipPath = FindLatestPackage(cfg.ResolvedDeployPackageDir);
        }
        if (zipPath == null)
        {
            Log.Error($"找不到任何 AdminNET_package_*.zip");
            Log.Error($"  已搜索: {saveDir}");
            if (saveDir != cfg.ResolvedDeployPackageDir)
                Log.Error($"  已搜索: {cfg.ResolvedDeployPackageDir}");
            return 1;
        }
        Log.Info($"使用包: {Path.GetFileName(zipPath)}");

        var serviceDir  = cfg.ServiceDir;
        var serviceName = cfg.ServiceName;

        if (!Directory.Exists(serviceDir))
        {
            Log.Error($"服务目录不存在: {serviceDir}");
            return 1;
        }

        // 3. 解压 zip 到临时目录 ─────────────────────────────────────────────
        var tempDir = Path.Combine(Path.GetTempPath(), $"AdminNET_deploy_{DateTime.Now:yyyyMMdd_HHmmss}");
        Log.Step($"解压到临时目录: {tempDir}");
        ZipFile.ExtractToDirectory(zipPath, tempDir);
        Log.Ok("解压完成");

        var backendStagingDir  = Path.Combine(tempDir, "backend");
        var frontendStagingDir = Path.Combine(tempDir, "frontend");

        if (!Directory.Exists(backendStagingDir))
        {
            Log.Error("zip 包中缺少 backend/ 目录，包文件可能损坏");
            Cleanup(tempDir);
            return 1;
        }
        if (!Directory.Exists(frontendStagingDir))
        {
            Log.Error("zip 包中缺少 frontend/ 目录，包文件可能损坏");
            Cleanup(tempDir);
            return 1;
        }

        // 4. 停止服务 ────────────────────────────────────────────────────────
        Log.Step($"停止服务: {serviceName}");
        if (!StopService(serviceName))
        {
            Cleanup(tempDir);
            return 1;
        }

        try
        {
            // 5. 备份 ────────────────────────────────────────────────────────
            Log.Step("备份原文件");
            var backupDir = CreateBackup(cfg, serviceDir);
            Log.Ok($"备份目录: {backupDir}");

            // 6a. 替换后端文件 ─────────────────────────────────────────────
            Log.Step("替换后端文件");
            var backendFiles = Directory.GetFiles(backendStagingDir);
            if (backendFiles.Length == 0)
            {
                Log.Warn("backend/ 目录为空，跳过后端更新");
            }
            else
            {
                foreach (var src in backendFiles)
                {
                    var dest = Path.Combine(serviceDir, Path.GetFileName(src));
                    File.Copy(src, dest, overwrite: true);
                    Log.Ok($"  ← {Path.GetFileName(src)}");
                }
            }

            // 6b. 更新前端 wwwroot ─────────────────────────────────────────
            Log.Step("更新前端 wwwroot");
            var wwwrootDir = Path.Combine(serviceDir, "wwwroot");
            Directory.CreateDirectory(wwwrootDir);

            Log.Info("清理 wwwroot（保留 upload/）...");
            FileHelper.CleanDirectory(wwwrootDir, keepDirNames: new[] { "upload" });
            Log.Ok("清理完成");

            Log.Info("复制新前端文件...");
            FileHelper.CopyDirectory(frontendStagingDir, wwwrootDir);
            var fileCount = Directory.GetFiles(frontendStagingDir, "*", SearchOption.AllDirectories).Length;
            Log.Ok($"复制完成，共 {fileCount} 个文件");
        }
        catch (Exception ex)
        {
            Log.Error($"部署过程中出错: {ex.Message}");
            Log.Warn("文件替换可能不完整，请检查后手动启动服务");
            Cleanup(tempDir);
            StartService(serviceName);
            return 1;
        }

        // 7. 启动服务 ────────────────────────────────────────────────────────
        Log.Step($"启动服务: {serviceName}");
        if (!StartService(serviceName))
        {
            Cleanup(tempDir);
            return 1;
        }

        // 8. 清理临时目录 ────────────────────────────────────────────────────
        Cleanup(tempDir);

        Log.Done($"部署成功！服务 [{serviceName}] 已运行");
        return 0;
    }

    // ── 下载 ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// 从 PackageDownloadUrl 下载 zip，保存到 ResolvedDownloadSaveDir。
    /// 返回 true 时 downloadedPath 为本地路径，false 时为 null。
    /// </summary>
    private static bool DownloadPackage(Config cfg, out string downloadedPath)
    {
        downloadedPath = null!;

        var url     = cfg.PackageDownloadUrl;
        var saveDir = cfg.ResolvedDownloadSaveDir;
        Directory.CreateDirectory(saveDir);

        // 从 URL 解析文件名，无法解析时用时间戳命名
        var urlFileName = Path.GetFileName(new Uri(url).LocalPath);
        if (string.IsNullOrWhiteSpace(urlFileName) || !urlFileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            urlFileName = $"AdminNET_package_{DateTime.Now:yyyyMMdd_HHmmss}.zip";

        var savePath = Path.Combine(saveDir, urlFileName);

        Log.Step($"下载更新包");
        Log.Info($"  URL : {url}");
        Log.Info($"  保存: {savePath}");

        try
        {
            using var handler = new HttpClientHandler
            {
                // 允许自签证书（内网服务器常见），生产环境可按需改为 false
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            };

            using var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(cfg.DownloadTimeoutSeconds),
            };

            // Basic Auth
            if (!string.IsNullOrWhiteSpace(cfg.DownloadUsername))
            {
                var credentials = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{cfg.DownloadUsername}:{cfg.DownloadPassword}"));
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", credentials);
                Log.Info($"  认证: Basic ({cfg.DownloadUsername})");
            }

            // 带进度显示的下载
            using var response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;
            var totalMb    = totalBytes.HasValue ? $"{totalBytes.Value / 1024.0 / 1024.0:F1} MB" : "未知大小";
            Log.Info($"  文件大小: {totalMb}");

            using var srcStream  = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
            using var destStream = File.Create(savePath);

            var buffer      = new byte[81920]; // 80 KB chunks
            long downloaded = 0;
            int  read;
            var  lastReport = DateTime.Now;

            while ((read = srcStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                destStream.Write(buffer, 0, read);
                downloaded += read;

                // 每秒更新一次进度
                if ((DateTime.Now - lastReport).TotalSeconds >= 1)
                {
                    if (totalBytes.HasValue)
                    {
                        var pct = downloaded * 100.0 / totalBytes.Value;
                        Console.Write($"\r  进度: {pct:F1}%  ({downloaded / 1024.0 / 1024.0:F1} / {totalBytes.Value / 1024.0 / 1024.0:F1} MB)   ");
                    }
                    else
                    {
                        Console.Write($"\r  已下载: {downloaded / 1024.0 / 1024.0:F1} MB   ");
                    }
                    lastReport = DateTime.Now;
                }
            }

            Console.WriteLine(); // 换行，结束进度行
            Log.Ok($"下载完成: {savePath}  ({new FileInfo(savePath).Length / 1024.0 / 1024.0:F2} MB)");
            downloadedPath = savePath;
            return true;
        }
        catch (TaskCanceledException)
        {
            Log.Error($"下载超时（超过 {cfg.DownloadTimeoutSeconds} 秒）");
            return false;
        }
        catch (Exception ex)
        {
            Log.Error($"下载失败: {ex.Message}");
            return false;
        }
    }

    // ── 私有辅助 ──────────────────────────────────────────────────────────────

    private static string? FindLatestPackage(string dir)
    {
        if (!Directory.Exists(dir)) return null;
        return Directory
            .EnumerateFiles(dir, "AdminNET_package_*.zip")
            .OrderByDescending(f => File.GetLastWriteTime(f))
            .FirstOrDefault();
    }

    private static string CreateBackup(Config cfg, string serviceDir)
    {
        var bakBase = cfg.ResolvedBackupBaseDir;
        var bakName = $"AppBak{DateTime.Now:yyyyMMdd}";
        var bakDir  = Path.Combine(bakBase, bakName);
        Directory.CreateDirectory(bakDir);

        foreach (var file in Directory.EnumerateFiles(serviceDir)
            .Where(f => Path.GetFileName(f).StartsWith(BackendPrefix, StringComparison.OrdinalIgnoreCase)))
        {
            FileHelper.CopyFileTo(file, Path.Combine(bakDir, "backend"));
            Log.Info($"  backed up: {Path.GetFileName(file)}");
        }

        var wwwrootSrc = Path.Combine(serviceDir, "wwwroot");
        if (Directory.Exists(wwwrootSrc))
        {
            FileHelper.CopyDirectory(wwwrootSrc, Path.Combine(bakDir, "wwwroot"));
            Log.Info("  backed up: wwwroot/");
        }

        return bakDir;
    }

    private static bool StopService(string name)
    {
        try
        {
            using var sc = new ServiceController(name);
            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                Log.Info($"服务 [{name}] 已停止，无需操作");
                return true;
            }
            Log.Info($"正在停止服务 [{name}]（当前状态: {sc.Status}）...");
            sc.Stop();
            sc.WaitForStatus(ServiceControllerStatus.Stopped, ServiceTimeout);
            Log.Ok($"服务 [{name}] 已停止");
            return true;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("does not exist"))
        {
            Log.Warn($"服务 [{name}] 不存在，跳过停止步骤");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"停止服务失败: {ex.Message}");
            return false;
        }
    }

    private static bool StartService(string name)
    {
        try
        {
            using var sc = new ServiceController(name);
            if (sc.Status == ServiceControllerStatus.Running)
            {
                Log.Info($"服务 [{name}] 已在运行");
                return true;
            }
            Log.Info($"正在启动服务 [{name}]...");
            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running, ServiceTimeout);
            Log.Ok($"服务 [{name}] 已启动");
            return true;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("does not exist"))
        {
            Log.Warn($"服务 [{name}] 不存在，跳过启动步骤");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"启动服务失败: {ex.Message}");
            return false;
        }
    }

    private static void Cleanup(string tempDir)
    {
        try
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
        catch
        {
            Log.Warn($"临时目录清理失败: {tempDir}，可手动删除");
        }
    }
}

