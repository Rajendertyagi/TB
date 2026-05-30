using System.IO;
using System.Linq;
using System.Text.Json;

namespace TB.Services.FeatureFlags;

public sealed class FeatureFlagService
    : IFeatureFlagService
{
    private const string FlagsFile =
        "Settings/feature-flags.json";

    private readonly List<FeatureFlag> _flags;

    public FeatureFlagService()
    {
        _flags =
            Load().ToList();
    }

    public IReadOnlyList<FeatureFlag> Load()
    {
        if (!File.Exists(
                FlagsFile))
        {
            var defaults =
                FeatureFlagDefaults.Create();

            Save(defaults);

            return defaults;
        }

        var json =
            File.ReadAllText(
                FlagsFile);

        return JsonSerializer.Deserialize<List<FeatureFlag>>(
                   json)
               ?? [];
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
            return;
        }

        flag.Enabled = enabled;

        Save(_flags);
    }
}