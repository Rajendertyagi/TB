#pragma warning disable CS8601, CS8602, CS8603, CS8604

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using TB.Helpers;
using TB.Infrastructure;
using TB.Models;
using TB.Services.Interfaces;
using Windows.UI;

namespace TB.Services;

public class ThemeService : IThemeService, IDisposable
{
    private readonly string _themesDirectory;
    private readonly ISettingsService _settings;
    private readonly DispatcherQueue _dispatcherQueue;

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
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread()!;

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

            _watcher.Changed += (_, _) => _dispatcherQueue.TryEnqueue(() => OnFileChanged());
            _watcher.Created += (_, _) => _dispatcherQueue.TryEnqueue(() => OnFileChanged());
            _watcher.Renamed += (_, _) => _dispatcherQueue.TryEnqueue(() => OnFileChanged());
            _watcher.EnableRaisingEvents = true;
        }
        catch (Exception ex) { Logger.Warning($"Theme watcher failed: {ex.Message}"); }
    }

    private async Task ReloadAndApplyAsync()
    {
        try
        {
            await Task.Delay(100); // Debounce file watcher events
            await ReloadThemeAsync();

            _dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    ApplyXamlResources();

                    if (App.MainWindow != null)
                        ApplyNativeTheme(App.MainWindow.AppWindow);

                    NotifyThemeChanged();
                }
                catch (Exception ex)
                {
                    Logger.Error("Theme apply failed", ex);
                }
            });
        }
        catch (Exception ex)
        {
            Logger.Error("Theme Hot Reload Engine", ex);
        }
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
            Logger.Error($"Failed to parse theme file", ex);
            _theme = new ThemeDefinition();
        }
    }

    public void ApplyNativeTheme(AppWindow appWindow)
    {
        try
        {
            var textHex = _theme.Native.TitleBarText;
            if (string.IsNullOrEmpty(textHex))
                _theme.Colors.TryGetValue("textMain", out textHex);

            var hoverHex = _theme.Native.TitleBarIconHover;
            if (string.IsNullOrEmpty(hoverHex))
                _theme.Colors.TryGetValue("accent", out hoverHex);

            _theme.Colors.TryGetValue("accent", out var accentHex);
            _theme.Colors.TryGetValue("borderCrisp", out var borderHex);

            var textColor = textHex != null ? ColorExtensions.ParseHex(textHex) : Colors.White;
            var hoverColor = hoverHex != null ? ColorExtensions.ParseHex(hoverHex) : Colors.White;
            var accentColor = accentHex != null ? ColorExtensions.ParseHex(accentHex) : Colors.Transparent;
            var borderColor = borderHex != null ? ColorExtensions.ParseHex(borderHex) : Colors.Transparent;

            appWindow.TitleBar.ButtonForegroundColor = textColor;
            appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.ButtonInactiveForegroundColor = textColor;
            appWindow.TitleBar.ButtonHoverBackgroundColor = borderColor;
            appWindow.TitleBar.ButtonHoverForegroundColor = hoverColor;
            appWindow.TitleBar.ButtonPressedBackgroundColor = accentColor;
            appWindow.TitleBar.ButtonPressedForegroundColor = Colors.White;
        }
        catch (Exception ex) { Logger.Error("Native theme failed", ex); }
    }

    public void ApplyXamlResources()
    {
        foreach (var (key, value) in _theme.Colors)
        {
            if (string.IsNullOrEmpty(value)) continue;

            Color color;
            if (value.Equals("transparent", StringComparison.OrdinalIgnoreCase))
                color = Colors.Transparent;
            else if (!value.StartsWith("#") || value.Length < 4)
                continue;
            else
            {
                try { color = ColorExtensions.ParseHex(value); }
                catch (Exception ex) { Logger.Error($"Parse color '{key}'", ex); continue; }
            }

            SetBrushResource(key, color);
        }

        if (_theme.Colors.TryGetValue("bgApp", out var bgAppHex) && !string.IsNullOrEmpty(bgAppHex))
        {
            try { SetBrushResource("commandPaletteBackdrop", ColorExtensions.ParseHex(bgAppHex)); }
            catch (Exception ex) { Logger.Error("Parse bgApp for commandPaletteBackdrop", ex); }
            try { SetBrushResource("commandPaletteCardBg", ColorExtensions.ParseHex(bgAppHex)); }
            catch (Exception ex) { Logger.Error("Parse bgApp for commandPaletteCardBg", ex); }
        }
        if (_theme.Colors.TryGetValue("accent", out var accentHex) && !string.IsNullOrEmpty(accentHex))
        {
            try { SetBrushResource("commandPaletteBorder", ColorExtensions.ParseHex(accentHex)); }
            catch (Exception ex) { Logger.Error("Parse accent for commandPaletteBorder", ex); }
            try { SetBrushResource("commandPaletteShortcutBg", ColorExtensions.ParseHex(accentHex)); }
            catch (Exception ex) { Logger.Error("Parse accent for commandPaletteShortcutBg", ex); }
        }
    }

    private static void SetBrushResource(string key, Color color)
    {
        var brushKey = $"{key}Brush";
        if (Application.Current.Resources.TryGetValue(brushKey, out var resource) && resource is SolidColorBrush brush)
            brush.Color = color;
        else
            Logger.Warning($"Theme brush not found: {brushKey}");
    }

    public async Task SetThemeAsync(string themeName)
    {
        _settings.Set("theme-name", themeName);
        await ReloadAndApplyAsync();
    }

    public Dictionary<string, string> GetCssVariables()
    {
        Dictionary<string, string> vars = [];

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

    private static string ToKebabCase(string str) =>
        string.Concat(str.Select((c, i) =>
            char.IsUpper(c) && i > 0
                ? "-" + char.ToLowerInvariant(c).ToString()
                : char.ToLowerInvariant(c).ToString()));

    public IReadOnlyList<ThemeInfo> GetAvailableThemes()
    {
        try
        {
            if (!Directory.Exists(_themesDirectory)) return [new ThemeInfo(Defaults.Theme, ToDisplayName(Defaults.Theme))];
            return Directory.EnumerateFiles(_themesDirectory, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new ThemeInfo(x!, ToDisplayName(x!)))
                .ToList();
        }
        catch (Exception ex)
        {
            Logger.Warning($"Failed to enumerate themes: {ex.Message}");
            return [new ThemeInfo(Defaults.Theme, ToDisplayName(Defaults.Theme))];
        }
    }

    private static string ToDisplayName(string id)
    {
        if (string.IsNullOrEmpty(id)) return "Unknown";
        return string.Join(' ', id.Split('-', '_').Select(w =>
            w.Length > 0 ? char.ToUpper(w[0]) + w[1..] : w));
    }

    public void Dispose()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher = null;
        }
    }

    public void NotifyThemeChanged() => ThemeChanged?.Invoke();

    public void CycleTheme()
    {
        var current = _settings.Get("theme-mode", "dark") ?? "dark";
        var next = current == "dark" ? "light" : "dark";
        _settings.Set("theme-mode", next);

        if (App.MainWindow is Window window && window.Content is FrameworkElement root)
        {
            root.RequestedTheme = next == "light" ? ElementTheme.Light : ElementTheme.Dark;
        }

        NotifyThemeChanged();
    }
}
