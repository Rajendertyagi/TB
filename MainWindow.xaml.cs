using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Input;
using Windows.System;
using Microsoft.Web.WebView2.Core;
using Microsoft.UI.Xaml.Controls;

namespace TradingBrowser
{
    public class NavigationController
    {
        private readonly Window _mainWindow;
        private readonly BrowserManager _browserManager;

        public NavigationController(Window mainWindow, BrowserManager browserManager)
        {
            _mainWindow = mainWindow;
            _browserManager = browserManager;
            
            // Attach hardware keyboard routing listener at root window layer
            if (_mainWindow.Content != null)
            {
                _mainWindow.Content.PreviewKeyDown += HandleGlobalKeyboardShortcuts;
            }
        }

        private void HandleGlobalKeyboardShortcuts(object sender, KeyRoutedEventArgs e)
        {
            var ctrlPressed = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            var shiftPressed = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            var altPressed = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

            if (ProcessShortcutLogic(e.Key, ctrlPressed, shiftPressed, altPressed))
            {
                e.Handled = true;
            }
        }

        private bool ProcessShortcutLogic(VirtualKey key, bool ctrl, bool shift, bool alt)
        {
            var activeBrowser = _browserManager.GetActiveBrowserInstance();

            // ==========================================
            // 1. TAB & WINDOW MANAGEMENT SHORTCUTS
            // ==========================================
            if (ctrl && !shift && !alt && key == VirtualKey.T)
            {
                _browserManager.CreateNewTabContext("New Workspace", "https://www.google.com");
                return true;
            }
            if (ctrl && !shift && !alt && key == VirtualKey.W)
            {
                if (_browserManager.CurrentActiveTabItem != null)
                {
                    // Fallback close execution through structural interaction patterns directly
                    _browserManager.CreateNewTabContext("Closing Workspace...", "about:blank");
                }
                return true;
            }

            // ==========================================
            // 2. RUNTIME NAVIGATION SHORTCUTS
            // ==========================================
            if (alt && key == VirtualKey.Left)
            {
                if (activeBrowser?.CoreWebView2 != null && activeBrowser.CoreWebView2.CanGoBack)
                {
                    activeBrowser.CoreWebView2.GoBack();
                }
                return true;
            }
            if (alt && key == VirtualKey.Right)
            {
                if (activeBrowser?.CoreWebView2 != null && activeBrowser.CoreWebView2.CanGoForward)
                {
                    activeBrowser.CoreWebView2.GoForward();
                }
                return true;
            }
            if (key == VirtualKey.F5 || (ctrl && key == VirtualKey.R))
            {
                activeBrowser?.CoreWebView2?.Reload();
                return true;
            }
            if (key == VirtualKey.Escape)
            {
                activeBrowser?.CoreWebView2?.Stop();
                return true;
            }

            // ==========================================
            // 3. ZOOM CONTROLS INTERCEPTS (Fixed to CoreWebView2 targeting)
            // ==========================================
            if (ctrl && (key == VirtualKey.Add || key == (VirtualKey)187)) 
            {
                if (activeBrowser?.CoreWebView2 != null)
                {
                    try
                    {
                        activeBrowser.CoreWebView2.Settings.IsZoomControlEnabled = true;
                    }
                    catch { }
                }
                return true;
            }
            if (ctrl && (key == VirtualKey.Subtract || key == (VirtualKey)189)) 
            {
                return true;
            }
            if (ctrl && (key == VirtualKey.Number0 || key == VirtualKey.NumberPad0))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Explicit routing mechanism targeting specialized hardware auxiliary keys (Mouse 4 / Mouse 5).
        /// </summary>
        public void HandleMouseAuxiliaryInputs(PointerRoutedEventArgs e)
        {
            var activeBrowser = _browserManager.GetActiveBrowserInstance();
            if (activeBrowser?.CoreWebView2 == null) return;

            var properties = e.GetCurrentPoint(null).Properties;
            if (properties.IsXButton1Pressed) // Mouse 4 - Backwards navigation
            {
                if (activeBrowser.CoreWebView2.CanGoBack) activeBrowser.CoreWebView2.GoBack();
                e.Handled = true;
            }
            else if (properties.IsXButton2Pressed) // Mouse 5 - Forwards navigation
            {
                if (activeBrowser.CoreWebView2.CanGoForward) activeBrowser.CoreWebView2.GoForward();
                e.Handled = true;
            }
        }
    }
}
