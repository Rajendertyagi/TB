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
    private readonly string _themeFilePath;
    private readonly ISettingsService _settings;
    private ThemeDefinition _theme = new();
    private FileSystemWatcher? _watcher;
    private DateTime _lastReload = DateTime.MinValue;

    public ThemeDefinition CurrentTheme => _theme;
    public string ActiveThemeName => _theme.Active;
    public event Action? ThemeChanged;

    public ThemeService(string basePath, ISettingsService settings)
    {
        _themeFilePath = Path.Combine(basePath, "wwwroot", "theme.json");
        _settings = settings;
        try
        {
            _watcher = new FileSystemWatcher(Path.GetDirectoryName(_themeFilePath)!)
            {
                Filter = Path.GetFileName(_themeFilePath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
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
        catch (Exception ex)
        {
            Logger.Warning($"Failed to start theme file watcher: {ex.Message}");
        }
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
            Logger.Info("Theme reloaded from file change");
        }
        catch (Exception ex)
        {
            Logger.Error($"Theme reload failed: {ex.Message}");
        }
    }

    public async Task ReloadThemeAsync()
    {
        Logger.Info($"Loading theme file: {_themeFilePath}");

        if (!File.Exists(_themeFilePath))
        {
            Logger.Warn("theme.json not found, using defaults");
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_themeFilePath);
            _theme = ThemeDefinition.Load(json);
            _settings.Set("theme-name", _theme.Active);
            Logger.Info($"Theme loaded. Active={_theme.Active}, Colors={_theme.Colors.Count}, Sizes={_theme.Sizes.Count}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load theme: {ex.GetType().Name}: {ex.Message}");
        }
    }

    public void ApplyNativeTheme(AppWindow appWindow)
    {
        try
        {
            var accentColor = ColorExtensions.ParseHex(_theme.Colors["accent"]);
            var borderColor = ColorExtensions.ParseHex(_theme.Colors["borderCrisp"]);

            appWindow.TitleBar.ButtonForegroundColor = _theme.Native.TitleBarTextColor;
            appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.ButtonHoverBackgroundColor = borderColor;
            appWindow.TitleBar.ButtonHoverForegroundColor = _theme.Native.TitleBarTextColor;
            appWindow.TitleBar.ButtonPressedBackgroundColor = accentColor;
            appWindow.TitleBar.ButtonPressedForegroundColor = Colors.White;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to apply native theme: {ex.Message}");
        }
    }

    public void ApplyXamlResources()
    {
        var themeDict = Application.Current.Resources.ThemeDictionaries;
        var dark = GetOrCreateDict(themeDict, "Dark");
        var light = GetOrCreateDict(themeDict, "Light");

        foreach (var (key, value) in _theme.Colors)
        {
            if (string.IsNullOrEmpty(value)) continue;

            // Skip "transparent" — let XAML fallback handle it to avoid parse errors
            if (value == "transparent")
            {
                SetBrushResource(dark, $"{key}Brush", Colors.Transparent);
                SetBrushResource(light, $"{key}Brush", Colors.Transparent);
                dark[$"{key}Color"] = Colors.Transparent;
                light[$"{key}Color"] = Colors.Transparent;
                Logger.Debug($"Theme resource: {key}Brush = transparent");
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
                Logger.Debug($"Theme resource: {key}Brush = {value}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to parse color '{key}': {ex.Message}");
            }
        }

        foreach (var (key, value) in _theme.Sizes)
        {
            if (key.StartsWith("radius"))
            {
                var cr = new CornerRadius(value);
                dark[$"{key}Length"] = cr;
                light[$"{key}Length"] = cr;
                Logger.Debug($"Theme resource: {key}Length = CornerRadius({value})");
            }
            else
            {
                dark[$"{key}Length"] = value;
                dark[$"{key}Thickness"] = new Thickness(value);
                light[$"{key}Length"] = value;
                light[$"{key}Thickness"] = new Thickness(value);
                Logger.Debug($"Theme resource: {key}Length = {value}");
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
        if (!themeDict.ContainsKey(key))
            themeDict[key] = new ResourceDictionary();
        return (ResourceDictionary)themeDict[key];
    }

    public async Task SetThemeAsync(string themeName)
    {
        try
        {
            var json = await File.ReadAllTextAsync(_themeFilePath);
            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            if (node is System.Text.Json.Nodes.JsonObject obj)
            {
                obj["active"] = themeName;
                var updated = obj.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_themeFilePath, updated);
            }
            await ReloadThemeAsync();
            ApplyXamlResources();
            if (App.MainWindow?.Content is FrameworkElement rootEl)
            {
                var current = rootEl.RequestedTheme;
                rootEl.RequestedTheme = current == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
                rootEl.RequestedTheme = current;
            }
            NotifyThemeChanged();
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to set theme '{themeName}': {ex.Message}");
        }
    }

    public Dictionary<string, string> GetCssVariables()
    {
        var vars = new Dictionary<string, string>();
        foreach (var (key, value) in _theme.Colors)
        {
            var cssKey = "--" + string.Concat(key.Select((c, i) =>
                char.IsUpper(c) && i > 0 ? "-" + char.ToLowerInvariant(c).ToString() : char.ToLowerInvariant(c).ToString()));
            vars[cssKey] = value;
        }
        foreach (var (key, value) in _theme.Sizes)
        {
            var cssKey = "--" + string.Concat(key.Select((c, i) =>
                char.IsUpper(c) && i > 0 ? "-" + char.ToLowerInvariant(c).ToString() : char.ToLowerInvariant(c).ToString()));
            vars[cssKey] = key.StartsWith("radius") ? $"{value}px" : $"{value}px";
        }
        return vars;
    }

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
