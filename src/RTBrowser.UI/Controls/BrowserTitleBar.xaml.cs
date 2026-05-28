using RTBrowser.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RTBrowser.UI.Controls
{
    public partial class BrowserTitleBar : UserControl
    {
        public event Action? NewTabRequested;

        public event Action<Guid>? CloseTabRequested;

        public event Action<Guid>? TabSelected;

        private readonly Dictionary<Guid, Border> _tabBorders =
            new();

        public BrowserTitleBar()
        {
            InitializeComponent();
        }

        public void RenderTabs(
            IReadOnlyList<BrowserTab> tabs)
        {
            TabsHost.Children.Clear();

            _tabBorders.Clear();

            foreach (BrowserTab tab in tabs)
            {
                Border border =
                    CreateTab(tab);

                _tabBorders.Add(
                    tab.Id,
                    border);

                TabsHost.Children.Add(border);
            }
        }

        private Border CreateTab(
            BrowserTab tab)
        {
            Border border =
                new()
                {
                    Width = 210,
                    Height = 24,
                    Margin = new Thickness(0, 0, 4, 0),
                    CornerRadius = new CornerRadius(5, 5, 0, 0),
                    BorderThickness = new Thickness(1),
                    Background =
                        tab.IsActive
                            ? Brush("#18191C")
                            : Brush("#111214"),
                    BorderBrush =
                        tab.IsActive
                            ? Brush("#2A2D32")
                            : Brush("#1B1D21"),
                    Cursor = Cursors.Hand,
                    Tag = tab.Id
                };

            Grid grid =
                new();

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = GridLength.Auto
                });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = GridLength.Auto
                });

            Border indicator =
                new()
                {
                    Width = 6,
                    Height = 6,
                    Margin = new Thickness(8, 0, 6, 0),
                    CornerRadius = new CornerRadius(3),
                    VerticalAlignment =
                        VerticalAlignment.Center,
                    Background =
                        tab.IsLoading
                            ? Brush("#FFB347")
                            : tab.IsActive
                                ? Brush("#4C8DFF")
                                : Brush("#44474E")
                };

            TextBlock title =
                new()
                {
                    Text =
                        string.IsNullOrWhiteSpace(tab.Title)
                            ? "New Tab"
                            : tab.Title,
                    FontSize = 10.5,
                    FontWeight = FontWeights.Medium,
                    VerticalAlignment =
                        VerticalAlignment.Center,
                    Foreground =
                        tab.IsActive
                            ? Brush("#ECECEC")
                            : Brush("#9A9EA5"),
                    TextTrimming =
                        TextTrimming.CharacterEllipsis
                };

            Button closeButton =
                new()
                {
                    Width = 16,
                    Height = 16,
                    Margin = new Thickness(0, 0, 6, 0),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Focusable = false,
                    Tag = tab.Id
                };

            TextBlock closeText =
                new()
                {
                    Text = "✕",
                    FontSize = 8,
                    HorizontalAlignment =
                        HorizontalAlignment.Center,
                    VerticalAlignment =
                        VerticalAlignment.Center,
                    Foreground =
                        tab.IsActive
                            ? Brush("#9DA3AB")
                            : Brush("#6F737A")
                };

            closeButton.Content =
                closeText;

            closeButton.Click +=
                OnDynamicCloseTab;

            border.MouseLeftButtonDown +=
                OnTabClicked;

            border.MouseEnter +=
                (_, _) =>
                {
                    if (tab.IsActive)
                    {
                        return;
                    }

                    border.Background =
                        Brush("#17181B");
                };

            border.MouseLeave +=
                (_, _) =>
                {
                    if (tab.IsActive)
                    {
                        return;
                    }

                    border.Background =
                        Brush("#111214");
                };

            Grid.SetColumn(indicator, 0);
            Grid.SetColumn(title, 1);
            Grid.SetColumn(closeButton, 2);

            grid.Children.Add(indicator);
            grid.Children.Add(title);
            grid.Children.Add(closeButton);

            border.Child = grid;

            return border;
        }

        private void OnTabClicked(
            object sender,
            MouseButtonEventArgs e)
        {
            if (sender is not Border border)
            {
                return;
            }

            if (border.Tag is not Guid tabId)
            {
                return;
            }

            TabSelected?.Invoke(tabId);
        }

        private void OnDynamicCloseTab(
            object sender,
            RoutedEventArgs e)
        {
            e.Handled = true;

            if (sender is not Button button)
            {
                return;
            }

            if (button.Tag is not Guid tabId)
            {
                return;
            }

            CloseTabRequested?.Invoke(tabId);
        }

        private Brush Brush(
            string hex)
        {
            return
                (Brush)new BrushConverter()
                    .ConvertFrom(hex)!;
        }

        private void OnDragWindow(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            Window.GetWindow(this)?.DragMove();
        }

        private void OnMinimize(
            object sender,
            RoutedEventArgs e)
        {
            Window? window =
                Window.GetWindow(this);

            if (window == null)
            {
                return;
            }

            window.WindowState =
                WindowState.Minimized;
        }

        private void OnMaximize(
            object sender,
            RoutedEventArgs e)
        {
            Window? window =
                Window.GetWindow(this);

            if (window == null)
            {
                return;
            }

            window.WindowState =
                window.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
        }

        private void OnClose(
            object sender,
            RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void OnNewTab(
            object sender,
            RoutedEventArgs e)
        {
            NewTabRequested?.Invoke();
        }
    }
}
