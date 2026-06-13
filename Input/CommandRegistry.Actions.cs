using System;
using TB.Helpers;
using TB.Infrastructure;

namespace TB.Input;

public partial class CommandRegistry
{
    private void RegisterActions()
    {
        // Tab Management
        _commandToActionMap[BrowserCommand.NewTab] = () => { _tabManager.Value.CreateTabAsync().FireAndForget(ex => Logger.Warning($"New tab failed: {ex.Message}")); return true; };
        _commandToActionMap[BrowserCommand.NewWindow] = () =>
        {
            var exePath = Environment.ProcessPath;
            if (exePath != null)
                System.Diagnostics.Process.Start(exePath);
            return true;
        };
        _commandToActionMap[BrowserCommand.CloseTab] = () => { _tabManager.Value.CloseActiveTab(); return true; };
        _commandToActionMap[BrowserCommand.ReopenClosedTab] = () => { _tabManager.Value.ReopenLastClosedTabAsync().FireAndForget(ex => Logger.Warning($"Reopen tab failed: {ex.Message}")); return true; };
        _commandToActionMap[BrowserCommand.NextTab] = () => { _tabManager.Value.NextTab(); return true; };
        _commandToActionMap[BrowserCommand.PrevTab] = () => { _tabManager.Value.PrevTab(); return true; };

        // GoToTab 1-8
        for (int i = 1; i <= 8; i++)
        {
            var idx = i;
            var cmd = (BrowserCommand)((int)BrowserCommand.GoToTab1 + (idx - 1));
            _commandToActionMap[cmd] = () => { _tabManager.Value.GoToTab(idx); return true; };
        }
        _commandToActionMap[BrowserCommand.GoToTab9] = () => { _tabManager.Value.GoToLastTab(); return true; };

        // Move Tab Left/Right
        _commandToActionMap[BrowserCommand.MoveTabLeft] = () => { _tabManager.Value.MoveTabLeft(); return true; };
        _commandToActionMap[BrowserCommand.MoveTabRight] = () => { _tabManager.Value.MoveTabRight(); return true; };

        // Navigation
        _commandToActionMap[BrowserCommand.Reload] = () => { _tabManager.Value.Reload(); return true; };
        _commandToActionMap[BrowserCommand.HardReload] = () => { _tabManager.Value.HardReloadAsync().FireAndForget(ex => Logger.Warning($"Hard reload failed: {ex.Message}")); return true; };
        _commandToActionMap[BrowserCommand.Back] = () => { _tabManager.Value.Back(); return true; };
        _commandToActionMap[BrowserCommand.Forward] = () => { _tabManager.Value.Forward(); return true; };
        _commandToActionMap[BrowserCommand.Home] = () => { _navigationService.Value.Navigate(Defaults.HomeUrl); return true; };

        // Zoom
        _commandToActionMap[BrowserCommand.ZoomIn] = () => { _tabManager.Value.ZoomInAsync().FireAndForget(ex => Logger.Warning($"Zoom in failed: {ex.Message}")); return true; };
        _commandToActionMap[BrowserCommand.ZoomOut] = () => { _tabManager.Value.ZoomOutAsync().FireAndForget(ex => Logger.Warning($"Zoom out failed: {ex.Message}")); return true; };
        _commandToActionMap[BrowserCommand.ResetZoom] = () => { _tabManager.Value.ResetZoomAsync().FireAndForget(ex => Logger.Warning($"Zoom reset failed: {ex.Message}")); return true; };

        // UI Routing events raise registry events and return true to consume
        _commandToActionMap[BrowserCommand.FocusAddressBar] = () => { FocusAddressBarRequested?.Invoke(); return true; };
        _commandToActionMap[BrowserCommand.CloseWindow] = () => { CloseWindowRequested?.Invoke(); return true; };
        _commandToActionMap[BrowserCommand.ToggleFullScreen] = () => { ToggleFullScreenRequested?.Invoke(); return true; };
        _commandToActionMap[BrowserCommand.ToggleCommandPalette] = () => { ToggleCommandPaletteRequested?.Invoke(); return true; };

        // New Chrome Parity Actions
        _commandToActionMap[BrowserCommand.OpenDownloads] = () => { _tabManager.Value.OpenDownloadsPageAsync().FireAndForget(ex => Logger.Warning($"Open downloads failed: {ex.Message}")); return true; };
        _commandToActionMap[BrowserCommand.OpenHistory] = () => { _tabManager.Value.OpenHistoryPageAsync().FireAndForget(ex => Logger.Warning($"Open history failed: {ex.Message}")); return true; };
        _commandToActionMap[BrowserCommand.OpenBookmarks] = () => { _tabManager.Value.OpenBookmarksManagerAsync().FireAndForget(ex => Logger.Warning($"Open bookmarks failed: {ex.Message}")); return true; };
        _commandToActionMap[BrowserCommand.ToggleBookmarksBar] = () => { _tabManager.Value.ToggleBookmarksBar(); return true; };
        _commandToActionMap[BrowserCommand.ClearBrowsingData] = () => { _tabManager.Value.OpenClearBrowsingDataDialogAsync().FireAndForget(ex => Logger.Warning($"Open clear data failed: {ex.Message}")); return true; };
        _commandToActionMap[BrowserCommand.Print] = () => { _tabManager.Value.PrintAsync().FireAndForget(ex => Logger.Warning($"Print failed: {ex.Message}")); return true; };
        _commandToActionMap[BrowserCommand.ViewSource] = () => { _tabManager.Value.ViewSource(); return true; };
        _commandToActionMap[BrowserCommand.ToggleDevTools] = () => { _tabManager.Value.OpenDeveloperToolsAsync().FireAndForget(ex => Logger.Warning($"Toggle devtools failed: {ex.Message}")); return true; };
        _commandToActionMap[BrowserCommand.OpenFind] = () => { _tabManager.Value.OpenFindBarAsync().FireAndForget(ex => Logger.Warning($"Open find bar failed: {ex.Message}")); return true; };
        _commandToActionMap[BrowserCommand.FindNext] = () => { _tabManager.Value.FindNextAsync().FireAndForget(ex => Logger.Warning($"Find next failed: {ex.Message}")); return true; };
        _commandToActionMap[BrowserCommand.FindPrev] = () => { _tabManager.Value.FindPreviousAsync().FireAndForget(ex => Logger.Warning($"Find prev failed: {ex.Message}")); return true; };
        _commandToActionMap[BrowserCommand.SavePage] = () => { _tabManager.Value.SavePageAsync().FireAndForget(ex => Logger.Warning($"Save page failed: {ex.Message}")); return true; };
    }
}
