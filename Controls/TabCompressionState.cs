namespace TB.Controls;

/// <summary>
/// Compression state of a single tab.
/// Drives all visibility decisions in TabItemViewModel and TabStrip XAML.
/// Progress: Normal → Compressed → Squashed → Overflow.
/// </summary>
public enum TabCompressionState
{
    /// <summary>
    /// Tab is at or near MaxTabWidth. Full UI: favicon + title + close button (on hover/active).
    /// Triggered when width ≥ LayoutConst.SquashedTabWidth.
    /// </summary>
    Normal,

    /// <summary>
    /// Tab is compressing — narrower than MaxTabWidth but still wide enough for title.
    /// Title may be clipped by TextTrimming. Close button shows on hover / active.
    /// Triggered when LayoutConst.SquashedTabWidth &lt; width &lt; LayoutConst.MaxTabWidth.
    /// </summary>
    Compressed,

    /// <summary>
    /// Tab has reached favicon-only territory (≤ LayoutConst.SquashedTabWidth).
    /// Title hidden. Close button: shows only when hovered (centered over favicon slot).
    /// Triggered when LayoutConst.MinTabWidth &lt; width ≤ LayoutConst.SquashedTabWidth.
    /// </summary>
    Squashed,

    /// <summary>
    /// Tab has hit the absolute floor (MinTabWidth). No further compression is possible.
    /// A scroll or overflow indicator should be shown at this point (future work).
    /// Triggered when tab count × MinTabWidth &gt; AvailableTabStripWidth.
    /// </summary>
    Overflow
}
