using System;
using System.IO;

namespace TB.Helpers;

public static class Paths
{
    // Base directory where the .exe lives
    public static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;

    // THE SINGLE LINE TO CHANGE:
    // Portable Mode: Path.Combine(BaseDir, "AppData")
    // Installed Mode: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TB")
    public static readonly string AppDataDir = Path.Combine(BaseDir, "AppData");

    // Centralized File & Folder Paths
    public static readonly string SettingsFile = Path.Combine(AppDataDir, "settings.json");
    public static readonly string SessionFile = Path.Combine(AppDataDir, "session.json");
    public static readonly string DownloadsDir = Path.Combine(AppDataDir, "Downloads");
    public static readonly string WebView2CacheDir = Path.Combine(AppDataDir, "WebView2Cache");
    public static readonly string WwwrootDir = Path.Combine(BaseDir, "wwwroot");

    // Static constructor runs once on startup to ensure folders exist
    static Paths()
    {
        Directory.CreateDirectory(AppDataDir);
        Directory.CreateDirectory(DownloadsDir);
        Directory.CreateDirectory(WebView2CacheDir);
    }
}

