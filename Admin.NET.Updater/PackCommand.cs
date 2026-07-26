using System.IO.Compression;

namespace AdminNetUpdater;

/// <summary>
/// upgrader pack
///
/// 从 publish 目录收集 Admin.NET.LvKong.Application.* 三个文件，
/// 从 Web/dist 收集前端产物，打包成带时间戳的 zip 文件。
///
/// zip 内部结构：
///   backend/
///     Admin.NET.LvKong.Application.dll
///     Admin.NET.LvKong.Application.pdb
///     Admin.NET.LvKong.Application.xml
///   frontend/
///     (dist 目录下的所有内容)
/// </summary>
public static class PackCommand
{
    // 需要打包的后端文件前缀
    private const string BackendPrefix = "Admin.NET.LvKong.Application";

    public static int Run(Config cfg)
    {
        Log.Step("Pack — 开始打包");

        // 1. 验证源目录
        var publishDir = cfg.ResolvedPublishDir;
        var distDir    = cfg.ResolvedWebDistDir;

        if (!Directory.Exists(publishDir))
        {
            Log.Error($"publish 目录不存在: {publishDir}");
            Log.Error("请先执行 dotnet publish 再运行 pack 命令");
            return 1;
        }

        if (!Directory.Exists(distDir))
        {
            Log.Error($"Web/dist 目录不存在: {distDir}");
            Log.Error("请先执行 npm run build 再运行 pack 命令");
            return 1;
        }

        // 2. 收集后端文件
        var backendFiles = Directory
            .EnumerateFiles(publishDir)
            .Where(f => Path.GetFileName(f).StartsWith(BackendPrefix,
                StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (backendFiles.Count == 0)
        {
            Log.Error($"在 {publishDir} 中找不到 {BackendPrefix}.* 文件");
            return 1;
        }

        Log.Info($"找到后端文件 {backendFiles.Count} 个：");
        foreach (var f in backendFiles)
            Log.Info($"  {Path.GetFileName(f)}");

        // 3. 准备输出目录和文件名
        var outputDir = cfg.ResolvedPackageOutputDir;
        Directory.CreateDirectory(outputDir);

        var timestamp  = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var zipName    = $"AdminNET_package_{timestamp}.zip";
        var zipPath    = Path.Combine(outputDir, zipName);

        Log.Info($"打包目标: {zipPath}");

        // 4. 创建 zip
        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            // 4a. 后端文件 → backend/
            Log.Step("打包后端文件 → backend/");
            foreach (var file in backendFiles)
            {
                var entryName = Path.Combine("backend", Path.GetFileName(file))
                    .Replace('\\', '/');
                zip.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
                Log.Ok($"  + {entryName}");
            }

            // 4b. 前端 dist → frontend/
            Log.Step("打包前端文件 → frontend/");
            AddDirectory(zip, distDir, "frontend");
        }

        Log.Done($"打包完成: {zipPath}");
        Log.Info($"文件大小: {new FileInfo(zipPath).Length / 1024.0 / 1024.0:F2} MB");
        return 0;
    }

    /// <summary>
    /// 递归将目录添加到 zip，entryBase 为 zip 内的目录前缀
    /// </summary>
    private static void AddDirectory(ZipArchive zip, string srcDir, string entryBase)
    {
        foreach (var file in Directory.EnumerateFiles(srcDir, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(srcDir, file);
            var entryName    = Path.Combine(entryBase, relativePath).Replace('\\', '/');
            zip.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
        }
        Log.Ok($"  + {entryBase}/ ({CountFiles(srcDir)} 个文件)");
    }

    private static int CountFiles(string dir)
        => Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length;
}
