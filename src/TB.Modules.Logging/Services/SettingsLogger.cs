using Serilog;

namespace TB.Modules.Logging.Services;

public static class SettingsLogger
{
    public static void Loaded(
        string settingsName)
    {
        Log.Information(
            "Settings.Loaded Name={Settings}",
            settingsName);
    }

    public static void Saved(
        string settingsName)
    {
        Log.Information(
            "Settings.Saved Name={Settings}",
            settingsName);
    }

    public static void Changed(
        string settingsName,
        string propertyName)
    {
        Log.Information(
            "Settings.Changed Name={Settings} Property={Property}",
            settingsName,
            propertyName);
    }

    public static void Reset(
        string settingsName)
    {
        Log.Warning(
            "Settings.Reset Name={Settings}",
            settingsName);
    }

    public static void Failed(
        string settingsName,
        Exception exception)
    {
        Log.Error(
            exception,
            "Settings.Failed Name={Settings}",
            settingsName);
    }
}
