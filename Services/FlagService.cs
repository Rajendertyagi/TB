using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using TB.Helpers;
using TB.Infrastructure;
using TB.Services.Interfaces;

namespace TB.Services;

public class FlagService : IFlagService
{
    private readonly ISettingsService _settingsService;
    private readonly List<FlagDefinition> _flags = new();

    public FlagService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        LoadRegistry();
    }

    private void LoadRegistry()
    {
        try
        {
            var registryPath = Path.Combine(Paths.WwwrootDir, "flags-registry.json");
            if (File.Exists(registryPath))
            {
                var json = File.ReadAllText(registryPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var list = JsonSerializer.Deserialize<List<FlagDefinition>>(json, options);
                if (list != null)
                {
                    _flags.AddRange(list);
                }
            }
            else
            {
                Logger.Error($"flags-registry.json not found at: {registryPath}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load flags-registry.json: {ex.Message}");
        }
    }

    public bool IsFeatureEnabled(string flagId)
    {
        var settingKey = $"flag-{flagId}";
        var value = _settingsService.Get<string>(settingKey, "default") ?? "default";

        if (value == "enabled") return true;
        if (value == "disabled") return false;

        // Fall back to registry default
        var def = _flags.FirstOrDefault(f => f.Id == flagId);
        return def != null && def.Default == "enabled";
    }

    public List<string> GetActiveChromiumSwitches()
    {
        var activeSwitches = new List<string>();
        foreach (var def in _flags)
        {
            if (IsFeatureEnabled(def.Id))
            {
                if (def.Switches != null && def.Switches.Length > 0)
                {
                    activeSwitches.AddRange(def.Switches);
                }
            }
        }
        return activeSwitches;
    }

    public object GetFlagsDataPayload()
    {
        string platform = "Windows";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) platform = "macOS";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) platform = "Linux";

        var flagsWithValues = _flags.Select(f => new
        {
            f.Id,
            f.Name,
            f.Description,
            f.Category,
            f.Default,
            f.Platforms,
            f.RequiresRestart,
            f.Status,
            Value = _settingsService.Get<string>($"flag-{f.Id}", "default") ?? "default"
        }).ToList();

        return new
        {
            Platform = platform,
            Flags = flagsWithValues
        };
    }

    private class FlagDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Default { get; set; } = "disabled";
        public string[] Platforms { get; set; } = Array.Empty<string>();
        public bool RequiresRestart { get; set; }
        public string Status { get; set; } = "experimental";
        public string[] Switches { get; set; } = Array.Empty<string>();
    }
}
