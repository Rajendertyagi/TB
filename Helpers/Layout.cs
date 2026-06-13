namespace TB.Helpers;

public static class Layout
{
    public const double TabbarHeight = 32;
    public const double NavHeight = 36;
    public const double TabHeight = 28;
    public const double UrlbarHeight = 28;
    public const double RadiusSm = 6;
    public const double RadiusMd = 8;
    public const double RadiusLg = 20;
    public const double TabInterTabGap = 2;
    public const double TabMinWidth = 32;
    public const double TabMaxWidth = 180;
    // Title text hidden when tab is ≤ this width; only favicon shown (centered)
    public const double TabSquashThreshold = 56;
    // Close button hidden when tab is ≤ this width (Helium spec: ≤36px)
    public const double TabCloseHideThreshold = 36;
    // Minimum drag rail to the left of the window controls in the tab strip row.
    // Users can always drag the window from this zone even when the strip is packed.
    public const double TabStripDragRailWidth = 40;
}
