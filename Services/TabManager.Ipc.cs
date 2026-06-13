using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TB.Services.Downloads;
using TB.Helpers;
using TB.Infrastructure;
using Windows.Foundation;

namespace TB.Services;

public static class IpcActions
{
    public const string DownloadProgress = "DOWNLOAD_PROGRESS";
    public const string DownloadCompleted = "DOWNLOAD_COMPLETED";
    public const string DownloadFailed = "DOWNLOAD_FAILED";
    public const string SettingsReady = "SETTINGS_READY";
    public const string SaveSetting = "SAVE_SETTING";
    public const string GetDownloads = "GET_DOWNLOADS";
    public const string RemoveDownload = "REMOVE_DOWNLOAD";
    public const string ClearDownloads = "CLEAR_DOWNLOADS";
    public const string CloseSettings = "CLOSE_SETTINGS";
    public const string BrowseFolder = "BROWSE_FOLDER";
    public const string ThemeUpdate = "THEME_UPDATE";
    public const string SettingsData = "SETTINGS_DATA";
    public const string DownloadsList = "DOWNLOADS_LIST";
    public const string GetThemes = "GET_THEMES";
    public const string ThemesList = "THEMES_LIST";
    public const string FlagsReady = "FLAGS_READY";
    public const string FlagsData = "FLAGS_DATA";
    public const string RelaunchBrowser = "RELAUNCH_BROWSER";
}

public partial class TabManager
{
    internal void SetupInternalPageIpc(WebView2 wv, int tabId)
    {
        TypedEventHandler<CoreWebView2, CoreWebView2WebMessageReceivedEventArgs> handler = async (s, e) =>
        {
            if (!_internalPageTabs.Contains(tabId)) return;

            try
            {
                using var doc = JsonDocument.Parse(e.WebMessageAsJson);
                if (!doc.RootElement.TryGetProperty("action", out var actionProp)) return;
                await RouteIpcMessage(tabId, actionProp.GetString() ?? "", doc.RootElement, wv);
            }
            catch (Exception ex) { Logger.Error($"IPC error: {ex.Message}"); }
        };

        wv.CoreWebView2.WebMessageReceived += handler;
        _ipcHandlers[tabId] = handler;
    }

    private async Task RouteIpcMessage(int tabId, string action, JsonElement root, WebView2 wv)
    {
        switch (action)
        {
            case IpcActions.SettingsReady:
                var settingsJson = _settingsService.GetAllJson();
                var themes = _themeService.GetAvailableThemes().Select(t => new { id = t.Id, name = t.Name }).ToList();
                var settingsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(settingsJson) ?? new Dictionary<string, object>();

                // FIX 1: Flat payload via PostWebMessageAsJson (Matches JS chrome.webview listener)
                var settingsResponse = new { action = IpcActions.SettingsData, settings = settingsDict, themes = themes };
                wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(settingsResponse));
                break;

            case IpcActions.GetThemes:
                var availableThemes = _themeService.GetAvailableThemes().Select(t => new { id = t.Id, name = t.Name }).ToList();
                var themesResponse = new { action = IpcActions.ThemesList, themes = availableThemes };
                wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(themesResponse));
                break;

            case IpcActions.SaveSetting:
                if (root.TryGetProperty("key", out var k) && root.TryGetProperty("value", out var v))
                {
                    var key = k.GetString() ?? "";
                    var valStr = v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : v.ToString();
                    _settingsService.Set(key, valStr);

                    if (key == "theme-name" && !string.IsNullOrEmpty(valStr))
                        _ = _themeService.SetThemeAsync(valStr);
                }
                break;

            case IpcActions.GetDownloads:
                var list = _downloads.Downloads.Select(DownloadViewModel.FromItem).ToList();
                // FIX 2: Flat payload (Removed the nested "dispatchIpc" wrapper)
                var dlResponse = new { action = IpcActions.DownloadsList, downloads = list };
                wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(dlResponse));
                break;

            case IpcActions.RemoveDownload:
                if (root.TryGetProperty("id", out var rmId))
                {
                    _downloadOwners.Remove(rmId.GetInt32());
                    _downloads.RemoveDownload(rmId.GetInt32());
                }
                break;

            case IpcActions.ClearDownloads:
                _downloadOwners.Clear();
                _downloads.ClearAll();
                break;

            case IpcActions.CloseSettings:
                CloseTab(tabId);
                break;

            case IpcActions.FlagsReady:
                var payload = _flagService.GetFlagsDataPayload();
                var flagsResponse = new { action = IpcActions.FlagsData, data = payload };
                var serializeOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(flagsResponse, serializeOptions));
                break;

            case IpcActions.RelaunchBrowser:
                var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(exePath))
                {
                    System.Diagnostics.Process.Start(exePath);
                    if (_dispatcherQueue.HasThreadAccess) Microsoft.UI.Xaml.Application.Current.Exit();
                    else _dispatcherQueue.TryEnqueue(Microsoft.UI.Xaml.Application.Current.Exit);
                }
                break;
        }
    }

    private void HandleDownloadStarting(object? sender, CoreWebView2DownloadStartingEventArgs args, int tabId)
    {
        if (tabId == 0) return;
        try
        {
            var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            var downloadDir = _settingsService.Get<string>("download-path", defaultPath) ?? defaultPath;
            Directory.CreateDirectory(downloadDir);

            var fileName = Path.GetFileName(args.ResultFilePath);
            if (string.IsNullOrEmpty(fileName)) fileName = "download_" + DateTime.Now.Ticks;

            var proposedPath = Path.GetFullPath(Path.Combine(downloadDir, fileName));
            if (!proposedPath.StartsWith(Path.GetFullPath(downloadDir), StringComparison.OrdinalIgnoreCase))
            {
                fileName = "download_" + DateTime.Now.Ticks;
                proposedPath = Path.GetFullPath(Path.Combine(downloadDir, fileName));
            }

            args.ResultFilePath = proposedPath;
            var dlItem = _downloads.AddDownload(args.DownloadOperation.Uri, fileName, proposedPath, (long)args.DownloadOperation.TotalBytesToReceive);
            _downloadOwners[dlItem.Id] = tabId;
            args.Handled = true;

            var op = args.DownloadOperation;
            op.StateChanged += (_, _) =>
            {
                if (op.State == CoreWebView2DownloadState.Completed) _downloads.CompleteDownload(dlItem.Id);
                else if (op.State == CoreWebView2DownloadState.Interrupted) _downloads.FailDownload(dlItem.Id, "Interrupted");
            };
            op.BytesReceivedChanged += (_, _) => _downloads.UpdateProgress(dlItem.Id, (long)op.BytesReceived);
        }
        catch (Exception ex) { Logger.Error($"Download error: {ex.Message}"); }
    }

    private void OnDownloadProgress(DownloadItem item) => SendToDownloadOwner(item.Id, new { action = IpcActions.DownloadProgress, id = item.Id, name = item.Name, url = item.Url, progressPercent = item.ProgressPercent, totalBytes = item.TotalBytes, status = item.Status });
    private void OnDownloadCompleted(DownloadItem item) => SendToDownloadOwner(item.Id, new { action = IpcActions.DownloadCompleted, id = item.Id, filePath = item.FilePath });
    private void OnDownloadFailed(DownloadItem item) => SendToDownloadOwner(item.Id, new { action = IpcActions.DownloadFailed, id = item.Id, error = "Download failed" });

    private void SendToDownloadOwner(int downloadId, object data)
    {
        if (!_dispatcherQueue.HasThreadAccess)
        {
            var dId = downloadId;
            var d = data;
            _dispatcherQueue.TryEnqueue(() => SendToDownloadOwner(dId, d));
            return;
        }

        var json = JsonSerializer.Serialize(data);
        HashSet<int> targetIds = [];
        if (_downloadOwners.TryGetValue(downloadId, out var ownerId)) targetIds.Add(ownerId);
        foreach (var id in _internalPageTabs) { if (_tabs.Any(t => t.Id == id && t.Url == Routes.Downloads)) targetIds.Add(id); }

        foreach (var id in targetIds)
        {
            var wv = GetWebView(id); if (wv != null)
            {
                try { wv.CoreWebView2?.PostWebMessageAsJson(json); }
                catch (Exception ex) { Logger.Warning($"PostWebMessageAsJson failed: {ex.Message}"); }
            }
        }
    }

    private void OnThemeChanged()
    {
        if (!_dispatcherQueue.HasThreadAccess)
        {
            _dispatcherQueue.TryEnqueue(OnThemeChanged);
            return;
        }

        var themeVars = _themeService.GetCssVariables();
        var flatJson = JsonSerializer.Serialize(new { action = IpcActions.ThemeUpdate, variables = themeVars });
        var initScript = $"window.__themeVariables = {JsonSerializer.Serialize(themeVars)};";

        foreach (var tab in _tabs)
        {
            var id = tab.Id;
            var wv = GetWebView(id); if (wv != null && wv.CoreWebView2 != null)
            {
                try
                {
                    // Inject for future navigations/reloads
                    _ = wv.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(initScript);
                    // Broadcast to currently loaded DOM
                    wv.CoreWebView2.PostWebMessageAsJson(flatJson);
                }
                catch (Exception ex) { Logger.Warning($"Theme broadcast failed: {ex.Message}"); }
            }
        }
    }
}
