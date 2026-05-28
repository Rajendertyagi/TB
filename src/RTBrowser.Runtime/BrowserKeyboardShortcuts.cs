using RTBrowser.Core;

using System.Windows;
using System.Windows.Input;

namespace RTBrowser.Runtime
{
    public static class BrowserKeyboardShortcuts
    {
        public static bool Handle(
            Window window,
            KeyEventArgs e,
            BrowserSessionManager sessionManager,
            System.Action newTabAction,
            System.Action closeTabAction,
            System.Action focusOmniboxAction,
            System.Action refreshAction)
        {
            bool ctrl =
                Keyboard.Modifiers.HasFlag(
                    ModifierKeys.Control);

            if (!ctrl)
            {
                return false;
            }

            switch (e.Key)
            {
                case Key.T:

                    newTabAction.Invoke();

                    e.Handled = true;

                    return true;

                case Key.W:

                    if (sessionManager.HasSessions)
                    {
                        closeTabAction.Invoke();
                    }

                    e.Handled = true;

                    return true;

                case Key.L:

                    focusOmniboxAction.Invoke();

                    e.Handled = true;

                    return true;

                case Key.R:

                    refreshAction.Invoke();

                    e.Handled = true;

                    return true;
            }

            return false;
        }
    }
}
