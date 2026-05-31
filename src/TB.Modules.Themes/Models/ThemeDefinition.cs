namespace TB.Modules.Themes.Models;

public sealed class ThemeDefinition
{
    public string Name { get; set; } = string.Empty;

    public string FontFamily { get; set; } = "Inter";

    public double TabHeight { get; set; }

    public double ToolbarHeight { get; set; }

    public double AddressBarHeight { get; set; }

    public double CornerRadius { get; set; }

    public string BackgroundColor { get; set; } = string.Empty;

    public string SurfaceColor { get; set; } = string.Empty;

    public string SurfaceHoverColor { get; set; } = string.Empty;

    public string AccentColor { get; set; } = string.Empty;

    public string TextColor { get; set; } = string.Empty;
}
