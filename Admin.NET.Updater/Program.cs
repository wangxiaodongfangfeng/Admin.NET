using AdminNetUpdater;
#if WINDOWS
using Microsoft.Extensions.Hosting.WindowsServices;
#endif

// ── 服务模式检测（仅 Windows）────────────────────────────────────────────────
#if WINDOWS
if (WindowsServiceHelpers.IsWindowsService())
{
    var cfgSvc = Config.Load();
    await WebServer.Run(cfgSvc, args);
    return 0;
}
#endif

// ── 命令行模式 ────────────────────────────────────────────────────────────────
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

var validCommands = new HashSet<string> { "pack", "deploy", "server", "setpassword" };
#if WINDOWS
validCommands.Add("install");
validCommands.Add("uninstall");
#endif

if (!validCommands.Contains(command))
{
    Log.Error($"未知命令: {command}");
    PrintHelp();
    return 1;
}

var cfg = Config.Load();

return command switch
{
    "pack"    => PackCommand.Run(cfg),
    "deploy"  => DeployCommand.Run(cfg),
    "server"  => await RunServer(cfg, args[1..]),
    "setpassword" => PasswordCommand.Run(cfg),
#if WINDOWS
    "install"   => ServiceInstaller.Install(cfg),
    "uninstall" => ServiceInstaller.Uninstall(cfg),
#endif
    _ => 1,
};

static async Task<int> RunServer(Config cfg, string[] remainingArgs)
{
    await WebServer.Run(cfg, remainingArgs);
    return 0;
}

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
      upgrader pack        — 将后端和前端产物打包为 zip
      upgrader deploy      — 停服 → 备份 → 部署最新 zip → 启服
      upgrader server      — 启动 Web 管理服务器（调试用，前台运行）
      upgrader setpassword — 设置/清除部署密码
    """);
#if WINDOWS
    Console.WriteLine("""
      upgrader install     — 将 upgrader 注册为 Windows 服务（需管理员）
      upgrader uninstall   — 注销 upgrader Windows 服务（需管理员）

    Windows 服务安装流程:
      1. 管理员身份运行:  upgrader install
      2. 启动服务:        sc start AdminNETUpdaterService
      3. 访问界面:        http://<IP>:<WebServerPort>
      4. 注销服务:        upgrader uninstall

    权限说明:
      服务以 LocalSystem 账户运行，可停止/启动其他 Windows 服务，无需额外配置。
    """);
#endif
    Console.WriteLine("""
    配置文件: updater.json（与 exe 同目录或当前工作目录）

    配置项:
      ServiceDir              目标应用服务的安装目录
      ServiceName             目标应用服务的 Windows 服务名
      WebServerPort           Web 管理服务器端口（默认 9000）
      UpdaterServiceName      upgrader 自身的服务名
      DeployPackageDir        deploy 时查找 zip 的目录
      BackupBaseDir           备份父目录（空=用户 Documents）
    """);
}
