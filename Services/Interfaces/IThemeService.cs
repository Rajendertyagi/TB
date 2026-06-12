using Microsoft.UI.Windowing;
using TB.Models;

namespace TB.Services.Interfaces;

public interface IThemeService
{
    ThemeDefinition CurrentTheme { get; }
    string ActiveThemeName { get; }
    event Action? ThemeChanged;
    void NotifyThemeChanged();
    IReadOnlyList<ThemeInfo> GetAvailableThemes();
    void ApplyNativeTheme(AppWindow appWindow);
    void ApplyXamlResources();
    void CycleTheme();
    Task ReloadThemeAsync();
    Task SetThemeAsync(string themeName);
    Dictionary<string, string> GetCssVariables();
}
