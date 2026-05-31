using Serilog;

namespace TB.Modules.Logging.Services;

public static class FeatureFlagLogger
{
    public static void Loaded(
        string flagName,
        bool enabled)
    {
        Log.Information(
            "FeatureFlag.Loaded Name={Flag} Enabled={Enabled}",
            flagName,
            enabled);
    }

    public static void Enabled(
        string flagName)
    {
        Log.Information(
            "FeatureFlag.Enabled Name={Flag}",
            flagName);
    }

    public static void Disabled(
        string flagName)
    {
        Log.Information(
            "FeatureFlag.Disabled Name={Flag}",
            flagName);
    }

    public static void Changed(
        string flagName,
        bool enabled)
    {
        Log.Information(
            "FeatureFlag.Changed Name={Flag} Enabled={Enabled}",
            flagName,
            enabled);
    }

    public static void Failed(
        string flagName,
        Exception exception)
    {
        Log.Error(
            exception,
            "FeatureFlag.Failed Name={Flag}",
            flagName);
    }
}