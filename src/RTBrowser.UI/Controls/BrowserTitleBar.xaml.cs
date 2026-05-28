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

                _tabBorders[tab.Id] =
                    border;

                TabsHost.Children.Add(border);
            }
        }

        private Border CreateTab(
            BrowserTab tab)
        {
            Border border =
                new()
                {
                    Width = 220,
                    Height = 24,
                    Margin = new Thickness(0, 0, 5, 0),
                    CornerRadius = new CornerRadius(5, 5, 0, 0),
                    BorderThickness = new Thickness(1),
                    Background =
                        tab.IsActive
                            ? Brush("#18191C")
                            : Brush("#121316"),
                    BorderBrush =
                        tab.IsActive
                            ? Brush("#2E3136")
                            : Brush("#1E2024"),
                    Tag = tab.Id,
                    Cursor = Cursors.Hand
                };

            Grid grid =
                new();

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = GridLength.Auto
                });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition());

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = GridLength.Auto
                });

            Border indicator =
                new()
                {
                    Width = 7,
                    Height = 7,
                    Margin = new Thickness(8, 0, 6, 0),
                    CornerRadius = new CornerRadius(4),
                    Background =
                        tab.IsActive
                            ? Brush("#4C8DFF")
                            : Brush("#43464C"),
                    VerticalAlignment =
                        VerticalAlignment.Center
                };

            TextBlock title =
                new()
                {
                    Text = tab.Title,
                    FontSize = 11,
                    Foreground =
                        tab.IsActive
                            ? Brush("#E7E7E7")
                            : Brush("#9CA1A9"),
                    VerticalAlignment =
                        VerticalAlignment.Center,
                    TextTrimming =
                        TextTrimming.CharacterEllipsis
                };

            Button close =
                new()
                {
                    Width = 18,
                    Height = 18,
                    Margin = new Thickness(0, 0, 6, 0),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Tag = tab.Id,
                    Content =
                        new TextBlock
                        {
                            Text = "✕",
                            FontSize = 9,
                            Foreground =
                                Brush("#8E949D"),
                            HorizontalAlignment =
                                HorizontalAlignment.Center,
                            VerticalAlignment =
                                VerticalAlignment.Center
                        }
                };

            close.Click += OnDynamicCloseTab;

            border.MouseEnter +=
                (_, _) =>
                {
                    if (!tab.IsActive)
                    {
                        border.Background =
                            Brush("#17181B");
                    }
                };

            border.MouseLeave +=
                (_, _) =>
                {
                    if (!tab.IsActive)
                    {
                        border.Background =
                            Brush("#121316");
                    }
                };

            Grid.SetColumn(indicator, 0);
            Grid.SetColumn(title, 1);
            Grid.SetColumn(close, 2);

            grid.Children.Add(indicator);
            grid.Children.Add(title);
            grid.Children.Add(close);

            border.Child = grid;

            border.MouseLeftButtonDown +=
                OnTabClicked;

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
