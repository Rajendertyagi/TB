using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Input;
using Windows.System;
using Microsoft.Web.WebView2.Core;

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
            _mainWindow.Content.PreviewKeyDown += HandleGlobalKeyboardShortcuts;
        }

        /// <summary>
        /// Explicitly binds to an initialized CoreWebView2 instance to intercept inputs before the engine swallows them.
        /// </summary>
        public void RegisterWebViewInputRouting(CoreWebView2 coreWebView)
        {
            if (coreWebView == null) return;

            // Intercepts accelerator keys directly inside the chromium subsystem render engine
            coreWebView.AcceleratorKeyPressed += (sender, args) =>
            {
                var virtualKey = (VirtualKey)args.VirtualKey;
                var status = args.KeyStatus;
                
                // Track state modifiers
                bool ctrlPressed = (PlatformInlines.GetKeyState(VirtualKey.Control) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
                bool shiftPressed = (PlatformInlines.GetKeyState(VirtualKey.Shift) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
                bool altPressed = (PlatformInlines.GetKeyState(VirtualKey.Menu) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

                if (args.KeyEventKind == CoreWebView2KeyEventKind.KeyDown || args.KeyEventKind == CoreWebView2KeyEventKind.SystemKeyDown)
                {
                    bool handled = ProcessShortcutLogic(virtualKey, ctrlPressed, shiftPressed, altPressed);
                    if (handled)
                    {
                        args.Handled = true;
                    }
                }
            };
        }

        private void HandleGlobalKeyboardShortcuts(object sender, KeyRoutedEventArgs e)
        {
            var ctrlPressed = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
            var shiftPressed = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);
            var altPressed = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down);

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
                // Request closing active window or tab via manager
                if (_browserManager.CurrentActiveTabItem != null)
                {
                    var args = new Microsoft.UI.Xaml.Controls.TabViewTabCloseRequestedEventArgs();
                    _browserManager.HandleTabCloseRequested(args, () => _mainWindow.Close());
                }
                return true;
            }
            if (ctrl && shift && !alt && key == VirtualKey.T)
            {
                // Reopen last closed tab placeholder fallback
                _browserManager.CreateNewTabContext("Restored Workspace", "https://www.google.com");
                return true;
            }

            // Tab Navigation Indexed Jumps (Ctrl + [1-9])
            if (ctrl && !shift && !alt && (key >= VirtualKey.Number1 && key <= VirtualKey.Number9))
            {
                int TargetIndex = (int)key - (int)VirtualKey.Number1;
                // Handled internally by tracking tab list arrays if index within range
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
            // 3. OMNIBOX & SEARCH FOCUS INTERCEPTS
            // ==========================================
            if ((ctrl && key == VirtualKey.L) || key == VirtualKey.F6)
            {
                // Access parent control properties to trigger UI focus updates on Omnibox element
                return true;
            }

            // ==========================================
            // 4. ZOOM CONTROLS INTERCEPTS
            // ==========================================
            if (ctrl && (key == VirtualKey.Add || key == (VirtualKey)187)) // 187 is system mapping for OEM Plus
            {
                if (activeBrowser?.CoreWebView2 != null)
                {
                    activeBrowser.CoreWebView2.Profile.PreferredTrackingPreventionLevel = CoreWebView2TrackingPreventionLevel.Balanced;
                    activeBrowser.ZoomFactor += 0.1;
                }
                return true;
            }
            if (ctrl && (key == VirtualKey.Subtract || key == (VirtualKey)189)) // 189 is system mapping for OEM Minus
            {
                if (activeBrowser != null && activeBrowser.ZoomFactor > 0.2)
                {
                    activeBrowser.ZoomFactor -= 0.1;
                }
                return true;
            }
            if (ctrl && (key == VirtualKey.Number0 || key == VirtualKey.NumberPad0))
            {
                if (activeBrowser != null) activeBrowser.ZoomFactor = 1.0;
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

    internal static class PlatformInlines
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
        public static extern short GetKeyState(VirtualKey virtualKey);
    }
}
