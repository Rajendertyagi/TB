#pragma warning disable CS8601, CS8602, CS8603, CS8604 // Disable WinRT projection nullable quirks

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using TB.Helpers;
using TB.Infrastructure;
using TB.Models;
using TB.Services.Interfaces;
using Windows.UI;

namespace TB.Services;

public class ThemeService : IThemeService
{
    private readonly string _themesDirectory;
    private readonly ISettingsService _settings;

    private ThemeDefinition _theme = new();
    private FileSystemWatcher? _watcher;
    private DateTime _lastReload = DateTime.MinValue;

    public ThemeDefinition CurrentTheme => _theme;
    public string ActiveThemeName => _theme.Active;
    public event Action? ThemeChanged;

    public ThemeService(string basePath, ISettingsService settings)
    {
        _themesDirectory = Path.Combine(basePath, "wwwroot", "themes");
        _settings = settings;

        Directory.CreateDirectory(_themesDirectory);
        SetupFileWatcher();
    }

    private void SetupFileWatcher()
    {
        try
        {
            _watcher = new FileSystemWatcher(_themesDirectory)
            {
                Filter = "*.json",
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };

            void OnFileChanged()
            {
                var now = DateTime.UtcNow;
                if ((now - _lastReload).TotalMilliseconds < 500) return;
                _lastReload = now;
                _ = ReloadAndApplyAsync();
            }

            _watcher.Changed += (_, _) => OnFileChanged();
            _watcher.Created += (_, _) => OnFileChanged();
            _watcher.Renamed += (_, _) => OnFileChanged();
            _watcher.EnableRaisingEvents = true;
        }
        catch (Exception ex) { Logger.Warning($"Theme watcher failed: {ex.Message}"); }
    }

    private async Task ReloadAndApplyAsync()
    {
        try
        {
            await Task.Delay(100);
            await ReloadThemeAsync();
            ApplyXamlResources();

            if (App.MainWindow?.Content is FrameworkElement root)
            {
                var current = root.RequestedTheme;
                root.RequestedTheme = current == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
                root.RequestedTheme = current;
            }

            NotifyThemeChanged();
        }
        catch (Exception ex) { Logger.Error($"Theme reload failed: {ex.Message}"); }
    }

    public async Task ReloadThemeAsync()
    {
        var activeTheme = _settings.Get("theme-name", Defaults.Theme);
        var themePath = Path.Combine(_themesDirectory, $"{activeTheme}.json");

        if (!File.Exists(themePath))
        {
            Logger.Warning($"Theme '{activeTheme}.json' not found. Falling back to default.");
            themePath = Path.Combine(_themesDirectory, $"{Defaults.Theme}.json");
        }

        if (!File.Exists(themePath))
        {
            Logger.Error("No theme files found. Using hardcoded C# defaults.");
            _theme = new ThemeDefinition();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(themePath);
            _theme = ThemeDefinition.Load(json);
            _theme.Active = activeTheme;
            Logger.Info($"Theme loaded: {activeTheme}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to parse {themePath}: {ex.Message}");
            _theme = new ThemeDefinition();
        }
    }

    public void ApplyNativeTheme(AppWindow appWindow)
    {
        try
        {
            var textHex = _theme.Native.TitleBarText ?? _theme.Colors.GetValueOrDefault("textMain");
            var hoverHex = _theme.Native.TitleBarIconHover ?? _theme.Colors.GetValueOrDefault("accent");
            var accentHex = _theme.Colors.GetValueOrDefault("accent");
            var borderHex = _theme.Colors.GetValueOrDefault("borderCrisp");

            var textColor = !string.IsNullOrEmpty(textHex) ? ColorExtensions.ParseHex(textHex) : Colors.White;
            var hoverColor = !string.IsNullOrEmpty(hoverHex) ? ColorExtensions.ParseHex(hoverHex) : Colors.White;
            var accentColor = !string.IsNullOrEmpty(accentHex) ? ColorExtensions.ParseHex(accentHex) : Colors.Transparent;
            var borderColor = !string.IsNullOrEmpty(borderHex) ? ColorExtensions.ParseHex(borderHex) : Colors.Transparent;

            appWindow.TitleBar.ButtonForegroundColor = textColor;
            appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.ButtonInactiveForegroundColor = textColor;
            appWindow.TitleBar.ButtonHoverBackgroundColor = borderColor;
            appWindow.TitleBar.ButtonHoverForegroundColor = hoverColor;
            appWindow.TitleBar.ButtonPressedBackgroundColor = accentColor;
            appWindow.TitleBar.ButtonPressedForegroundColor = Colors.White;
        }
        catch (Exception ex) { Logger.Error($"Native theme failed: {ex.Message}"); }
    }

    public void ApplyXamlResources()
    {
        var themeDict = Application.Current.Resources.ThemeDictionaries;
        var dark = GetOrCreateDict(themeDict, "Dark");
        var light = GetOrCreateDict(themeDict, "Light");

        foreach (var (key, value) in _theme.Colors)
        {
            if (string.IsNullOrEmpty(value)) continue;

            if (value.Equals("transparent", StringComparison.OrdinalIgnoreCase))
            {
                SetBrushResource(dark, $"{key}Brush", Colors.Transparent);
                SetBrushResource(light, $"{key}Brush", Colors.Transparent);
                dark[$"{key}Color"] = Colors.Transparent;
                light[$"{key}Color"] = Colors.Transparent;
                continue;
            }

            if (!value.StartsWith("#") || value.Length < 4) continue;

            try
            {
                var color = ColorExtensions.ParseHex(value);
                dark[$"{key}Color"] = color;
                SetBrushResource(dark, $"{key}Brush", color);
                light[$"{key}Color"] = color;
                SetBrushResource(light, $"{key}Brush", color);
            }
            catch (Exception ex) { Logger.Error($"Parse color '{key}' failed: {ex.Message}"); }
        }

        var layoutMap = new Dictionary<string, double>
        {
            { "tabbarHeight", Layout.TabbarHeight }, { "navHeight", Layout.NavHeight },
            { "tabHeight", Layout.TabHeight }, { "urlbarHeight", Layout.UrlbarHeight },
            { "radiusSm", Layout.RadiusSm }, { "radiusMd", Layout.RadiusMd }, { "radiusLg", Layout.RadiusLg },
            { "tabInterTabGap", Layout.TabInterTabGap }, { "tabMinWidth", Layout.TabMinWidth }, { "tabMaxWidth", Layout.TabMaxWidth }
        };

        foreach (var (key, value) in layoutMap)
        {
            if (key.StartsWith("radius"))
            {
                var cr = new CornerRadius(value);
                dark[$"{key}Length"] = cr; light[$"{key}Length"] = cr;
            }
            else
            {
                dark[$"{key}Length"] = value; dark[$"{key}Thickness"] = new Thickness(value);
                light[$"{key}Length"] = value; light[$"{key}Thickness"] = new Thickness(value);
            }
        }
    }

    private static void SetBrushResource(IDictionary<object, object> dict, string key, Color color)
    {
        if (dict.TryGetValue(key, out var existing) && existing is SolidColorBrush brush)
            brush.Color = color;
        else
            dict[key] = new SolidColorBrush(color);
    }

    private static ResourceDictionary GetOrCreateDict(IDictionary<object, object> themeDict, string key)
    {
        if (themeDict.TryGetValue(key, out var existing) && existing is ResourceDictionary dict)
            return dict;

        var newDict = new ResourceDictionary();
        themeDict[key] = newDict;
        return newDict;
    }

    public async Task SetThemeAsync(string themeName)
    {
        _settings.Set("theme-name", themeName);
        await ReloadAndApplyAsync();
    }

    public Dictionary<string, string> GetCssVariables()
    {
        var vars = new Dictionary<string, string>();

        foreach (var (key, value) in _theme.Colors)
        {
            vars[$"--{ToKebabCase(key)}"] = value ?? "";
        }

        vars["--tabbar-height"] = $"{Layout.TabbarHeight}px";
        vars["--nav-height"] = $"{Layout.NavHeight}px";
        vars["--tab-height"] = $"{Layout.TabHeight}px";
        vars["--urlbar-height"] = $"{Layout.UrlbarHeight}px";
        vars["--radius-sm"] = $"{Layout.RadiusSm}px";
        vars["--radius-md"] = $"{Layout.RadiusMd}px";
        vars["--radius-lg"] = $"{Layout.RadiusLg}px";
        vars["--tab-gap"] = $"{Layout.TabInterTabGap}px";
        vars["--tab-min-width"] = $"{Layout.TabMinWidth}px";
        vars["--tab-max-width"] = $"{Layout.TabMaxWidth}px";

        return vars;
    }

    private static string ToKebabCase(string str) => string.Concat(str.Select((c, i) => char.IsUpper(c) && i > 0 ? "-" + char.ToLowerInvariant(c).ToString() : char.ToLowerInvariant(c).ToString()));

    public void NotifyThemeChanged() => ThemeChanged?.Invoke();

    public void CycleTheme()
    {
        if (App.MainWindow is Window window && window.Content is FrameworkElement root)
        {
            root.RequestedTheme = root.RequestedTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
        }
        NotifyThemeChanged();
    }
}

