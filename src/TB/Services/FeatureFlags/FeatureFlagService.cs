using System.IO;
using System.Linq;
using System.Text.Json;
using TB.Modules.Logging.Services;

namespace TB.Services.FeatureFlags;

public sealed class FeatureFlagService
    : IFeatureFlagService
{
    private const string FlagsFile =
        "Settings/feature-flags.json";

    private readonly List<FeatureFlag> _flags;

    public FeatureFlagService()
    {
        LifecycleLogger.Created(
            nameof(FeatureFlagService));

        _flags =
            Load().ToList();

        LifecycleLogger.Initialized(
            nameof(FeatureFlagService));
    }

    public IReadOnlyList<FeatureFlag> Load()
    {
        if (!File.Exists(
                FlagsFile))
        {
            var defaults =
                FeatureFlagDefaults.Create();

            Save(defaults);

            FeatureFlagLogger.Loaded(
                "Defaults",
                true);

            return defaults;
        }

        var json =
            File.ReadAllText(
                FlagsFile);

        var flags =
            JsonSerializer.Deserialize<List<FeatureFlag>>(
                json)
            ?? [];

        foreach (var flag in flags)
        {
            FeatureFlagLogger.Loaded(
                flag.Name,
                flag.Enabled);
        }

        return flags;
    }

    public void Save(
        IReadOnlyList<FeatureFlag> flags)
    {
        Directory.CreateDirectory(
            "Settings");

        var json =
            JsonSerializer.Serialize(
                flags,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        File.WriteAllText(
            FlagsFile,
            json);
    }

    public bool IsEnabled(
        string name)
    {
        return _flags.FirstOrDefault(
                   x => x.Name == name)
               ?.Enabled
               ?? false;
    }

    public void SetEnabled(
        string name,
        bool enabled)
    {
        var flag =
            _flags.FirstOrDefault(
                x => x.Name == name);

        if (flag is null)
        {
            FeatureFlagLogger.Failed(
                name,
                new InvalidOperationException(
                    "Feature flag not found"));

            return;
        }

        flag.Enabled = enabled;

        if (enabled)
        {
            FeatureFlagLogger.Enabled(
                name);
        }
        else
        {
            FeatureFlagLogger.Disabled(
                name);
        }

        FeatureFlagLogger.Changed(
            name,
            enabled);

        Save(
            _flags);
    }
}