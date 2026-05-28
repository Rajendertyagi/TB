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

            bool shift =
                Keyboard.Modifiers.HasFlag(
                    ModifierKeys.Shift);

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

                case Key.Tab:

                    if (shift)
                    {
                        ActivatePreviousTab(
                            sessionManager);
                    }
                    else
                    {
                        ActivateNextTab(
                            sessionManager);
                    }

                    e.Handled = true;

                    return true;
            }

            return false;
        }

        private static void ActivateNextTab(
            BrowserSessionManager sessionManager)
        {
            if (sessionManager.SessionCount <= 1)
            {
                return;
            }

            int currentIndex =
                sessionManager.IndexOfActiveSession();

            int nextIndex =
                currentIndex + 1;

            if (nextIndex >= sessionManager.SessionCount)
            {
                nextIndex = 0;
            }

            sessionManager.SetActiveSession(
                sessionManager
                    .Sessions[nextIndex]
                    .Id);
        }

        private static void ActivatePreviousTab(
            BrowserSessionManager sessionManager)
        {
            if (sessionManager.SessionCount <= 1)
            {
                return;
            }

            int currentIndex =
                sessionManager.IndexOfActiveSession();

            int previousIndex =
                currentIndex - 1;

            if (previousIndex < 0)
            {
                previousIndex =
                    sessionManager.SessionCount - 1;
            }

            sessionManager.SetActiveSession(
                sessionManager
                    .Sessions[previousIndex]
                    .Id);
        }
    }
}
