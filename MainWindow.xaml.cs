using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using System;

namespace TradingBrowser
{
    public partial class MainWindow : Window
    {
        private AppWindow _appWindow;

        public MainWindow()
        {
            this.InitializeComponent();
            InitializeCustomTitleBar();
        }

        private void InitializeCustomTitleBar()
        {
            // Get the window handle (HWND)
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            // Check if AppWindowTitleBar custom extension is supported on host system
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = _appWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;

                // Explicitly set native caption button themes to match Chrome Dark Mode
                titleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
                titleBar.ButtonForegroundColor = Microsoft.UI.Colors.White;
                titleBar.ButtonHoverBackgroundColor = System.Drawing.Color.FromArgb(40, 255, 255, 255).ToColor();
                titleBar.ButtonPressedBackgroundColor = System.Drawing.Color.FromArgb(60, 255, 255, 255).ToColor();

                // Connect window drag manager to the XAML top menu container
                this.Activated += (s, e) =>
                {
                    // Forces layout updates to recalculate sizing rules upon interaction change
                    AppTitleBar.Opacity = e.WindowActivationState == WindowActivationState.Deactivated ? 0.6 : 1.0;
                };
            }
        }
    }

    public static class ColorExtensions
    {
        public static Windows.UI.Color ToColor(this System.Drawing.Color color)
        {
            return Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
