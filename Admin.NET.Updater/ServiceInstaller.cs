#if WINDOWS
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace AdminNetUpdater;

/// <summary>
/// 将 upgrader 自身注册 / 注销为 Windows 服务。
///
/// 权限说明
/// ─────────────────────────────────────────────────────────────────────────
/// • install / uninstall 命令需要以管理员身份运行（UAC）。
/// • 服务账户默认使用 LocalSystem（NT AUTHORITY\SYSTEM），该账户拥有
///   ServiceController 操作另一个服务所需的 SERVICE_STOP / SERVICE_START
///   权限，无需额外配置。
/// • 若希望使用最小权限账户，可修改 install 命令中的 obj= 参数，并在
///   Windows 本地安全策略中授予该账户"作为服务登录"权限。
/// </summary>
public static class ServiceInstaller
{
    public static int Install(Config cfg)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Log.Error("install 命令仅支持 Windows 平台");
            return 1;
        }

        if (!IsAdministrator())
        {
            Log.Error("install 命令需要以管理员身份运行");
            Log.Warn("请右键单击命令提示符或 PowerShell，选择「以管理员身份运行」");
            return 1;
        }

        var exePath     = Process.GetCurrentProcess().MainModule?.FileName
                          ?? Path.Combine(AppContext.BaseDirectory, "upgrader.exe");
        var svcName     = cfg.UpdaterServiceName;
        var displayName = cfg.UpdaterServiceDisplayName;
        var port        = cfg.WebServerPort;

        Log.Step($"注册 Windows 服务: {svcName}");
        Log.Info($"  可执行文件: {exePath}");
        Log.Info($"  显示名称  : {displayName}");
        Log.Info($"  监听端口  : {port}");
        Log.Info($"  服务账户  : LocalSystem (NT AUTHORITY\\SYSTEM)");

        // 如果服务已存在，先停止再删除
        if (ServiceExists(svcName))
        {
            Log.Warn($"服务 [{svcName}] 已存在，将先删除再重新注册");
            StopAndDelete(svcName);
        }

        // sc create <name> binPath= "<exe> server" start= auto obj= LocalSystem DisplayName= "<display>"
        // ⚠ sc.exe 参数格式要求等号后必须有空格
        var binPath    = $"\"{exePath}\" server";
        var scArgs     = $"create \"{svcName}\" binPath= \"{binPath}\" start= auto obj= LocalSystem DisplayName= \"{displayName}\"";

        var result = RunSc(scArgs);
        if (result != 0)
        {
            Log.Error("sc create 失败，请检查是否有管理员权限");
            return 1;
        }

        // 添加服务描述
        RunSc($"description \"{svcName}\" \"Admin.NET Updater Web 管理服务，提供远程上传更新包和一键部署功能，监听端口 {port}\"");

        // 配置失败恢复策略：失败后 60 秒自动重启，最多重启 3 次
        RunSc($"failure \"{svcName}\" reset= 86400 actions= restart/60000/restart/60000/restart/60000");

        Log.Ok($"服务 [{svcName}] 注册成功");
        Log.Info("");
        Log.Info("  接下来您可以：");
        Log.Info($"    sc start {svcName}          # 启动服务");
        Log.Info($"    sc stop  {svcName}          # 停止服务");
        Log.Info($"    upgrader uninstall          # 注销服务");
        Log.Info($"    http://localhost:{port}       # 访问管理界面");
        Log.Info("");
        Log.Warn("  注意：服务以 LocalSystem 账户运行，拥有停止/启动其他服务的权限。");
        Log.Warn("        如需更严格的权限控制，请修改 sc create 中的 obj= 参数。");
        return 0;
    }

    public static int Uninstall(Config cfg)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Log.Error("uninstall 命令仅支持 Windows 平台");
            return 1;
        }

        if (!IsAdministrator())
        {
            Log.Error("uninstall 命令需要以管理员身份运行");
            return 1;
        }

        var svcName = cfg.UpdaterServiceName;
        Log.Step($"注销 Windows 服务: {svcName}");

        if (!ServiceExists(svcName))
        {
            Log.Warn($"服务 [{svcName}] 不存在，无需注销");
            return 0;
        }

        StopAndDelete(svcName);
        Log.Ok($"服务 [{svcName}] 已注销");
        return 0;
    }

    // ── 私有辅助 ──────────────────────────────────────────────────────────────

    private static bool IsAdministrator()
    {
        using var identity  = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static bool ServiceExists(string name)
    {
        try
        {
            using var sc = new System.ServiceProcess.ServiceController(name);
            _ = sc.Status; // 触发查询，不存在时抛异常
            return true;
        }
        catch { return false; }
    }

    private static void StopAndDelete(string name)
    {
        // 先尝试停止
        try
        {
            using var sc = new System.ServiceProcess.ServiceController(name);
            if (sc.Status != System.ServiceProcess.ServiceControllerStatus.Stopped)
            {
                Log.Info($"停止服务 [{name}]...");
                sc.Stop();
                sc.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped,
                    TimeSpan.FromSeconds(30));
                Log.Ok($"服务 [{name}] 已停止");
            }
        }
        catch { /* 停止失败也继续删除 */ }

        // sc delete
        var result = RunSc($"delete \"{name}\"");
        if (result == 0)
            Log.Ok($"服务 [{name}] 已删除");
        else
            Log.Warn($"sc delete 返回 {result}，服务可能已标记为待删除（重启后生效）");
    }

    /// <summary>运行 sc.exe，返回退出码</summary>
    private static int RunSc(string args)
    {
        Log.Info($"  sc {args}");
        var psi = new ProcessStartInfo("sc.exe", args)
        {
            UseShellExecute        = false,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            CreateNoWindow         = true,
        };
        using var proc = Process.Start(psi)!;
        var stdout = proc.StandardOutput.ReadToEnd();
        var stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit();

        if (!string.IsNullOrWhiteSpace(stdout)) Log.Info($"  → {stdout.Trim()}");
        if (!string.IsNullOrWhiteSpace(stderr)) Log.Warn($"  → {stderr.Trim()}");

        return proc.ExitCode;
    }
}

#endif // WINDOWS
