using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TB.Helpers;
using TB.Infrastructure;
using TB.Models;

namespace TB.Services;

public partial class TabManager
{
    internal void ScheduleSaveSession()
    {
        lock (_saveLock)
        {
            _saveCts?.Cancel();
            _saveCts = new CancellationTokenSource();
            var token = _saveCts.Token;
            _ = Task.Delay(500, token).ContinueWith(async t => { if (!t.IsCanceled) await SaveSessionInternalAsync(); }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

    private async Task SaveSessionInternalAsync()
    {
        if (_isRestoring) return;
        await _sessionSaveLock.WaitAsync();
        try
        {
            var state = new SessionState { ActiveTabIndex = _activeId == -1 ? -1 : _tabs.FindIndex(t => t.Id == _activeId), Tabs = _tabs.Select(t => new TabEntry { Url = t.Url, Title = t.Title }).ToList() };
            var dir = Path.GetDirectoryName(_sessionPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            await File.WriteAllTextAsync(_sessionPath, JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex) { Logger.Error($"Failed to save session: {ex.Message}"); }
        finally { if (!_disposed) _sessionSaveLock.Release(); }
    }

    public async Task<bool> TryLoadSessionAsync()
    {
        try
        {
            if (!File.Exists(_sessionPath)) return false;
            var json = await File.ReadAllTextAsync(_sessionPath);
            if (string.IsNullOrWhiteSpace(json)) return false;
            var state = JsonSerializer.Deserialize<SessionState>(json);
            if (state?.Tabs == null || state.Tabs.Count == 0) return false;

            _isRestoring = true;
            int activeIdx = state.ActiveTabIndex >= 0 && state.ActiveTabIndex < state.Tabs.Count ? state.ActiveTabIndex : 0;

            for (int i = 0; i < state.Tabs.Count; i++)
            {
                var url = string.IsNullOrEmpty(state.Tabs[i].Url) ? Defaults.HomeUrl : state.Tabs[i].Url;
                await CreateTabInternalAsync(url, deferNavigation: i != activeIdx);
            }

            _isRestoring = false;
            ScheduleSaveSession();
            if (state.ActiveTabIndex >= 0 && state.ActiveTabIndex < _tabs.Count) SwitchTab(_tabs[state.ActiveTabIndex].Id);
            return true;
        }
        catch (Exception ex) { Logger.Error($"Failed to load session: {ex.Message}"); _isRestoring = false; return false; }
    }

    [Conditional("DEBUG")]
    private void AssertConsistent()
    {
        var tabIds = new System.Collections.Generic.HashSet<int>(_tabs.Select(t => t.Id));
        var viewIds = new System.Collections.Generic.HashSet<int>(_webViews.Keys);
        var zoomIds = new System.Collections.Generic.HashSet<int>(_zoomLevels.Keys);
        Debug.Assert(tabIds.SetEquals(viewIds), $"Tab IDs and WebView IDs out of sync");
        Debug.Assert(tabIds.SetEquals(zoomIds), $"Tab IDs and zoom levels out of sync");
        Debug.Assert(_activeId == -1 || _webViews.ContainsKey(_activeId), $"Active tab {_activeId} has no WebView");
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        Logger.Info($"Shutting down: saving session ({_tabs.Count} tabs)");
        await SaveSessionInternalAsync();

        _downloads.OnProgress -= OnDownloadProgress;
        _downloads.OnCompleted -= OnDownloadCompleted;
        _downloads.OnFailed -= OnDownloadFailed;
        _themeService.ThemeChanged -= OnThemeChanged;

        foreach (var id in _tabs.Select(t => t.Id).ToList())
        {
            if (_webViews.TryGetValue(id, out var wv))
            {
                DetachAndCleanState(id, wv);
                try { wv.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed; } catch (Exception ex) { Logger.Debug("Collapse visibility on restore cleanup", ex); }
                try { wv.Close(); } catch (Exception ex) { Logger.Debug("Close on restore cleanup", ex); }
            }
        }

        _sessionSaveLock.Dispose();
        _webViews.Clear(); _zoomLevels.Clear(); _internalPageTabs.Clear(); _ipcHandlers.Clear(); _acceleratorSubscriptions.Clear(); _wvHandlers.Clear(); _tabs.Clear();
    }
}

