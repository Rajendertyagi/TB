using System.Text.Json;
using TB.Modules.Settings.Contracts;
using TB.Modules.Settings.Models;

namespace TB.Modules.Settings.Services;

public sealed class SettingsService : ISettingsService
{
    private const string SettingsFile = "Settings/appsettings.json";

    public ApplicationSettings Load()
    {
        if (!File.Exists(SettingsFile))
        {
            var defaults = new ApplicationSettings();
            Save(defaults);
            return defaults;
        }

        var json = File.ReadAllText(SettingsFile);

        return JsonSerializer.Deserialize<ApplicationSettings>(json)
               ?? new ApplicationSettings();
    }

    public void Save(ApplicationSettings settings)
    {
        Directory.CreateDirectory("Settings");

        var json = JsonSerializer.Serialize(
            settings,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(SettingsFile, json);
    }
}
