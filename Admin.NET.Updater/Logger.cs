namespace AdminNetUpdater;

/// <summary>
/// Simple colored console logger
/// </summary>
public static class Log
{
    public static void Info(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("[INFO]  ");
        Console.ResetColor();
        Console.WriteLine(msg);
    }

    public static void Ok(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("[ OK ]  ");
        Console.ResetColor();
        Console.WriteLine(msg);
    }

    public static void Warn(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("[WARN]  ");
        Console.ResetColor();
        Console.WriteLine(msg);
    }

    public static void Error(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("[ERR ]  ");
        Console.ResetColor();
        Console.WriteLine(msg);
    }

    public static void Step(string msg)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
        Console.WriteLine($"▶ {msg}");
        Console.ResetColor();
    }

    public static void Done(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✔ {msg}");
        Console.ResetColor();
    }
}
