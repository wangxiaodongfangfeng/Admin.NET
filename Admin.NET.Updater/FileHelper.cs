namespace AdminNetUpdater;

public static class FileHelper
{
    /// <summary>
    /// 递归复制目录，可选排除某些子目录名
    /// </summary>
    public static void CopyDirectory(string src, string dest,
        IEnumerable<string>? excludeDirNames = null)
    {
        var excludeSet = new HashSet<string>(
            excludeDirNames ?? Enumerable.Empty<string>(),
            StringComparer.OrdinalIgnoreCase);

        Directory.CreateDirectory(dest);

        foreach (var file in Directory.EnumerateFiles(src))
        {
            var destFile = Path.Combine(dest, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite: true);
        }

        foreach (var dir in Directory.EnumerateDirectories(src))
        {
            var dirName = Path.GetFileName(dir);
            if (excludeSet.Contains(dirName)) continue;
            CopyDirectory(dir, Path.Combine(dest, dirName), excludeDirNames);
        }
    }

    /// <summary>
    /// 删除目录中除指定子目录名外的所有内容（文件 + 子目录）
    /// </summary>
    public static void CleanDirectory(string dir, IEnumerable<string>? keepDirNames = null)
    {
        if (!Directory.Exists(dir)) return;

        var keepSet = new HashSet<string>(
            keepDirNames ?? Enumerable.Empty<string>(),
            StringComparer.OrdinalIgnoreCase);

        foreach (var file in Directory.EnumerateFiles(dir))
            File.Delete(file);

        foreach (var subDir in Directory.EnumerateDirectories(dir))
        {
            if (keepSet.Contains(Path.GetFileName(subDir))) continue;
            Directory.Delete(subDir, recursive: true);
        }
    }

    /// <summary>
    /// 复制单个文件到目标目录（保留文件名）
    /// </summary>
    public static void CopyFileTo(string srcFile, string destDir)
    {
        Directory.CreateDirectory(destDir);
        File.Copy(srcFile, Path.Combine(destDir, Path.GetFileName(srcFile)), overwrite: true);
    }
}
