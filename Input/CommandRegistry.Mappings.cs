using System;
using Windows.System;

namespace TB.Input;

public partial class CommandRegistry
{
    private void RegisterKeyMappings()
    {
        void Map(VirtualKey key, bool ctrl, bool alt, bool shift, BrowserCommand cmd, string name, string shortcutText, string description)
        {
            _keyToCommandMap[(key, ctrl, alt, shift)] = cmd;
            
            int existingIdx = -1;
            for (int i = 0; i < _commands.Count; i++)
            {
                if (_commands[i].Command == cmd)
                {
                    existingIdx = i;
                    break;
                }
            }

            if (existingIdx >= 0)
            {
                var existing = _commands[existingIdx];
                if (!existing.Shortcut.Contains(shortcutText))
                {
                    _commands[existingIdx] = existing with { Shortcut = existing.Shortcut + ", " + shortcutText };
                }
            }
            else
            {
                _commands.Add(new CommandMetadata(cmd, name, shortcutText, description));
            }
        }

        // Tab Management
        Map(VirtualKey.T, true, false, false, BrowserCommand.NewTab, "New Tab", "Ctrl+T", "Open a new browser tab");
        Map(VirtualKey.N, true, false, false, BrowserCommand.NewWindow, "New Window", "Ctrl+N", "Launch a new browser window instance");
        Map(VirtualKey.W, true, false, false, BrowserCommand.CloseTab, "Close Tab", "Ctrl+W", "Close the current active tab");
        Map(VirtualKey.F4, true, false, false, BrowserCommand.CloseTab, "Close Tab", "Ctrl+F4", "Close the current active tab");
        Map(VirtualKey.T, true, false, true, BrowserCommand.ReopenClosedTab, "Reopen Closed Tab", "Ctrl+Shift+T", "Restore the most recently closed tab");
        Map(VirtualKey.Tab, true, false, false, BrowserCommand.NextTab, "Next Tab", "Ctrl+Tab", "Switch to the next tab");
        Map(VirtualKey.Tab, true, false, true, BrowserCommand.PrevTab, "Previous Tab", "Ctrl+Shift+Tab", "Switch to the previous tab");

        // Ctrl+1..8
        for (int i = 1; i <= 8; i++)
        {
            var vk = VirtualKey.Number0 + i;
            var cmd = (BrowserCommand)((int)BrowserCommand.GoToTab1 + (i - 1));
            Map(vk, true, false, false, cmd, $"Go to Tab {i}", $"Ctrl+{i}", $"Switch focus to tab position {i}");
        }
        Map(VirtualKey.Number9, true, false, false, BrowserCommand.GoToTab9, "Go to Last Tab", "Ctrl+9", "Switch focus to the rightmost tab");

        // Move Tab
        Map(VirtualKey.PageUp, true, false, true, BrowserCommand.MoveTabLeft, "Move Tab Left", "Ctrl+Shift+PgUp", "Shift the active tab to the left");
        Map(VirtualKey.PageDown, true, false, true, BrowserCommand.MoveTabRight, "Move Tab Right", "Ctrl+Shift+PgDn", "Shift the active tab to the right");

        // Navigation
        Map(VirtualKey.F5, false, false, false, BrowserCommand.Reload, "Reload", "F5", "Reload the current page");
        Map(VirtualKey.R, true, false, false, BrowserCommand.Reload, "Reload", "Ctrl+R", "Reload the current page");
        Map(VirtualKey.F5, false, false, true, BrowserCommand.HardReload, "Hard Reload", "Shift+F5", "Force reload bypassing cache");
        Map(VirtualKey.R, true, false, true, BrowserCommand.HardReload, "Hard Reload", "Ctrl+Shift+R", "Force reload bypassing cache");
        Map(VirtualKey.Left, false, true, false, BrowserCommand.Back, "Back", "Alt+Left", "Navigate back in history");
        Map(VirtualKey.Right, false, true, false, BrowserCommand.Forward, "Forward", "Alt+Right", "Navigate forward in history");
        Map(VirtualKey.Home, false, true, false, BrowserCommand.Home, "Home", "Alt+Home", "Navigate to the home page");

        // Zoom
        Map((VirtualKey)0xBB, true, false, false, BrowserCommand.ZoomIn, "Zoom In", "Ctrl++", "Increase webpage scale");
        Map(VirtualKey.Add, true, false, false, BrowserCommand.ZoomIn, "Zoom In", "Ctrl+Num+", "Increase webpage scale");
        Map((VirtualKey)0xBD, true, false, false, BrowserCommand.ZoomOut, "Zoom Out", "Ctrl+-", "Decrease webpage scale");
        Map(VirtualKey.Subtract, true, false, false, BrowserCommand.ZoomOut, "Zoom Out", "Ctrl+Num-", "Decrease webpage scale");
        Map(VirtualKey.Number0, true, false, false, BrowserCommand.ResetZoom, "Reset Zoom", "Ctrl+0", "Reset zoom scale to 100%");
        Map(VirtualKey.NumberPad0, true, false, false, BrowserCommand.ResetZoom, "Reset Zoom", "Ctrl+Num0", "Reset zoom scale to 100%");

        // UI Routing keys
        Map(VirtualKey.L, true, false, false, BrowserCommand.FocusAddressBar, "Focus Address Bar", "Ctrl+L", "Highlight Omnibar address input");
        Map(VirtualKey.F6, false, false, false, BrowserCommand.FocusAddressBar, "Focus Address Bar", "F6", "Highlight Omnibar address input");
        Map(VirtualKey.D, false, true, false, BrowserCommand.FocusAddressBar, "Focus Address Bar", "Alt+D", "Highlight Omnibar address input");
        Map(VirtualKey.F4, false, true, false, BrowserCommand.CloseWindow, "Close Window", "Alt+F4", "Exit the application window");
        Map(VirtualKey.W, true, false, true, BrowserCommand.CloseWindow, "Close Window", "Ctrl+Shift+W", "Exit the application window");
        Map(VirtualKey.F11, false, false, false, BrowserCommand.ToggleFullScreen, "Toggle Full Screen", "F11", "Switch window display mode");
        Map(VirtualKey.P, true, false, true, BrowserCommand.ToggleCommandPalette, "Command Palette", "Ctrl+Shift+P", "Open command palette overlay");

        // New Chrome Parity Mappings
        Map(VirtualKey.J, true, false, false, BrowserCommand.OpenDownloads, "Open Downloads", "Ctrl+J", "Open download history tab");
        Map(VirtualKey.H, true, false, false, BrowserCommand.OpenHistory, "Open History", "Ctrl+H", "Open browsing history tab");
        Map(VirtualKey.O, true, false, true, BrowserCommand.OpenBookmarks, "Open Bookmarks", "Ctrl+Shift+O", "Open bookmarks manager tab");
        Map(VirtualKey.B, true, false, true, BrowserCommand.ToggleBookmarksBar, "Toggle Bookmarks Bar", "Ctrl+Shift+B", "Show or hide bookmarks bar");
        Map(VirtualKey.Delete, true, false, true, BrowserCommand.ClearBrowsingData, "Clear Browsing Data", "Ctrl+Shift+Del", "Clear cached history, cookies, and data");
        Map(VirtualKey.P, true, false, false, BrowserCommand.Print, "Print Page", "Ctrl+P", "Print the current web page");
        Map(VirtualKey.U, true, false, false, BrowserCommand.ViewSource, "View Source", "Ctrl+U", "Inspect the current page's source code");
        Map(VirtualKey.F12, false, false, false, BrowserCommand.ToggleDevTools, "Toggle DevTools", "F12", "Open web page developer inspector");
        Map(VirtualKey.I, true, false, true, BrowserCommand.ToggleDevTools, "Toggle DevTools", "Ctrl+Shift+I", "Open web page developer inspector");
        Map(VirtualKey.F, true, false, false, BrowserCommand.OpenFind, "Find in Page", "Ctrl+F", "Find text matching queries on page");
        Map(VirtualKey.F3, false, false, false, BrowserCommand.FindNext, "Find Next", "F3", "Scroll to next matching query item");
        Map(VirtualKey.F3, false, false, true, BrowserCommand.FindPrev, "Find Previous", "Shift+F3", "Scroll to previous matching query item");
        Map(VirtualKey.S, true, false, false, BrowserCommand.SavePage, "Save Page", "Ctrl+S", "Save the current web page to disk");
    }
}
