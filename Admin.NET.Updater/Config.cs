using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdminNetUpdater;

/// <summary>
/// updater.json 配置映射
/// </summary>
public class Config
{
    // ── pack ─────────────────────────────────────────────────────────────────
    [JsonPropertyName("PublishDir")]
    public string PublishDir { get; set; } = "../publish";

    [JsonPropertyName("WebDistDir")]
    public string WebDistDir { get; set; } = "../Web/dist";

    [JsonPropertyName("PackageOutputDir")]
    public string PackageOutputDir { get; set; } = "../packages";

    // ── deploy ───────────────────────────────────────────────────────────────
    [JsonPropertyName("PackageDownloadUrl")]
    public string PackageDownloadUrl { get; set; } = "";

    [JsonPropertyName("DownloadUsername")]
    public string DownloadUsername { get; set; } = "";

    [JsonPropertyName("DownloadPassword")]
    public string DownloadPassword { get; set; } = "";

    [JsonPropertyName("DownloadTimeoutSeconds")]
    public int DownloadTimeoutSeconds { get; set; } = 300;

    [JsonPropertyName("DownloadSaveDir")]
    public string DownloadSaveDir { get; set; } = "";

    [JsonPropertyName("DeployPackageDir")]
    public string DeployPackageDir { get; set; } = "../packages";

    [JsonPropertyName("ServiceDir")]
    public string ServiceDir { get; set; } = @"C:\Users\Administrator\Documents\publish";

    [JsonPropertyName("ServiceName")]
    public string ServiceName { get; set; } = "AdminNETService";

    [JsonPropertyName("BackupBaseDir")]
    public string BackupBaseDir { get; set; } = "";

    // ── helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// 将配置中的路径解析为绝对路径（相对路径基于 exe 所在目录）
    /// </summary>
    public string Resolve(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return path;
        if (Path.IsPathRooted(path)) return path;
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));
    }

    public string ResolvedPublishDir      => Resolve(PublishDir);
    public string ResolvedWebDistDir      => Resolve(WebDistDir);
    public string ResolvedPackageOutputDir => Resolve(PackageOutputDir);
    public string ResolvedDeployPackageDir => Resolve(DeployPackageDir);
    public string ResolvedDownloadSaveDir  =>
        string.IsNullOrWhiteSpace(DownloadSaveDir)
            ? ResolvedDeployPackageDir
            : Resolve(DownloadSaveDir);
    public string ResolvedBackupBaseDir   =>
        string.IsNullOrWhiteSpace(BackupBaseDir)
            ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            : Resolve(BackupBaseDir);

    // ── loader ───────────────────────────────────────────────────────────────

    private static readonly JsonSerializerOptions _opts = new()
    {
        ReadCommentHandling   = JsonCommentHandling.Skip,
        AllowTrailingCommas   = true,
        PropertyNameCaseInsensitive = true,
    };

    public static Config Load()
    {
        // 搜索顺序：exe 目录 → 当前工作目录
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "updater.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "updater.json"),
        };

        foreach (var path in candidates)
        {
            if (!File.Exists(path)) continue;
            Console.WriteLine($"  [Config] loaded from {path}");
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Config>(json, _opts) ?? new Config();
        }

        Console.WriteLine("  [Config] updater.json not found, using defaults");
        return new Config();
    }
}
