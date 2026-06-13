using Microsoft.UI.Xaml;
using Microsoft.Web.WebView2.Core;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TB.Helpers;
using TB.Infrastructure;
using TB.Models;
using TB.Services.Interfaces;
namespace TB.Services;

public partial class TabManager
{
    public void NextTab() { if (_tabs.Count >= 2) SwitchTab(_tabs[(_tabs.FindIndex(t => t.Id == _activeId) + 1) % _tabs.Count].Id); }
    public void PrevTab() { if (_tabs.Count >= 2) SwitchTab(_tabs[(_tabs.FindIndex(t => t.Id == _activeId) - 1 + _tabs.Count) % _tabs.Count].Id); }
    public void GoToTab(int index) { if (index >= 1 && index <= 9) SwitchToIndex(index); }
    public void GoToLastTab() => SwitchToIndex(9);
    public void MoveTabLeft()
    {
        if (_activeId == -1) return;
        var idx = _tabs.FindIndex(t => t.Id == _activeId);
        if (idx > 0)
        {
            var t = _tabs[idx];
            _tabs.RemoveAt(idx);
            _tabs.Insert(idx - 1, t);
            TabMoved?.Invoke(this, new TabMovedEventArgs { TabId = _activeId, FromIndex = idx, ToIndex = idx - 1 });
            ScheduleSaveSession();
        }
    }
    public void MoveTabRight()
    {
        if (_activeId == -1) return;
        var idx = _tabs.FindIndex(t => t.Id == _activeId);
        if (idx >= 0 && idx < _tabs.Count - 1)
        {
            var t = _tabs[idx];
            _tabs.RemoveAt(idx);
            _tabs.Insert(idx + 1, t);
            TabMoved?.Invoke(this, new TabMovedEventArgs { TabId = _activeId, FromIndex = idx, ToIndex = idx + 1 });
            ScheduleSaveSession();
        }
    }
    public void NavigateActiveTab(string url)
    {
        if (_disposed || _activeId == -1) return; var wv = GetWebView(_activeId); if (wv == null) return;
        if (_tabs.FirstOrDefault(t => t.Id == _activeId) is not { } tab) return;
        if (UrlResolver.IsInternalUrl(url))
        {
            wv.Source = new Uri(UrlResolver.Resolve(url, _wwwrootPath));
            tab.Url = url;
            tab.Title = UrlResolver.GetTabTitle(url);
            tab.IsInternalPage = true;
            if (_internalPageTabs.Add(_activeId)) SetupInternalPageIpc(wv, _activeId);
        }
        else
        {
            if (_internalPageTabs.Remove(_activeId) && _ipcHandlers.TryGetValue(_activeId, out var prevHandler))
            {
                try { wv.CoreWebView2.WebMessageReceived -= prevHandler; }
                catch (Exception ex) { Logger.Debug($"Prev handler unsubscribe failed: {ex.Message}"); }
                _ipcHandlers.Remove(_activeId);
            }
            wv.Source = new Uri(url);
            tab.Url = url;
            tab.IsInternalPage = false;
        }
        ScheduleSaveSession();
    }
    public void Reload() { var wv = GetWebView(_activeId); if (_activeId != -1 && wv != null) wv.Reload(); }
    public void ReloadTab(int id) { if (!_disposed && (GetWebView(id) is { } wv)) wv.Reload(); }
    public void Stop() { var wv = GetWebView(_activeId); if (_activeId != -1 && wv != null) wv.CoreWebView2?.Stop(); }
    public void Back() { if (_activeId != -1 && (GetWebView(_activeId) is { } wv) && wv.CoreWebView2?.CanGoBack == true) wv.GoBack(); }
    public void Forward() { if (_activeId != -1 && (GetWebView(_activeId) is { } wv) && wv.CoreWebView2?.CanGoForward == true) wv.GoForward(); }
    public void Home() => NavigateActiveTab(Defaults.HomeUrl);
    public async Task SetZoomAsync(int tabId, double zoom)
    {
        var wv = GetWebView(tabId); if (wv == null || wv.CoreWebView2 == null) return;
        zoom = Math.Clamp(zoom, Defaults.MinZoom, Defaults.MaxZoom);
        _zoomLevels[tabId] = zoom;
        try
        {
            await wv.CoreWebView2.CallDevToolsProtocolMethodAsync("Emulation.setPageScaleFactor", JsonSerializer.Serialize(new { scaleFactor = zoom }));
        }
        catch
        {
            wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "zoom", a = new { v = zoom } }));
        }
    }
    public Task ZoomInAsync() => SetZoomAsync(_activeId, GetZoom(_activeId) + Defaults.ZoomStep);
    public Task ZoomOutAsync() => SetZoomAsync(_activeId, GetZoom(_activeId) - Defaults.ZoomStep);
    public Task ResetZoomAsync() => SetZoomAsync(_activeId, Defaults.DefaultZoom);
    private double GetZoom(int id) => _zoomLevels.GetValueOrDefault(id, Defaults.DefaultZoom);
    // Native FindBar handles the search session. Raise event to open native UI.
    public Task OpenFindBarAsync()
    {
        _isFindBarOpen = true;
        FindBarOpenRequested?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
    public Task FindNextAsync()
    {
        if (!_isFindBarOpen)
        {
            return OpenFindBarAsync();
        }
        var wv = GetWebView(_activeId);
        if (_activeId != -1 && wv != null && wv.CoreWebView2 != null)
        {
            try { wv.CoreWebView2.Find.FindNext(); }
            catch (Exception ex) { Logger.Warn("Native FindNext failed", ex); }
        }
        return Task.CompletedTask;
    }
    public Task FindPreviousAsync()
    {
        if (!_isFindBarOpen)
        {
            return OpenFindBarAsync();
        }
        var wv = GetWebView(_activeId);
        if (_activeId != -1 && wv != null && wv.CoreWebView2 != null)
        {
            try { wv.CoreWebView2.Find.FindPrevious(); }
            catch (Exception ex) { Logger.Warn("Native FindPrevious failed", ex); }
        }
        return Task.CompletedTask;
    }
    public async Task SavePageAsync()
    {
        var wv = GetWebView(_activeId);
        if (_activeId == -1 || wv == null || wv.CoreWebView2 == null) return;

        try
        {
            var htmlJson = await wv.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
            if (string.IsNullOrEmpty(htmlJson) || htmlJson == "null") return;
            var html = JsonSerializer.Deserialize<string>(htmlJson) ?? "";

            var titleJson = await wv.CoreWebView2.ExecuteScriptAsync("document.title");
            var title = JsonSerializer.Deserialize<string>(titleJson) ?? "page";
            var invalidChars = Path.GetInvalidFileNameChars();
            var safeTitle = string.Join("_", title.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).Trim();
            if (string.IsNullOrEmpty(safeTitle)) safeTitle = "page";

            if (App.MainWindow != null)
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
                savePicker.FileTypeChoices.Add("HTML Document", new System.Collections.Generic.List<string>() { ".html" });
                savePicker.SuggestedFileName = safeTitle;

                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    await Windows.Storage.FileIO.WriteTextAsync(file, html);
                }
            }
            else
            {
                var js = @"(function() {
                    var html = document.documentElement.outerHTML;
                    var blob = new Blob([html], {type: 'text/html'});
                    var a = document.createElement('a');
                    a.href = URL.createObjectURL(blob);
                    a.download = (document.title || 'page').replace(/[\/\\?%*:|""<>. ]/g, '_') + '.html';
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                })();";
                _ = wv.CoreWebView2.ExecuteScriptAsync(js);
            }
        }
        catch (Exception ex)
        {
            Logger.Error("SavePageAsync failed", ex);
        }
    }
    public Task CloseFindBarAsync()
    {
        _isFindBarOpen = false;
        FindBarCloseRequested?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
    public async Task StartFindAsync(string text)
    {
        var wv = GetWebView(_activeId);
        if (_activeId != -1 && wv != null && wv.CoreWebView2 != null)
        {
            try
            {
                var options = wv.CoreWebView2.Environment.CreateFindOptions();
                options.FindTerm = text;
                options.SuppressDefaultFindDialog = true;
                options.ShouldHighlightAllMatches = true;
                await wv.CoreWebView2.Find.StartAsync(options);
            }
            catch (Exception ex) { Logger.Warn($"Native StartFindAsync failed for text '{text}'", ex); }
        }
    }
    public Task StopFindAsync()
    {
        var wv = GetWebView(_activeId);
        if (_activeId != -1 && wv != null && wv.CoreWebView2 != null)
        {
            try { wv.CoreWebView2.Find.Stop(); }
            catch (Exception ex) { Logger.Debug("Native Stop failed", ex); }
        }
        return Task.CompletedTask;
    }
    // FIX: Removed 'async' and return Task.CompletedTask to eliminate CS1998 state machine overhead
    public Task HardReloadAsync()
    {
        var wv = GetWebView(_activeId); if (_activeId != -1 && wv != null)
            wv.CoreWebView2?.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "hardReload" }));
        return Task.CompletedTask;
    }
    public Task PrintAsync()
    {
        var wv = GetWebView(_activeId); if (_activeId != -1 && wv != null)
            wv.CoreWebView2?.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "print" }));
        return Task.CompletedTask;
    }
    public void ViewSource() { var wv = GetWebView(_activeId); if (_activeId != -1 && wv != null) wv.CoreWebView2?.OpenDevToolsWindow(); }
    public void FocusActiveTab() { var wv = GetWebView(_activeId); if (_activeId != -1 && wv != null) wv.Focus(FocusState.Programmatic); }
    public async Task DuplicateTabAsync(int id)
    {
        if (!_disposed) await CreateTabAsync(_tabs.FirstOrDefault(t => t.Id == id)?.Url ?? Defaults.HomeUrl);
    }
    public async Task ReopenLastClosedTabAsync()
    {
        if (_lastClosedUrls.Count > 0)
        {
            var url = _lastClosedUrls[^1];
            _lastClosedUrls.RemoveAt(_lastClosedUrls.Count - 1);
            await CreateTabAsync(url);
        }
    }
    public async Task OpenFeedbackWindowAsync() => await CreateTabAsync(Defaults.FeedbackUrl);
    public void ToggleBookmarksBar() => Logger.Info("Toggle bookmarks bar requested");
    public async Task OpenBookmarksManagerAsync() => await CreateTabAsync(Defaults.BookmarksUrl);
    public async Task OpenHistoryPageAsync() => await CreateTabAsync(Defaults.HistoryUrl);
    public async Task OpenDownloadsPageAsync() => await CreateTabAsync(Defaults.DownloadsUrl);
    public async Task OpenTaskManagerAsync() => await CreateTabAsync(Defaults.TaskManagerUrl);
    public Task OpenDeveloperToolsAsync()
    {
        var wv = GetWebView(_activeId); if (_activeId != -1 && wv != null)
            wv.CoreWebView2?.OpenDevToolsWindow();
        return Task.CompletedTask;
    }
    public Task OpenChromeMenuAsync()
    {
        Logger.Info("Open Chrome menu requested");
        return Task.CompletedTask;
    }
    public async Task OpenClearBrowsingDataDialogAsync() => await CreateTabAsync(Defaults.ClearDataUrl);
    public void AddressBarEnd() => FocusActiveTab();
    public Task PageTopAsync()
    {
        var wv = GetWebView(_activeId); if (_activeId != -1 && wv != null && wv.CoreWebView2 != null)
            wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "scrollTop" }));
        return Task.CompletedTask;
    }
    public Task PageBottomAsync()
    {
        var wv = GetWebView(_activeId); if (_activeId != -1 && wv != null && wv.CoreWebView2 != null)
            wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "scrollBottom" }));
        return Task.CompletedTask;
    }
    public void ToggleFullScreen() => Logger.Info("Toggle full screen requested");
    public void SelectMultipleTabs() => Logger.Info("Select multiple tabs requested");
    public Task CursorWordPreviousAsync()
    {
        var wv = GetWebView(_activeId); if (_activeId != -1 && wv != null && wv.CoreWebView2 != null)
            wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "cursorWordPrev" }));
        return Task.CompletedTask;
    }
    public Task CursorWordNextAsync()
    {
        var wv = GetWebView(_activeId); if (_activeId != -1 && wv != null && wv.CoreWebView2 != null)
            wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "cursorWordNext" }));
        return Task.CompletedTask;
    }
    public Task DeleteWordPreviousAsync()
    {
        var wv = GetWebView(_activeId); if (_activeId != -1 && wv != null && wv.CoreWebView2 != null)
            wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "deleteWordPrev" }));
        return Task.CompletedTask;
    }
}
