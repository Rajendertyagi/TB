namespace TB.Controls;

/// <summary>
/// Single source of truth for all tab-strip dimensional constants.
/// This file contains numbers only — no logic, no dependencies, no imports.
/// </summary>
public static class LayoutConst
{
    // ── Tab sizing ────────────────────────────────────────────────────────

    /// <summary>Maximum tab width before compression begins (Stage 1 start).</summary>
    public const double MaxTabWidth = 180;

    /// <summary>Minimum tab width floor — tabs never shrink below this. 32px holds a 16×16 favicon.</summary>
    public const double MinTabWidth = 32;

    /// <summary>
    /// Width at which the tab enters Squashed state.
    /// Below this threshold title text is hidden and only the favicon is shown.
    /// </summary>
    public const double SquashedTabWidth = 56;

    /// <summary>Gap between adjacent tabs in logical pixels.</summary>
    public const double TabGap = 2;

    /// <summary>Tab row height in logical pixels.</summary>
    public const double TabHeight = 28;

    /// <summary>Width of the new-tab (+) button including its left margin.</summary>
    public const double NewTabButtonWidth = 36;

    // ── Chrome reservations ───────────────────────────────────────────────

    /// <summary>
    /// Minimum drag rail width that must always remain to the left of the window controls.
    /// Users can drag the window from this zone even when the strip is fully packed.
    /// </summary>
    public const double MinimumDragRegionWidth = 40;

    /// <summary>
    /// Fallback window-control zone width used when the OS value is unavailable (e.g., before first layout pass).
    /// At 96 DPI, Win11 caption buttons are ~138px logical. 150px is a safe margin.
    /// </summary>
    public const double WindowControlWidth = 150;
}
