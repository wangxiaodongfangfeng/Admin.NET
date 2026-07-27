using System.Diagnostics;
using System.Runtime.InteropServices;
#if WINDOWS
using System.ServiceProcess;
#endif

namespace AdminNetUpdater;

/// <summary>
/// 跨平台服务控制
/// • Windows: 使用 ServiceController API
/// • Linux:   使用 systemctl 命令（需要当前用户有 sudo 或 polkit 权限，
///            或服务以 root / 有权限的账户运行）
/// </summary>
public static class ServiceControl
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);

    // ── 公共接口 ───────────────────────────────────────────────────────────────

    public static bool Stop(string serviceName, Action<string, string> log, TimeSpan? timeout = null)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return StopWindows(serviceName, log, timeout ?? DefaultTimeout);
        else
            return SystemctlCommand("stop", serviceName, log, timeout ?? DefaultTimeout);
    }

    public static bool Start(string serviceName, Action<string, string> log, TimeSpan? timeout = null)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return StartWindows(serviceName, log, timeout ?? DefaultTimeout);
        else
            return SystemctlCommand("start", serviceName, log, timeout ?? DefaultTimeout);
    }

    /// <summary>
    /// 查询服务当前状态字符串。
    /// 返回值：running | stopped | failed | not_found | unknown
    /// </summary>
    public static string GetStatus(string serviceName)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return GetStatusWindows(serviceName);
        else
            return GetStatusLinux(serviceName);
    }

    // ── Windows 实现 ──────────────────────────────────────────────────────────

#if WINDOWS
    private static bool StopWindows(string name, Action<string, string> log, TimeSpan timeout)
    {
        try
        {
            using var sc = new ServiceController(name);
            if (sc.Status == ServiceControllerStatus.Stopped)
            { log("info", $"服务 [{name}] 已停止，无需操作"); return true; }

            log("info", $"正在停止服务 [{name}]（当前: {sc.Status}）...");
            sc.Stop();
            sc.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            log("ok", $"服务 [{name}] 已停止");
            return true;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("does not exist"))
        { log("warn", $"服务 [{name}] 不存在，跳过停止"); return true; }
        catch (Exception ex)
        { log("error", $"停止服务失败: {ex.Message}"); return false; }
    }

    private static bool StartWindows(string name, Action<string, string> log, TimeSpan timeout)
    {
        try
        {
            using var sc = new ServiceController(name);
            if (sc.Status == ServiceControllerStatus.Running)
            { log("info", $"服务 [{name}] 已在运行"); return true; }

            log("info", $"正在启动服务 [{name}]...");
            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running, timeout);
            log("ok", $"服务 [{name}] 已启动");
            return true;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("does not exist"))
        { log("warn", $"服务 [{name}] 不存在，跳过启动"); return true; }
        catch (Exception ex)
        { log("error", $"启动服务失败: {ex.Message}"); return false; }
    }

    private static string GetStatusWindows(string name)
    {
        try
        {
            using var sc = new ServiceController(name);
            return sc.Status switch
            {
                ServiceControllerStatus.Running => "running",
                ServiceControllerStatus.Stopped => "stopped",
                _                               => sc.Status.ToString().ToLower(),
            };
        }
        catch { return "not_found"; }
    }
#else
    // Linux 也需要这些方法签名（实际走 systemctl 分支）
    private static bool StopWindows(string name, Action<string, string> log, TimeSpan timeout)
        => SystemctlCommand("stop", name, log, timeout);
    private static bool StartWindows(string name, Action<string, string> log, TimeSpan timeout)
        => SystemctlCommand("start", name, log, timeout);
    private static string GetStatusWindows(string name) => GetStatusLinux(name);
#endif

    // ── Linux systemctl 实现 ──────────────────────────────────────────────────

    /// <summary>
    /// 执行 systemctl &lt;action&gt; &lt;service&gt; 并等待目标状态。
    /// 若当前用户没有直接权限，自动尝试 sudo systemctl。
    /// </summary>
    private static bool SystemctlCommand(string action, string name,
        Action<string, string> log, TimeSpan timeout)
    {
        var targetStatus = action == "stop" ? "inactive" : "active";
        log("info", $"systemctl {action} {name}");

        // 先尝试不带 sudo，失败了再试 sudo
        foreach (var useSudo in new[] { false, true })
        {
            var (cmd, cmdArgs) = useSudo
                ? ("sudo", $"systemctl {action} {name}")
                : ("systemctl", $"{action} {name}");

            var (exitCode, stdout, stderr) = RunProcess(cmd, cmdArgs);

            // sudo 提示密码错误时不重试
            if (useSudo && stderr.Contains("password"))
            {
                log("error", "sudo 需要密码，请以 root 运行或配置 sudoers 免密");
                return false;
            }

            if (exitCode == 0)
            {
                log("ok", $"systemctl {action} {name} 成功");
                // 等待状态收敛
                return WaitForStatus(name, targetStatus, log, timeout);
            }

            if (!useSudo)
            {
                log("warn", $"systemctl 返回 {exitCode}，尝试 sudo...");
                if (!string.IsNullOrWhiteSpace(stderr))
                    log("warn", $"  {stderr.Trim()}");
            }
            else
            {
                log("error", $"sudo systemctl {action} 失败 (exit {exitCode}): {stderr.Trim()}");
                return false;
            }
        }
        return false;
    }

    private static bool WaitForStatus(string name, string expected,
        Action<string, string> log, TimeSpan timeout)
    {
        var deadline = DateTime.Now + timeout;
        while (DateTime.Now < deadline)
        {
            var status = GetStatusLinux(name);
            if (status == expected || status == "not_found")
                return true;
            // failed 也算已停止
            if (expected == "inactive" && status == "failed")
                return true;
            Thread.Sleep(500);
        }
        log("warn", $"等待服务状态超时（期望: {expected}，当前: {GetStatusLinux(name)}）");
        return false;
    }

    private static string GetStatusLinux(string name)
    {
        // systemctl is-active 返回: active | inactive | failed | unknown
        var (exitCode, stdout, _) = RunProcess("systemctl", $"is-active {name}");
        var raw = stdout.Trim().ToLower();
        return raw switch
        {
            "active"   => "running",
            "inactive" => "stopped",
            "failed"   => "failed",
            _          => exitCode == 4 ? "not_found" : "unknown",
        };
    }

    // ── 通用进程执行 ──────────────────────────────────────────────────────────

    private static (int exitCode, string stdout, string stderr) RunProcess(string cmd, string args)
    {
        try
        {
            var psi = new ProcessStartInfo(cmd, args)
            {
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true,
            };
            using var proc = Process.Start(psi)!;
            var stdout = proc.StandardOutput.ReadToEnd();
            var stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit(30_000);
            return (proc.ExitCode, stdout, stderr);
        }
        catch (Exception ex)
        {
            return (-1, "", ex.Message);
        }
    }
}
