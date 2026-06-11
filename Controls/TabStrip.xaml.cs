using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using TB.Helpers;
using TB.Infrastructure;
using TB.ViewModels;
using Windows.Foundation;

namespace TB.Controls;

public sealed partial class TabStrip : UserControl
{
    public TabStrip()
    {
        this.InitializeComponent();
        TabRepeater.ElementPrepared += OnTabElementPrepared;

        // ADDED: This is what actually triggers the tabs to shrink/expand when resizing the window
        this.SizeChanged += (_, _) => RecalculateTabWidths();
    }

    public void RecalculateTabWidths()
    {
        // ADDED: Small safety check (TabStripGrid.ActualWidth > 0) to prevent negative math errors
        if (DataContext is ChromeViewModel chrome && TabStripGrid.ActualWidth > 0)
        {
            chrome.RecalculateTabWidth(TabStripGrid.ActualWidth);
            double w = chrome.TabWidth;
            int count = TabRepeater.ItemsSourceView?.Count ?? 0;
            for (int i = 0; i < count; i++)
            {
                if (TabRepeater.TryGetElement(i) is Button btn)
                    btn.Width = w;
            }
            foreach (var tab in chrome.Tabs)
                tab.IsSquashed = w <= 36;
        }
    }

    private void OnTabElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is Button btn)
        {
            // ADDED: Defers the width calculation for a split second so the UI thread doesn't choke on new tabs
            DispatcherQueue.TryEnqueue(() =>
            {
                if (DataContext is ChromeViewModel vm) btn.Width = vm.TabWidth;
            });

            btn.PointerEntered += (_, _) =>
            {
                if (btn.DataContext is TabItemViewModel tabVm) tabVm.IsHovered = true;
            };
            btn.PointerExited += (_, _) =>
            {
                if (btn.DataContext is TabItemViewModel tabVm) tabVm.IsHovered = false;
            };
            btn.PointerPressed += (_, e) =>
            {
                try
                {
                    var pt = e.GetCurrentPoint(btn);
                    if (pt.Properties.IsRightButtonPressed)
                    {
                        e.Handled = true;
                        if (btn.DataContext is TabItemViewModel tabVm && DataContext is ChromeViewModel chromeVm)
                            ShowTabContextMenu(btn, tabVm, chromeVm, pt.Position);
                    }
                }
                catch (Exception ex) { Logger.Warning($"Tab pointer handler: {ex.Message}"); }
            };
        }
    }

    private void ShowTabContextMenu(FrameworkElement element, TabItemViewModel vm, ChromeViewModel chrome, Point point)
    {
        try
        {
            var presenterStyle = (Style)Application.Current.Resources["HeliumMenuFlyoutPresenterStyle"];
            var itemStyle = (Style)Application.Current.Resources["HeliumMenuFlyoutItemStyle"];
            var flyout = new MenuFlyout { MenuFlyoutPresenterStyle = presenterStyle };
            flyout.Items.Add(new MenuFlyoutItem { Text = "New Tab", Command = vm.NewTabCommand, Style = itemStyle });
            flyout.Items.Add(new MenuFlyoutItem { Text = "Duplicate Tab", Command = vm.DuplicateCommand, Style = itemStyle });
            flyout.Items.Add(new MenuFlyoutItem { Text = "Reload", Command = vm.ReloadTabCommand, Style = itemStyle });
            flyout.Items.Add(new MenuFlyoutSeparator());
            flyout.Items.Add(new MenuFlyoutItem { Text = "Close", Command = vm.CloseCommand, Style = itemStyle });
            flyout.Items.Add(new MenuFlyoutItem { Text = "Close Other Tabs", Command = vm.CloseOtherTabsCommand, Style = itemStyle });
            flyout.ShowAt(element, point);
        }
        catch (Exception ex) { Logger.Warning($"Tab context menu failed: {ex.Message}"); }
    }
}