using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using WinRT.Interop;

namespace TradingBrowser
{
    /// <summary>
    /// Core Window Frame Interceptor for the Trading Browser Application Shell.
    /// Handles Native Windows 11 DWM Custom Decorations and Input Router Bridging.
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppWindow _appWindow;
        private BrowserManager _browserManager;
        private NavigationController _inputController;

        public MainWindow()
        {
            this.InitializeComponent();
            
            // 1. Establish custom title bar properties and native Win11 frame buttons
            InitializeCustomTitleBar();

            // 2. Instantiate the isolated Tab & WebView2 Infrastructure Controller
            _browserManager = new BrowserManager(
                MainTabs, 
                BrowserGrid, 
                Omnibox, 
                Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()
            );

            // 3. Connect the Input Processing Engine to route hotkeys (Ctrl+T, Ctrl+W, etc.)
            _inputController = new NavigationController(this, _browserManager);

            // 4. Register event handlers for fluid layout resizing and workspace management
            RegisterWindowShellEvents();

            // 5. Spawn primary trading engine workspace on application startup
            _browserManager.CreateNewTabContext("Google Workspace", "https://www.google.com");
        }

        /// <summary>
        /// Binds the custom XAML Title Bar element directly to Desktop Window Manager (DWM) 
        /// to allow dragging while preserving standard Windows 11 title bar states.
        /// </summary>
        private void InitializeCustomTitleBar()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = _appWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;

                // Restyle native window management caption boxes to match Chrome Dark Mode
                titleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
                titleBar.ButtonForegroundColor = Microsoft.UI.Colors.White;
                titleBar.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(40, 255, 255, 255);
                titleBar.ButtonPressedBackgroundColor = Windows.UI.Color.FromArgb(60, 255, 255, 255);
                titleBar.ButtonHoverForegroundColor = Microsoft.UI.Colors.White;
                titleBar.ButtonInactiveForegroundColor = Microsoft.UI.Colors.Gray;
            }
        }

        /// <summary>
        /// Connects physical interaction events, resizing handlers, and pointer metrics 
        /// directly to the operational window shell canvas.
        /// </summary>
        private void RegisterWindowShellEvents()
        {
            // Dim custom title bar when the window loses active desktop focus
            this.Activated += (sender, args) =>
            {
                if (args.WindowActivationState == WindowActivationState.Deactivated)
                {
                    AppTitleBar.Opacity = 0.5;
                }
                else
                {
                    AppTitleBar.Opacity = 1.0;
                }
            };

            // Route auxiliary mouse pointer events (Mouse 4 / Mouse 5 side-buttons) for Back/Forward navigation
            this.Content.PointerPressed += (sender, args) =>
            {
                _inputController.HandleMouseAuxiliaryInputs(args);
            };

            // Handle clean UI sizing fallbacks when tabs are selected
            MainTabs.SelectionChanged += (sender, args) =>
            {
                _browserManager.HandleTabSelectionChanged();
            };

            // Process explicit tab termination calls cleanly
            MainTabs.TabCloseRequested += (sender, args) =>
            {
                _browserManager.HandleTabCloseRequested(args, () => this.Close());
            };
        }

        /// <summary>
        /// Exposed command structure accessible from XAML UI Data Bindings to spin up new browser contexts.
        /// </summary>
        public void NewTabCommand()
        {
            _browserManager.CreateNewTabContext("New Workspace", "https://www.google.com");
        }
    }
}
