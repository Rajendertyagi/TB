using TB.Modules.Logging.Services;
using TB.Modules.Themes.Contracts;
using TB.Modules.Themes.Models;

namespace TB.Modules.Themes.Services;

public sealed class ThemeService : IThemeService
{
    private readonly ThemeSettings _settings =
        new();

    public ThemeSettings Load()
    {
        ThemeLogger.Loaded(
            _settings.CurrentTheme);

        return _settings;
    }

    public ThemeDefinition GetCurrentTheme()
    {
        ThemeLogger.Applied(
            _settings.CurrentTheme);

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
        ThemeLogger.Changed(
            _settings.CurrentTheme,
            themeName);

        _settings.CurrentTheme =
            themeName;

        ThemeLogger.Applied(
            themeName);
    }
}


