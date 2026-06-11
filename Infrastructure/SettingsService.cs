using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TB.Services.Interfaces;

namespace TB.Infrastructure
{
    public class SettingsService : ISettingsService
    {
        private readonly string _dataFile;
        private Dictionary<string, object> _settings;

        public SettingsService(string basePath)
        {
            _dataFile = Path.Combine(basePath, "AppData", "settings.json");
            _settings = new Dictionary<string, object>
            {
                ["search-engine"] = "google",
                ["home-page"] = "https://www.google.com",
                ["new-tab-page"] = "google",
                ["theme-select"] = "dark",
                ["accent-color"] = "#5b9cf6",
                ["font-size"] = "13",
                ["compact-mode"] = false,
                ["block-trackers"] = true,
                ["block-cookies"] = true,
                ["https-only"] = true,
                ["do-not-track"] = false,
                ["download-path"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                ["ask-save"] = false,
                ["auto-open-pdf"] = false
            };
            Load();
        }

        public T? Get<T>(string key, T? defaultValue = default)
        {
            if (_settings.TryGetValue(key, out var val))
            {
                try { return (T)Convert.ChangeType(val, typeof(T)); }
                catch (Exception) { /* silent fallback — type conversion expected to fail */ }
            }
            return defaultValue;
        }

        public void Set(string key, object value)
        {
            _settings[key] = value;
            Save();
        }

        public Dictionary<string, object> GetAll() => new(_settings);

        public string GetAllJson()
        {
            return JsonSerializer.Serialize(_settings);
        }

        private void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_dataFile, json);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to save settings: {ex.Message}");
            }
        }

        private void Load()
        {
            try
            {
                if (File.Exists(_dataFile))
                {
                    var json = File.ReadAllText(_dataFile);
                    var loaded = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if (loaded != null)
                    {
                        foreach (var kv in loaded)
                            _settings[kv.Key] = kv.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load settings: {ex.Message}");
            }
        }
    }
}
