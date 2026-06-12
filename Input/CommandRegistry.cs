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

    // OPTIMIZATION: Use a Dictionary for O(1) instant lookups instead of a List foreach loop
    private readonly Dictionary<(VirtualKey Key, bool Ctrl, bool Alt, bool Shift), ShortcutBinding> _bindingMap = [];

    // Keep the list for UI binding displays (e.g., if you build a Settings -> Shortcuts page later)
    private readonly List<ShortcutBinding> _bindings = [];

    public IReadOnlyList<ShortcutBinding> Bindings => _bindings;

    public CommandRegistry(Lazy<ITabManager> tabManager, Lazy<INavigationService> navigationService)
    {
        _tabManager = tabManager ?? throw new ArgumentNullException(nameof(tabManager));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        Build();
    }

    private void Build()
    {
        void Add(VirtualKey key, bool ctrl, bool alt, bool shift, Func<bool> fn)
        {
            var binding = new ShortcutBinding(key, ctrl, alt, shift, fn);
            _bindings.Add(binding);
            _bindingMap[(key, ctrl, alt, shift)] = binding;
        }

        // ==========================================
        // TAB MANAGEMENT
        // ==========================================

        // Ctrl+T: New Tab
        Add(VirtualKey.T, true, false, false, () => { _tabManager.Value.CreateTabAsync().FireAndForget(ex => Logger.Warning($"New tab failed: {ex.Message}")); return true; });

        // Ctrl+W / Ctrl+F4: Close Tab
        Add(VirtualKey.W, true, false, false, () => { _tabManager.Value.CloseActiveTab(); return true; });
        Add(VirtualKey.F4, true, false, false, () => { _tabManager.Value.CloseActiveTab(); return true; });

        // FIX: Alt+F4 / Ctrl+Shift+W: Close Window (Returns false to let MainWindow handle OS-level closing)
        Add(VirtualKey.F4, false, true, false, () => false);
        Add(VirtualKey.W, true, false, true, () => false);

        // Ctrl+Shift+T: Reopen Last Closed Tab
        Add(VirtualKey.T, true, false, true, () => { _tabManager.Value.ReopenLastClosedTabAsync().FireAndForget(ex => Logger.Warning($"Reopen tab failed: {ex.Message}")); return true; });

        // Ctrl+Tab / Ctrl+Shift+Tab: Next/Prev Tab
        Add(VirtualKey.Tab, true, false, false, () => { _tabManager.Value.NextTab(); return true; });
        Add(VirtualKey.Tab, true, false, true, () => { _tabManager.Value.PrevTab(); return true; });

        // Ctrl+1-8: Jump to Tab
        for (int i = 1; i <= 8; i++)
        {
            var vk = VirtualKey.Number0 + i;
            var idx = i;
            Add(vk, true, false, false, () => { _tabManager.Value.GoToTab(idx); return true; });
        }

        // Ctrl+9: Last Tab
        Add(VirtualKey.Number9, true, false, false, () => { _tabManager.Value.GoToLastTab(); return true; });

        // Ctrl+Shift+PageUp/Down: Move Tab Left/Right
        Add(VirtualKey.PageUp, true, false, true, () => { _tabManager.Value.MoveTabLeft(); return true; });
        Add(VirtualKey.PageDown, true, false, true, () => { _tabManager.Value.MoveTabRight(); return true; });

        // ==========================================
        // NAVIGATION & ZOOM
        // ==========================================

        // F5 / Ctrl+R: Reload
        Add(VirtualKey.F5, false, false, false, () => { _tabManager.Value.Reload(); return true; });
        Add(VirtualKey.R, true, false, false, () => { _tabManager.Value.Reload(); return true; });

        // Shift+F5 / Ctrl+Shift+R: Hard Reload
        Add(VirtualKey.F5, false, false, true, () => { _tabManager.Value.HardReloadAsync().FireAndForget(ex => Logger.Warning($"Hard reload failed: {ex.Message}")); return true; });
        Add(VirtualKey.R, true, false, true, () => { _tabManager.Value.HardReloadAsync().FireAndForget(ex => Logger.Warning($"Hard reload failed: {ex.Message}")); return true; });

        // Alt+Left/Right: Back/Forward
        Add(VirtualKey.Left, false, true, false, () => { _tabManager.Value.Back(); return true; });
        Add(VirtualKey.Right, false, true, false, () => { _tabManager.Value.Forward(); return true; });

        // Alt+Home: Home page
        Add(VirtualKey.Home, false, true, false, () => { _navigationService.Value.Navigate(Defaults.HomeUrl); return true; });

        // Escape: Close Find Bar / Stop
        Add(VirtualKey.Escape, false, false, false, () => { _tabManager.Value.CloseFindBarAsync().FireAndForget(ex => Logger.Warning($"Escape failed: {ex.Message}")); _tabManager.Value.Stop(); return true; });

        // Zoom In: Ctrl+= (0xBB) / Ctrl+Numpad+
        Add((VirtualKey)0xBB, true, false, false, () => { _tabManager.Value.ZoomInAsync().FireAndForget(ex => Logger.Warning($"Zoom in failed: {ex.Message}")); return true; });
        Add(VirtualKey.Add, true, false, false, () => { _tabManager.Value.ZoomInAsync().FireAndForget(ex => Logger.Warning($"Zoom in failed: {ex.Message}")); return true; });

        // Zoom Out: Ctrl+- (0xBD) / Ctrl+Numpad-
        Add((VirtualKey)0xBD, true, false, false, () => { _tabManager.Value.ZoomOutAsync().FireAndForget(ex => Logger.Warning($"Zoom out failed: {ex.Message}")); return true; });
        Add(VirtualKey.Subtract, true, false, false, () => { _tabManager.Value.ZoomOutAsync().FireAndForget(ex => Logger.Warning($"Zoom out failed: {ex.Message}")); return true; });

        // Reset Zoom: Ctrl+0
        Add(VirtualKey.Number0, true, false, false, () => { _tabManager.Value.ResetZoomAsync().FireAndForget(ex => Logger.Warning($"Zoom reset failed: {ex.Message}")); return true; });
        Add(VirtualKey.NumberPad0, true, false, false, () => { _tabManager.Value.ResetZoomAsync().FireAndForget(ex => Logger.Warning($"Zoom reset failed: {ex.Message}")); return true; });

        // ==========================================
        // UI ROUTING (Returns false to trigger MainWindow Events)
        // ==========================================

        // Ctrl+L / F6 / Alt+D: Focus URL bar
        Add(VirtualKey.L, true, false, false, () => false);
        Add(VirtualKey.F6, false, false, false, () => false);
        Add(VirtualKey.D, false, true, false, () => false);

        // Ctrl+N: New Window
        Add(VirtualKey.N, true, false, false, () => false);

        // F11: Full Screen
        Add(VirtualKey.F11, false, false, false, () => false);

        // ==========================================
        // TOOLS & INTERNAL PAGES
        // ==========================================

        // Ctrl+F: Find Bar
        Add(VirtualKey.F, true, false, false, () => { _tabManager.Value.OpenFindBarAsync().FireAndForget(ex => Logger.Warning($"Find failed: {ex.Message}")); return true; });

        // F3 / Ctrl+G: Find Next
        Add(VirtualKey.F3, false, false, false, () => { _tabManager.Value.FindNextAsync().FireAndForget(ex => Logger.Warning($"Find next failed: {ex.Message}")); return true; });
        Add(VirtualKey.G, true, false, false, () => { _tabManager.Value.FindNextAsync().FireAndForget(ex => Logger.Warning($"Find next failed: {ex.Message}")); return true; });

        // Shift+F3 / Ctrl+Shift+G: Find Previous
        Add(VirtualKey.F3, false, false, true, () => { _tabManager.Value.FindPreviousAsync().FireAndForget(ex => Logger.Warning($"Find previous failed: {ex.Message}")); return true; });
        Add(VirtualKey.G, true, false, true, () => { _tabManager.Value.FindPreviousAsync().FireAndForget(ex => Logger.Warning($"Find previous failed: {ex.Message}")); return true; });

        // Ctrl+H: History
        Add(VirtualKey.H, true, false, false, () => { _tabManager.Value.OpenHistoryPageAsync().FireAndForget(ex => Logger.Warning($"History failed: {ex.Message}")); return true; });

        // Ctrl+J: Downloads
        Add(VirtualKey.J, true, false, false, () => { _tabManager.Value.OpenDownloadsPageAsync().FireAndForget(ex => Logger.Warning($"Downloads failed: {ex.Message}")); return true; });

        // Ctrl+Shift+O: Bookmarks Manager
        Add(VirtualKey.O, true, false, true, () => { _tabManager.Value.OpenBookmarksManagerAsync().FireAndForget(ex => Logger.Warning($"Bookmarks failed: {ex.Message}")); return true; });

        // Ctrl+Shift+B: Toggle Bookmarks Bar
        Add(VirtualKey.B, true, false, true, () => { _tabManager.Value.ToggleBookmarksBar(); return true; });

        // Ctrl+Shift+Delete: Clear Browsing Data
        Add(VirtualKey.Delete, true, false, true, () => { _tabManager.Value.OpenClearBrowsingDataDialogAsync().FireAndForget(ex => Logger.Warning($"Clear data failed: {ex.Message}")); return true; });

        // Ctrl+P: Print
        Add(VirtualKey.P, true, false, false, () => { _tabManager.Value.PrintAsync().FireAndForget(ex => Logger.Warning($"Print failed: {ex.Message}")); return true; });

        // Ctrl+U: View Source
        Add(VirtualKey.U, true, false, false, () => { _tabManager.Value.ViewSource(); return true; });

        // F12 / Ctrl+Shift+J: DevTools
        Add(VirtualKey.F12, false, false, false, () => { _tabManager.Value.OpenDeveloperToolsAsync().FireAndForget(ex => Logger.Warning($"DevTools failed: {ex.Message}")); return true; });
        Add(VirtualKey.J, true, false, true, () => { _tabManager.Value.OpenDeveloperToolsAsync().FireAndForget(ex => Logger.Warning($"DevTools failed: {ex.Message}")); return true; });

        // Alt+I: Feedback
        Add(VirtualKey.I, false, true, false, () => { _tabManager.Value.OpenFeedbackWindowAsync().FireAndForget(ex => Logger.Warning($"Feedback failed: {ex.Message}")); return true; });

        // F10: Chrome Menu
        Add(VirtualKey.F10, false, false, false, () => { _tabManager.Value.OpenChromeMenuAsync().FireAndForget(ex => Logger.Warning($"Chrome menu failed: {ex.Message}")); return true; });
    }

    // OPTIMIZATION: O(1) Dictionary Lookup
    public ShortcutBinding? Match(VirtualKey key, bool ctrl, bool alt, bool shift)
    {
        if (_bindingMap.TryGetValue((key, ctrl, alt, shift), out var binding))
            return binding;

        return null;
    }
}
