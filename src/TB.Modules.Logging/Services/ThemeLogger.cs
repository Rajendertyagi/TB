using Serilog;

namespace TB.Modules.Logging.Services;

public static class ThemeLogger
{
    public static void Loaded(
        string themeName)
    {
        Log.Information(
            "Theme.Loaded Name={Theme}",
            themeName);
    }

    public static void Applied(
        string themeName)
    {
        Log.Information(
            "Theme.Applied Name={Theme}",
            themeName);
    }

    public static void Changed(
        string oldTheme,
        string newTheme)
    {
        Log.Information(
            "Theme.Changed OldTheme={OldTheme} NewTheme={NewTheme}",
            oldTheme,
            newTheme);
    }

    public static void Reset()
    {
        Log.Warning(
            "Theme.Reset");
    }

    public static void Failed(
        string themeName,
        Exception exception)
    {
        Log.Error(
            exception,
            "Theme.Failed Name={Theme}",
            themeName);
    }
}