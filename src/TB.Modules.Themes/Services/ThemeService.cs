using TB.Modules.Themes.Contracts;
using TB.Modules.Themes.Models;

namespace TB.Modules.Themes.Services;

public sealed class ThemeService : IThemeService
{
    private readonly ThemeSettings _settings =
        new();

    public ThemeSettings Load()
    {
        return _settings;
    }

    public ThemeDefinition GetCurrentTheme()
    {
        return new ThemeDefinition
        {
            Name = "HeliumDark",

            FontFamily = "Inter",

            TabHeight = 28,

            ToolbarHeight = 32,

            AddressBarHeight = 32,

            CornerRadius = 10,

            BackgroundColor = "#111111",

            SurfaceColor = "#1A1A1A",

            SurfaceHoverColor = "#242424",

            AccentColor = "#7C5CFF",

            TextColor = "#FFFFFF"
        };
    }

    public void Apply(
        string themeName)
    {
        _settings.CurrentTheme =
            themeName;
    }
}
