namespace TB.Controls;

/// <summary>
/// Pure calculator for the width available to tabs after all chrome reservations are subtracted.
///
/// Inputs:  Window width, OS-reported window-control zone width, DPI scale.
/// Output:  AvailableTabStripWidth (logical pixels).
///
/// No UI references. No tab-count knowledge. No compression logic.
/// MainWindow feeds raw OS values; this class returns the safe budget for tabs.
/// </summary>
public static class WindowChromeLayout
{
    /// <summary>
    /// Computes the logical pixel width that the tab strip may safely occupy.
    /// </summary>
    /// <param name="windowWidthLogical">
    ///     Total logical width of the window's client area (RootGrid.ActualWidth).
    /// </param>
    /// <param name="rightInsetRaw">
    ///     AppWindow.TitleBar.RightInset — raw screen pixels reported by the OS.
    ///     Represents the width of the native window-control buttons (Min/Max/Close).
    /// </param>
    /// <param name="scale">
    ///     XamlRoot.RasterizationScale (e.g. 1.0 = 96 DPI, 1.25 = 120 DPI).
    ///     Used to convert raw px → logical px.
    /// </param>
    /// <returns>Available tab strip width in logical pixels. Always ≥ 0.</returns>
    public static double ComputeAvailableTabStripWidth(
        double windowWidthLogical,
        int    rightInsetRaw,
        double scale)
    {
        // Convert OS-reported raw pixels to logical pixels for this DPI.
        double logicalInset = scale > 0
            ? rightInsetRaw / scale
            : LayoutConst.WindowControlWidth;   // safe fallback

        // Reserve the control zone plus the minimum drag rail.
        double reserved = logicalInset + LayoutConst.MinimumDragRegionWidth;

        // Remaining space is the only budget tabs may use.
        double available = windowWidthLogical - reserved;

        return available > 0 ? available : 0;
    }

    /// <summary>
    /// Computes the logical width of the reserved right zone (window controls + drag rail).
    /// TabStrip uses this to set the width of ReservedRightColumn so tabs are
    /// structurally blocked from rendering there.
    /// </summary>
    public static double ComputeReservedRightWidth(int rightInsetRaw, double scale)
    {
        double logicalInset = scale > 0
            ? rightInsetRaw / scale
            : LayoutConst.WindowControlWidth;

        return logicalInset + LayoutConst.MinimumDragRegionWidth;
    }
}
