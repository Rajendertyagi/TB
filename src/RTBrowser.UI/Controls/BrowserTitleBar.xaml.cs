using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RTBrowser.UI.Controls
{
    public partial class BrowserTitleBar : UserControl
    {
        public event Action? NewTabRequested;

        public event Action? CloseTabRequested;

        public BrowserTitleBar()
        {
            InitializeComponent();
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

        private void OnCloseTab(
            object sender,
            RoutedEventArgs e)
        {
            CloseTabRequested?.Invoke();
        }
    }
}
