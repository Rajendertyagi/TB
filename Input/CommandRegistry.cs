using System;
using System.Collections.Generic;
using Windows.System;
using TB.Helpers;
using TB.Infrastructure;
using TB.Services.Interfaces;

namespace TB.Input;

public class CommandRegistry
{
    private readonly Lazy<ITabManager> _tabManager;
    private readonly Lazy<INavigationService> _navigationService;
    private readonly List<ShortcutBinding> _bindings = new();

    public IReadOnlyList<ShortcutBinding> Bindings => _bindings;

    public CommandRegistry(Lazy<ITabManager> tabManager, Lazy<INavigationService> navigationService)
    {
        _tabManager = tabManager;
        _navigationService = navigationService;
        Build();
    }

    private void Build()
    {
        void Add(VirtualKey key, bool ctrl, bool alt, bool shift, Func<bool> fn) =>
            _bindings.Add(new ShortcutBinding(key, ctrl, alt, shift, fn));

        // Escape: Close Find Bar / Stop
        Add(VirtualKey.Escape, false, false, false, () => { _tabManager.Value.CloseFindBarAsync().FireAndForget(ex => Logger.Warning($"Escape failed: {ex.Message}")); _tabManager.Value.Stop(); return true; });

        // Ctrl+T: New Tab
        Add(VirtualKey.T, true, false, false, () => { _tabManager.Value.CreateTabAsync().FireAndForget(ex => Logger.Warning($"New tab failed: {ex.Message}")); return true; });

        // Ctrl+W: Close Tab
        Add(VirtualKey.W, true, false, false, () => { _tabManager.Value.CloseActiveTab(); return true; });

        // Ctrl+F4: Close Tab (Alt+F4 path handled separately for Close Window)
        Add(VirtualKey.F4, true, false, false, () => { _tabManager.Value.CloseActiveTab(); return true; });
        Add(VirtualKey.F4, false, true, false, () => { _tabManager.Value.CloseActiveTab(); return true; });

        // Ctrl+Shift+W: Close Window — signals MainWindow via event routing (returns false)
        Add(VirtualKey.W, true, false, true, () => false);

        // Ctrl+Tab: Next Tab
        Add(VirtualKey.Tab, true, false, false, () => { _tabManager.Value.NextTab(); return true; });

        // Ctrl+Shift+Tab: Prev Tab
        Add(VirtualKey.Tab, true, false, true, () => { _tabManager.Value.PrevTab(); return true; });

        // Ctrl+L / F6: Focus URL bar — returns false, MainWindow handles routing
        Add(VirtualKey.L, true, false, false, () => false);
        Add(VirtualKey.F6, false, false, false, () => false);

        // Alt+D: Focus URL bar — returns false, MainWindow handles routing
        Add(VirtualKey.D, false, true, false, () => false);

        // F5: Reload
        Add(VirtualKey.F5, false, false, false, () => { _tabManager.Value.Reload(); return true; });

        // Shift+F5: Hard Reload
        Add(VirtualKey.F5, false, false, true, () => { _tabManager.Value.HardReloadAsync().FireAndForget(ex => Logger.Warning($"Hard reload failed: {ex.Message}")); return true; });

        // Ctrl+R: Reload
        Add(VirtualKey.R, true, false, false, () => { _tabManager.Value.Reload(); return true; });

        // Ctrl+Shift+R: Hard Reload
        Add(VirtualKey.R, true, false, true, () => { _tabManager.Value.HardReloadAsync().FireAndForget(ex => Logger.Warning($"Hard reload failed: {ex.Message}")); return true; });

        // Alt+Left: Back
        Add(VirtualKey.Left, false, true, false, () => { _tabManager.Value.Back(); return true; });

        // Alt+Right: Forward
        Add(VirtualKey.Right, false, true, false, () => { _tabManager.Value.Forward(); return true; });

        // Ctrl+P: Print
        Add(VirtualKey.P, true, false, false, () => { _tabManager.Value.PrintAsync().FireAndForget(ex => Logger.Warning($"Print failed: {ex.Message}")); return true; });

        // Ctrl+U: View Source
        Add(VirtualKey.U, true, false, false, () => { _tabManager.Value.ViewSource(); return true; });

        // Ctrl+N: New Window — returns false, MainWindow handles routing
        Add(VirtualKey.N, true, false, false, () => false);

        // Ctrl+= (VK_OEM_PLUS = 0xBB) / Ctrl+Numpad+: Zoom In
        Add((VirtualKey)0xBB, true, false, false, () => { _tabManager.Value.ZoomInAsync().FireAndForget(ex => Logger.Warning($"Zoom in failed: {ex.Message}")); return true; });
        Add(VirtualKey.Add, true, false, false, () => { _tabManager.Value.ZoomInAsync().FireAndForget(ex => Logger.Warning($"Zoom in failed: {ex.Message}")); return true; });

        // Ctrl+- (VK_OEM_MINUS = 0xBD) / Ctrl+Numpad-: Zoom Out
        Add((VirtualKey)0xBD, true, false, false, () => { _tabManager.Value.ZoomOutAsync().FireAndForget(ex => Logger.Warning($"Zoom out failed: {ex.Message}")); return true; });
        Add(VirtualKey.Subtract, true, false, false, () => { _tabManager.Value.ZoomOutAsync().FireAndForget(ex => Logger.Warning($"Zoom out failed: {ex.Message}")); return true; });

        // Ctrl+0: Reset Zoom
        Add(VirtualKey.Number0, true, false, false, () => { _tabManager.Value.ResetZoomAsync().FireAndForget(ex => Logger.Warning($"Zoom reset failed: {ex.Message}")); return true; });
        Add(VirtualKey.NumberPad0, true, false, false, () => { _tabManager.Value.ResetZoomAsync().FireAndForget(ex => Logger.Warning($"Zoom reset failed: {ex.Message}")); return true; });

        // Ctrl+Shift+T: Reopen Last Closed Tab
        Add(VirtualKey.T, true, false, true, () => { _tabManager.Value.ReopenLastClosedTabAsync().FireAndForget(ex => Logger.Warning($"Reopen tab failed: {ex.Message}")); return true; });

        // Ctrl+1-8: Jump to Tab
        for (int i = 1; i <= 8; i++)
        {
            var vk = VirtualKey.Number0 + i;
            var idx = i;
            Add(vk, true, false, false, () => { _tabManager.Value.GoToTab(idx); return true; });
        }

        // Ctrl+9: Last Tab
        Add(VirtualKey.Number9, true, false, false, () => { _tabManager.Value.GoToLastTab(); return true; });

        // Ctrl+Shift+PageUp: Move Tab Left
        Add(VirtualKey.PageUp, true, false, true, () => { _tabManager.Value.MoveTabLeft(); return true; });

        // Ctrl+Shift+PageDown: Move Tab Right
        Add(VirtualKey.PageDown, true, false, true, () => { _tabManager.Value.MoveTabRight(); return true; });

        // Alt+I: Feedback
        Add(VirtualKey.I, false, true, false, () => { _tabManager.Value.OpenFeedbackWindowAsync().FireAndForget(ex => Logger.Warning($"Feedback failed: {ex.Message}")); return true; });

        // Ctrl+Shift+B: Toggle Bookmarks Bar
        Add(VirtualKey.B, true, false, true, () => { _tabManager.Value.ToggleBookmarksBar(); return true; });

        // Ctrl+Shift+O: Bookmarks Manager
        Add(VirtualKey.O, true, false, true, () => { _tabManager.Value.OpenBookmarksManagerAsync().FireAndForget(ex => Logger.Warning($"Bookmarks failed: {ex.Message}")); return true; });

        // Alt+Home: Home page
        Add(VirtualKey.Home, false, true, false, () => { _navigationService.Value.Navigate(Defaults.HomeUrl); return true; });

        // F12 / Ctrl+Shift+J: DevTools
        Add(VirtualKey.F12, false, false, false, () => { _tabManager.Value.OpenDeveloperToolsAsync().FireAndForget(ex => Logger.Warning($"DevTools failed: {ex.Message}")); return true; });
        Add(VirtualKey.J, true, false, true, () => { _tabManager.Value.OpenDeveloperToolsAsync().FireAndForget(ex => Logger.Warning($"DevTools failed: {ex.Message}")); return true; });

        // Ctrl+H: History
        Add(VirtualKey.H, true, false, false, () => { _tabManager.Value.OpenHistoryPageAsync().FireAndForget(ex => Logger.Warning($"History failed: {ex.Message}")); return true; });

        // Ctrl+J: Downloads
        Add(VirtualKey.J, true, false, false, () => { _tabManager.Value.OpenDownloadsPageAsync().FireAndForget(ex => Logger.Warning($"Downloads failed: {ex.Message}")); return true; });

        // Ctrl+F: Find Bar
        Add(VirtualKey.F, true, false, false, () => { _tabManager.Value.OpenFindBarAsync().FireAndForget(ex => Logger.Warning($"Find failed: {ex.Message}")); return true; });

        // F3: Find Next
        Add(VirtualKey.F3, false, false, false, () => { _tabManager.Value.FindNextAsync().FireAndForget(ex => Logger.Warning($"Find next failed: {ex.Message}")); return true; });

        // Ctrl+G: Find Next
        Add(VirtualKey.G, true, false, false, () => { _tabManager.Value.FindNextAsync().FireAndForget(ex => Logger.Warning($"Find next failed: {ex.Message}")); return true; });

        // Shift+F3: Find Previous
        Add(VirtualKey.F3, false, false, true, () => { _tabManager.Value.FindPreviousAsync().FireAndForget(ex => Logger.Warning($"Find previous failed: {ex.Message}")); return true; });

        // Ctrl+Shift+G: Find Previous
        Add(VirtualKey.G, true, false, true, () => { _tabManager.Value.FindPreviousAsync().FireAndForget(ex => Logger.Warning($"Find previous failed: {ex.Message}")); return true; });

        // Ctrl+Shift+Delete: Clear Browsing Data
        Add(VirtualKey.Delete, true, false, true, () => { _tabManager.Value.OpenClearBrowsingDataDialogAsync().FireAndForget(ex => Logger.Warning($"Clear data failed: {ex.Message}")); return true; });

        // F11: Full Screen — returns false, MainWindow handles routing
        Add(VirtualKey.F11, false, false, false, () => false);

        // F10: Chrome Menu
        Add(VirtualKey.F10, false, false, false, () => { _tabManager.Value.OpenChromeMenuAsync().FireAndForget(ex => Logger.Warning($"Chrome menu failed: {ex.Message}")); return true; });
    }

    public ShortcutBinding? Match(VirtualKey key, bool ctrl, bool alt, bool shift)
    {
        foreach (var b in _bindings)
        {
            if (b.Key == key && b.Ctrl == ctrl && b.Alt == alt && b.Shift == shift)
                return b;
        }
        return null;
    }
}
