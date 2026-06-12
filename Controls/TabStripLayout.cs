using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using TB.ViewModels;
using Windows.Foundation;

namespace TB.Controls;

public partial class TabStripLayout : NonVirtualizingLayout
{
    protected override Size MeasureOverride(NonVirtualizingLayoutContext context, Size availableSize)
    {
        int tabCount = context.Children.Count;
        if (tabCount == 0) return new Size(0, 30);

        double totalGap = (tabCount - 1) * 2;
        double spaceForTabs = availableSize.Width - 40 - totalGap;
        double finalWidth = Math.Clamp(spaceForTabs / tabCount, 40, 220);

        foreach (var child in context.Children)
        {
            child.Measure(new Size(finalWidth, 30));
        }
        return new Size(availableSize.Width, 30);
    }

    protected override Size ArrangeOverride(NonVirtualizingLayoutContext context, Size finalSize)
    {
        double x = 0;
        foreach (var child in context.Children)
        {
            double width = child.DesiredSize.Width;
            child.Arrange(new Rect(x, 0, width, 30));

            if (child is FrameworkElement fe && fe.DataContext is TabItemViewModel tab)
                tab.IsSquashed = width <= 40;

            x += width + 2;
        }
        return finalSize;
    }
}
