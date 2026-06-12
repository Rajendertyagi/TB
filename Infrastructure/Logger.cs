using System;
using System.IO;

namespace TB.Infrastructure;

public static class Logger
{
    private static string? _logDir;
    private static readonly object _lock = new();

    public static void Initialize(string basePath)
    {
        _logDir = Path.Combine(basePath, "logs");
        Directory.CreateDirectory(_logDir);
        Info($"Logger initialized at {_logDir}");
    }

    private static void Write(string level, string msg)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var line = $"[{timestamp}] [{level}] {msg}";
        Console.WriteLine(line);
        if (_logDir == null) return;
        var file = Path.Combine(_logDir, $"tb-{DateTime.Now:yyyy-MM-dd}.log");
        lock (_lock)
        {
            try { File.AppendAllText(file, line + Environment.NewLine); }
            catch { /* best effort */ }
        }
    }

    public static void Debug(string msg) => Write("DEBUG", msg);
    public static void Info(string msg) => Write("INFO", msg);
    public static void Warn(string msg) => Write("WARN", msg);
    public static void Warning(string msg) => Warn(msg);
    public static void Error(string msg) => Write("ERROR", msg);

    // ===============================================================================
    // 🛡️ MODERN EXCEPTION OVERLOADS: Formats the entire diagnostic stack trace
    // ===============================================================================
    public static void Debug(string context, Exception ex) => Write("DEBUG", FormatException(context, ex));
    public static void Warn(string context, Exception ex) => Write("WARN", FormatException(context, ex));
    public static void Error(string context, Exception ex) => Write("ERROR", FormatException(context, ex));

    private static string FormatException(string context, Exception ex)
    {
        if (ex == null) return context;
        return $"{context} -> Failure: {ex.Message}{Environment.NewLine}Stack Trace:{Environment.NewLine}{ex.StackTrace}";
    }
}
