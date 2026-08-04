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

    // ── web server ───────────────────────────────────────────────────────────
    /// <summary>upgrader server / Windows 服务监听端口，默认 9000</summary>
    [JsonPropertyName("WebServerPort")]
    public int WebServerPort { get; set; } = 9000;

    /// <summary>
    /// 触发部署操作所需的密码哈希（BCrypt格式）。
    /// 留空表示不需要密码验证（仅限内网信任环境）。
    /// 使用命令 upgrader setpassword 设置密码。
    /// </summary>
    [JsonPropertyName("DeployPasswordHash")]
    public string DeployPasswordHash { get; set; } = "";

    /// <summary>
    /// 将 upgrader 自身注册为 Windows 服务时使用的服务名。
    /// 默认 AdminNETUpdaterService，与 ServiceName（被管理的应用服务）区分开。
    /// </summary>
    [JsonPropertyName("UpdaterServiceName")]
    public string UpdaterServiceName { get; set; } = "AdminNETUpdaterService";

    /// <summary>
    /// 将 upgrader 自身注册为 Windows 服务时使用的显示名称。
    /// </summary>
    [JsonPropertyName("UpdaterServiceDisplayName")]
    public string UpdaterServiceDisplayName { get; set; } = "Admin.NET Updater Service";

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
