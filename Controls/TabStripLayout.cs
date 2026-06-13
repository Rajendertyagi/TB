namespace TB.Controls;

/// <summary>
/// Pure tab-width calculator and compression-state engine.
///
/// Inputs:  Available width (from WindowChromeLayout), tab count.
/// Outputs: TabWidth (logical px), CompressionState.
///
/// No UI references. No window/OS APIs. No ViewModel dependencies.
/// Called exclusively by TabStrip.xaml.cs, which then applies the results to the UI.
/// </summary>
public static class TabStripLayout
{
    /// <summary>Result returned by <see cref="Compute"/>.</summary>
    public readonly struct Result
    {
        /// <summary>Width every tab should be set to (logical pixels).</summary>
        public double TabWidth { get; init; }

        /// <summary>Compression state that applies to ALL tabs at this width.</summary>
        public TabCompressionState CompressionState { get; init; }

        /// <summary>True when tabs have hit MinTabWidth and overflow is imminent.</summary>
        public bool IsOverflow { get; init; }
    }

    /// <summary>
    /// Computes tab width and compression state from the available strip width and tab count.
    ///
    /// Compression stages (in order):
    ///   Stage 1: width between MaxTabWidth and SquashedTabWidth   → Normal / Compressed
    ///   Stage 2: width between SquashedTabWidth and MinTabWidth   → Squashed (title hidden)
    ///   Stage 3: width == MinTabWidth and tabs still don't fit    → Overflow (floor hit)
    /// </summary>
    /// <param name="availableWidth">
    ///     Logical pixel width available to the tab strip.
    ///     Must already exclude window controls and drag rail (from WindowChromeLayout).
    /// </param>
    /// <param name="tabCount">Number of tabs currently open.</param>
    public static Result Compute(double availableWidth, int tabCount)
    {
        if (tabCount <= 0 || availableWidth <= 0)
            return new Result { TabWidth = LayoutConst.MaxTabWidth, CompressionState = TabCompressionState.Normal };

        // Total gap between tabs (N-1 gaps for N tabs)
        double totalGap = (tabCount - 1) * LayoutConst.TabGap;

        // Space left for tabs after reserving the new-tab (+) button and gaps
        double spaceForTabs = availableWidth - LayoutConst.NewTabButtonWidth - totalGap;

        // Raw per-tab width before clamping
        double rawWidth = spaceForTabs / tabCount;

        // Clamp to [MinTabWidth, MaxTabWidth]
        double tabWidth = System.Math.Clamp(rawWidth, LayoutConst.MinTabWidth, LayoutConst.MaxTabWidth);

        // Determine compression state from the clamped width
        TabCompressionState state;
        bool isOverflow = false;

        if (tabWidth >= LayoutConst.SquashedTabWidth)
        {
            // Stage 1: Normal or shrinking — title still visible
            state = tabWidth >= LayoutConst.MaxTabWidth
                ? TabCompressionState.Normal
                : TabCompressionState.Compressed;
        }
        else if (tabWidth > LayoutConst.MinTabWidth)
        {
            // Stage 2: Title hidden, favicon-only
            state = TabCompressionState.Squashed;
        }
        else
        {
            // Stage 3: Hit the absolute floor — overflow begins
            state = TabCompressionState.Overflow;
            isOverflow = rawWidth < LayoutConst.MinTabWidth;   // true only when truly overflowing
        }

        return new Result
        {
            TabWidth         = tabWidth,
            CompressionState = state,
            IsOverflow       = isOverflow
        };
    }
}
