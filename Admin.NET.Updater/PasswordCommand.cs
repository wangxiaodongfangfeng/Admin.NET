using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdminNetUpdater;

/// <summary>
/// upgrader setpassword — 交互式设置/清除部署密码
/// 将 BCrypt 哈希写入 updater.json 的 DeployPasswordHash 字段。
/// </summary>
public static class PasswordCommand
{
    public static int Run(Config cfg)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  设置部署密码（输入时不显示字符）");
        Console.WriteLine("  输入空密码并回车 = 清除密码（无需验证即可部署，谨慎使用）");
        Console.ResetColor();
        Console.WriteLine();

        Console.Write("  新密码: ");
        var password = ReadPassword();
        Console.WriteLine();

        if (string.IsNullOrEmpty(password))
        {
            if (!Confirm("  确定清除密码吗？任何人都可直接触发部署 [y/N]: "))
            {
                Log.Warn("已取消");
                return 0;
            }
            WriteHash(cfg, "");
            Log.Ok("密码已清除，部署操作无需验证");
            return 0;
        }

        Console.Write("  确认密码: ");
        var confirm = ReadPassword();
        Console.WriteLine();

        if (password != confirm)
        {
            Log.Error("两次输入的密码不一致");
            return 1;
        }

        if (password.Length < 6)
        {
            Log.Error("密码长度至少 6 位");
            return 1;
        }

        // 生成 BCrypt 哈希（work factor 12 足够安全，约 300ms）
        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        WriteHash(cfg, hash);

        Log.Ok("密码已设置，下次触发部署时将要求验证");
        return 0;
    }

    /// <summary>验证明文密码是否匹配存储的哈希</summary>
    public static bool Verify(string plaintext, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(storedHash)) return true; // 未设置密码，放行
        if (string.IsNullOrWhiteSpace(plaintext))  return false;
        try { return BCrypt.Net.BCrypt.Verify(plaintext, storedHash); }
        catch  { return false; }
    }

    // ── 私有辅助 ──────────────────────────────────────────────────────────────

    private static string ReadPassword()
    {
        var sb = new System.Text.StringBuilder();
        ConsoleKeyInfo key;
        while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                sb.Length--;
            else if (key.KeyChar >= ' ')
                sb.Append(key.KeyChar);
        }
        return sb.ToString();
    }

    private static bool Confirm(string prompt)
    {
        Console.Write(prompt);
        var answer = Console.ReadLine()?.Trim().ToLower();
        return answer is "y" or "yes";
    }

    /// <summary>将新哈希写回 updater.json（保留其他字段不变）</summary>
    private static void WriteHash(Config cfg, string hash)
    {
        // 找到 updater.json 的实际路径
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "updater.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "updater.json"),
        };

        var path = candidates.FirstOrDefault(File.Exists);
        if (path == null)
        {
            Log.Error("找不到 updater.json，请先确认配置文件存在");
            return;
        }

        // 读取原始 JSON，修改 DeployPasswordHash 后写回
        var opts = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented       = true,
        };

        var json = File.ReadAllText(path);
        var doc  = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, opts)
                   ?? new Dictionary<string, JsonElement>();

        // 重建字典（更新 DeployPasswordHash）
        var updated = new Dictionary<string, object?>();
        foreach (var kv in doc)
            updated[kv.Key] = kv.Value;

        updated["DeployPasswordHash"] = hash;

        var newJson = JsonSerializer.Serialize(updated, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, newJson);
        Log.Info($"  已更新: {path}");
    }
}
