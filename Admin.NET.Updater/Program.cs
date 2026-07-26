using AdminNetUpdater;

// ── Entry point ───────────────────────────────────────────────────────────────

PrintBanner();

if (args.Length == 0)
{
    PrintHelp();
    return 0;
}

var command = args[0].ToLowerInvariant();

if (command is "-h" or "--help" or "help")
{
    PrintHelp();
    return 0;
}

if (command is not ("pack" or "deploy"))
{
    Log.Error($"未知命令: {command}");
    PrintHelp();
    return 1;
}

// 加载配置
var cfg = Config.Load();

return command switch
{
    "pack"   => PackCommand.Run(cfg),
    "deploy" => DeployCommand.Run(cfg),
    _        => 1,
};

// ── Helpers ───────────────────────────────────────────────────────────────────

static void PrintBanner()
{
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine("""
    ╔══════════════════════════════════════════╗
    ║        Admin.NET  Updater  v1.0          ║
    ║   Pack & Deploy automation tool          ║
    ╚══════════════════════════════════════════╝
    """);
    Console.ResetColor();
}

static void PrintHelp()
{
    Console.WriteLine("""
    用法:
      upgrader pack      — 将后端和前端产物打包为 zip
      upgrader deploy    — 停服 → 备份 → 部署最新 zip → 启服

    配置文件:  updater.json（与 exe 同目录或当前工作目录）

    pack 前置条件:
      1. 运行 dotnet publish（输出到 publish/ 目录）
      2. 运行 npm run build（输出到 Web/dist/ 目录）

    deploy 说明:
      - 自动选择 packages/ 中最新的 AdminNET_package_*.zip
      - 备份目录: %DOCUMENTS%\AppBakyyyyMMdd\
      - wwwroot 中的 upload/ 目录不会被删除

    配置项说明（updater.json）:
      PublishDir            dotnet publish 输出目录
      WebDistDir            npm build 输出目录
      PackageOutputDir      zip 输出目录
      PackageDownloadUrl    远程 zip 下载地址（http/https），留空跳过下载
      DownloadUsername      Basic Auth 用户名（可选）
      DownloadPassword      Basic Auth 密码（可选）
      DownloadTimeoutSeconds 下载超时秒数，默认 300
      DownloadSaveDir       下载 zip 的保存目录，留空同 DeployPackageDir
      DeployPackageDir      deploy 时查找 zip 的目录（无远程下载时用）
      ServiceDir            Windows 服务安装目录
      ServiceName           Windows 服务名称
      BackupBaseDir         备份父目录（空=用户 Documents）
    """);
}
