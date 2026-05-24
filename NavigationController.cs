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
        private readonly BrowserViewModel _viewModel;

        public NavigationController(BrowserViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>
        /// Global Keyboard Hook: Processes application-wide hotkeys.
        /// Attach this to your MainWindow's PreviewKeyDown event.
        /// </summary>
        public void HandleGlobalKeyboardShortcuts(object sender, KeyRoutedEventArgs e)
        {
            var ctrl = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            var shift = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            var alt = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

            if (ProcessShortcutLogic(e.Key, ctrl, shift, alt))
            {
                e.Handled = true;
            }
        }

        private bool ProcessShortcutLogic(VirtualKey key, bool ctrl, bool shift, bool alt)
        {
            // 1. Tab Management
            if (ctrl && !shift && !alt && key == VirtualKey.T)
            {
                _viewModel.OpenNewTabCommand.Execute(null);
                return true;
            }

            // 2. Navigation
            if (alt && key == VirtualKey.Left)
            {
                _viewModel.ActiveTab?.BrowserInstance?.CoreWebView2?.GoBack();
                return true;
            }
            if (alt && key == VirtualKey.Right)
            {
                _viewModel.ActiveTab?.BrowserInstance?.CoreWebView2?.GoForward();
                return true;
            }
            if (key == VirtualKey.F5 || (ctrl && key == VirtualKey.R))
            {
                _viewModel.ActiveTab?.BrowserInstance?.CoreWebView2?.Reload();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Hardware Auxiliary Input: Processes Mouse 4/5 (Back/Forward).
        /// Attach to PointerPressed event.
        /// </summary>
        public void HandleMouseAuxiliaryInputs(PointerRoutedEventArgs e)
        {
            var props = e.GetCurrentPoint(null).Properties;
            if (props.IsXButton1Pressed) // Mouse 4: Back
            {
                _viewModel.ActiveTab?.BrowserInstance?.CoreWebView2?.GoBack();
                e.Handled = true;
            }
            else if (props.IsXButton2Pressed) // Mouse 5: Forward
            {
                _viewModel.ActiveTab?.BrowserInstance?.CoreWebView2?.GoForward();
                e.Handled = true;
            }
        }
    }
}
