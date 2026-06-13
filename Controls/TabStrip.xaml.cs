using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using TB.Infrastructure;
using TB.ViewModels;
using Windows.Foundation;

namespace TB.Controls;

/// <summary>
/// Tab strip UI bridge.
///
/// Responsibilities:
///   1. Receive chrome dimensions from MainWindow (via SetChromeMetrics).
///   2. Call WindowChromeLayout → TabStripLayout to compute width + state.
///   3. Apply computed values to the XAML ItemsRepeater buttons and TabItemViewModels.
///   4. Wire pointer events for hover/close/context-menu.
///
/// Must NOT: calculate available width, read AppWindow properties, or own compression logic.
/// </summary>
public sealed partial class TabStrip : UserControl
{
    // ── Anti-jiggle ───────────────────────────────────────────────────────
    // Set by MainWindow when the pointer enters/exits the chrome container.
    // Defers resize recalc to avoid tab jitter while hovering.
    public bool IsMouseInChrome { get; set; }

    // ── Chrome metrics (set once per resize by MainWindow) ────────────────
    private double _reservedRightWidth;   // logical px; used for ReservedRightColumn

    // ── Constructor ───────────────────────────────────────────────────────
    public TabStrip()
    {
        this.InitializeComponent();
        TabRepeater.ElementPrepared += OnTabElementPrepared;
        TabRepeater.ElementClearing += OnTabElementClearing;
        this.SizeChanged            += OnSizeChanged;
    }

    // ── Public API called by MainWindow ───────────────────────────────────

    /// <summary>
    /// Called by MainWindow on every Loaded / SizeChanged / DPI change.
    /// Receives OS-level measurements and applies the full layout pipeline.
    /// </summary>
    /// <param name="rightInsetRaw">AppWindow.TitleBar.RightInset (raw screen px).</param>
    /// <param name="scale">XamlRoot.RasterizationScale.</param>
    public void SetChromeMetrics(int rightInsetRaw, double scale)
    {
        _reservedRightWidth = WindowChromeLayout.ComputeReservedRightWidth(rightInsetRaw, scale);
        ReservedRightColumn.Width = new GridLength(_reservedRightWidth);
        ApplyLayout();
    }

    // ── Layout pipeline ───────────────────────────────────────────────────

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (IsMouseInChrome)
            DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, ApplyLayout);
        else
            ApplyLayout();
    }

    /// <summary>
    /// The one function that drives the entire tab-strip layout.
    /// Called from: SizeChanged, SetChromeMetrics, TabCollection changes, ElementPrepared.
    /// </summary>
    public void ApplyLayout()
    {
        if (DataContext is not ChromeViewModel chrome) return;

        // ── Step 1: Compute available width ───────────────────────────────
        // TabStripGrid.ActualWidth already excludes ReservedRightColumn because
        // the column is a fixed-width sibling. But we subtract anyway for safety
        // in case ActualWidth is measured before the column is updated.
        double stripWidth = TabStripGrid.ActualWidth;
        double availableWidth = Math.Max(0, stripWidth - _reservedRightWidth);

        if (availableWidth <= 0) return;

        // ── Step 2: Run compression algorithm ────────────────────────────
        var result = TabStripLayout.Compute(availableWidth, chrome.Tabs.Count);

        // ── Step 3: Apply width to every realized tab button ─────────────
        int count = TabRepeater.ItemsSourceView?.Count ?? 0;
        for (int i = 0; i < count; i++)
        {
            if (TabRepeater.TryGetElement(i) is Button btn)
                btn.Width = result.TabWidth;
        }

        // ── Step 4: Update compression state on every ViewModel ──────────
        foreach (var tab in chrome.Tabs)
            tab.CompressionState = result.CompressionState;
    }

    // ── Interaction ───────────────────────────────────────────────────────

    private void OnTabElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is not Button btn) return;

        // Stamp width immediately on prepare (before the next SizeChanged fires)
        DispatcherQueue.TryEnqueue(() =>
        {
            if (DataContext is ChromeViewModel vm && vm.Tabs.Count > 0)
            {
                var result = TabStripLayout.Compute(
                    Math.Max(0, TabStripGrid.ActualWidth - _reservedRightWidth),
                    vm.Tabs.Count);
                btn.Width = result.TabWidth;
                if (btn.DataContext is TabItemViewModel tabVm)
                    tabVm.CompressionState = result.CompressionState;
            }
        });

        btn.PointerEntered += OnTabPointerEntered;
        btn.PointerExited  += OnTabPointerExited;
        btn.PointerPressed += OnTabPointerPressed;
    }

    private void OnTabElementClearing(ItemsRepeater sender, ItemsRepeaterElementClearingEventArgs args)
    {
        if (args.Element is not Button btn) return;
        btn.PointerEntered -= OnTabPointerEntered;
        btn.PointerExited  -= OnTabPointerExited;
        btn.PointerPressed -= OnTabPointerPressed;
    }

    private void OnTabPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is TabItemViewModel vm)
            vm.IsHovered = true;
    }

    private void OnTabPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is TabItemViewModel vm)
            vm.IsHovered = false;
    }

    private void OnTabPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        var pt = e.GetCurrentPoint(btn);
        if (!pt.Properties.IsRightButtonPressed) return;
        e.Handled = true;
        if (btn.DataContext is TabItemViewModel tabVm && DataContext is ChromeViewModel chromeVm)
            ShowTabContextMenu(btn, tabVm, chromeVm, pt.Position);
    }

    // ── Context menu ──────────────────────────────────────────────────────

    private void ShowTabContextMenu(FrameworkElement element, TabItemViewModel vm, ChromeViewModel chrome, Point point)
    {
        try
        {
            var presenterStyle = (Style)Application.Current.Resources["TbMenuFlyoutPresenterStyle"];
            var itemStyle      = (Style)Application.Current.Resources["TbMenuFlyoutItemStyle"];
            var flyout = new MenuFlyout { MenuFlyoutPresenterStyle = presenterStyle };
            flyout.Items.Add(new MenuFlyoutItem { Text = "New Tab",          Command = vm.NewTabCommand,          Style = itemStyle });
            flyout.Items.Add(new MenuFlyoutItem { Text = "Duplicate Tab",    Command = vm.DuplicateCommand,       Style = itemStyle });
            flyout.Items.Add(new MenuFlyoutItem { Text = "Reload",           Command = vm.ReloadTabCommand,       Style = itemStyle });
            flyout.Items.Add(new MenuFlyoutSeparator());
            flyout.Items.Add(new MenuFlyoutItem { Text = "Close",            Command = vm.CloseCommand,           Style = itemStyle });
            flyout.Items.Add(new MenuFlyoutItem { Text = "Close Other Tabs", Command = vm.CloseOtherTabsCommand,  Style = itemStyle });
            flyout.ShowAt(element, point);
        }
        catch (Exception ex) { Logger.Warning($"Tab context menu: {ex.Message}"); }
    }
}