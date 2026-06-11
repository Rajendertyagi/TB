using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TB.Helpers;
using TB.Services.Interfaces;

namespace TB.Infrastructure;

public class SettingsService : ISettingsService
{
    private readonly string _dataFile;
    private readonly Dictionary<string, object> _settings;
    private readonly object _lock = new(); // Thread-safety for concurrent IPC/UI access

    public SettingsService(string basePath)
    {
        // PORTABLE MODE: Keep settings in the same folder as the .exe
        var appDataFolder = Path.Combine(basePath, "AppData");
        Directory.CreateDirectory(appDataFolder);

        _dataFile = Path.Combine(appDataFolder, "settings.json");

        // INDUSTRY STANDARD DEFAULTS
        _settings = new Dictionary<string, object>
        {
            // Navigation
            ["search-engine"] = "google",
            ["home-page"] = Defaults.HomeUrl,
            ["new-tab-page"] = Defaults.HomeUrl,

            // Appearance
            ["theme-name"] = Defaults.Theme,
            ["accent-color"] = "", // Empty = defer to theme.json
            ["font-size"] = 14,
            ["compact-mode"] = false,

            // Privacy & Security
            ["block-trackers"] = true,
            ["block-cookies"] = false,
            ["https-only"] = false,
            ["do-not-track"] = true,

            // Downloads (Standard: OS User Downloads folder, not hidden AppData)
            ["download-path"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            ["ask-save"] = true,
            ["auto-open-pdf"] = false
        };

        Load();
    }

    public T? Get<T>(string key, T? defaultValue = default)
    {
        lock (_lock)
        {
            if (_settings.TryGetValue(key, out var val))
            {
                try
                {
                    // System.Text.Json deserializes 'object' as JsonElement. 
                    // We must explicitly unwrap it to prevent InvalidCastException.
                    if (val is JsonElement element)
                    {
                        return element.Deserialize<T>() ?? defaultValue;
                    }

                    return (T)Convert.ChangeType(val, typeof(T));
                }
                catch
                {
                    // Silent fallback for type mismatches
                }
            }
            return defaultValue;
        }
    }

    public void Set(string key, object value)
    {
        lock (_lock)
        {
            _settings[key] = value;
        }
        Save();
    }

    public Dictionary<string, object> GetAll()
    {
        lock (_lock)
        {
            return new Dictionary<string, object>(_settings);
        }
    }

    public string GetAllJson()
    {
        lock (_lock)
        {
            return JsonSerializer.Serialize(_settings);
        }
    }

    private void Save()
    {
        lock (_lock)
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
    }

    private void Load()
    {
        lock (_lock)
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