using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using TB.Features.Downloads;
using TB.Helpers;
using TB.Infrastructure;
using TB.Input;
using TB.Models;
using TB.Services.Interfaces;

namespace TB.Services;

public class TabManager : ITabManager
{
    private Grid? _contentGrid;
    private CoreWebView2Environment? _env;
    private readonly string _wwwrootPath;
    private readonly IThemeService _themeService;
    private readonly ISettingsService _settingsService;
    private readonly IDownloadService _downloads;
    private readonly KeyboardShortcutHandler _keyboardHandler;
    private readonly Dictionary<int, WebView2> _webViews = new();
    private readonly Dictionary<int, double> _zoomLevels = new();
    private readonly List<TabItem> _tabs = new();
    private readonly HashSet<int> _internalPageTabs = new();
    private readonly Dictionary<int, int> _downloadOwners = new();
    private readonly Dictionary<int, TypedEventHandler<CoreWebView2, CoreWebView2WebMessageReceivedEventArgs>> _ipcHandlers = new();
    private readonly Dictionary<int, IDisposable> _acceleratorSubscriptions = new();
    private readonly string _sessionPath;
    private int _nextId = 1;
    private int _activeId = -1;
    private bool _disposed;
    private bool _initialized;
    private bool _isRestoring;
    private readonly SemaphoreSlim _sessionSaveLock = new(1, 1);



    public int ActiveTabId => _activeId;
    public int TabCount => _tabs.Count;
    public bool IsInitialized => _initialized;
    public IReadOnlyList<TabItem> Tabs => _tabs.AsReadOnly();

    public event EventHandler<TabEventArgs>? TabCreated;
    public event EventHandler<TabEventArgs>? TabSwitched;
    public event EventHandler<TabEventArgs>? TabClosed;
    public event EventHandler<UrlEventArgs>? UrlChanged;
    public event EventHandler? NavigationStarted;
    public event EventHandler? NavigationCompleted;
#pragma warning disable CS0067 // event never raised (reserved for future use)
    public event EventHandler<EventArgs>? TabsCleared;
#pragma warning restore CS0067
    public event EventHandler<FaviconEventArgs>? FaviconUpdated;
    public event EventHandler? BeforeShutdown;
    public event EventHandler<NavStateEventArgs>? NavStateChanged;
    public event EventHandler<TabMovedEventArgs>? TabMoved;

    public TabManager(string basePath, IThemeService themeService, ISettingsService settingsService, IDownloadService downloads, KeyboardShortcutHandler keyboardHandler)
    {
        _wwwrootPath = Path.Combine(basePath, "wwwroot");
        _sessionPath = Path.Combine(basePath, "AppData", "session.json");
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _downloads = downloads ?? throw new ArgumentNullException(nameof(downloads));
        _keyboardHandler = keyboardHandler ?? throw new ArgumentNullException(nameof(keyboardHandler));

        _downloads.OnProgress += item => SendToDownloadOwner(item.Id, new { action = "DOWNLOAD_PROGRESS", id = item.Id, name = item.Name, url = item.Url, progressPercent = item.ProgressPercent, totalBytes = item.TotalBytes, status = item.Status });
        _downloads.OnCompleted += item => SendToDownloadOwner(item.Id, new { action = "DOWNLOAD_COMPLETED", id = item.Id, filePath = item.FilePath });
        _downloads.OnFailed += item => SendToDownloadOwner(item.Id, new { action = "DOWNLOAD_FAILED", id = item.Id, error = "Download failed" });

        _themeService.ThemeChanged += OnThemeChanged;
    }

    public async Task InitializeAsync(Grid contentGrid, CoreWebView2Environment env)
    {
        _contentGrid = contentGrid ?? throw new ArgumentNullException(nameof(contentGrid));
        _env = env ?? throw new ArgumentNullException(nameof(env));
        _initialized = true;
        Logger.Info("TabManager initialized");

        if (!await TryLoadSessionAsync())
        {
            Logger.Info("No session to restore, creating default tab");
            await CreateTabAsync(Defaults.HomeUrl);
        }
    }

    public async Task CreateTabAsync(string url = Defaults.HomeUrl)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureInitialized();

        int id = _nextId++;
        var webView = new WebView2
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Visibility = Visibility.Collapsed
        };

        _contentGrid!.Children.Add(webView);

        try
        {
            await webView.EnsureCoreWebView2Async(_env);
            try { webView.CoreWebView2.Settings.IsZoomControlEnabled = false; } catch (ObjectDisposedException) { }
            try { webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false; } catch (ObjectDisposedException) { }
            SubscribeToDownloadStarting(webView.CoreWebView2, id);

            webView.CoreWebView2.ProcessFailed += async (_, args) =>
            {
                var crashUrl = _tabs.FirstOrDefault(t => t.Id == id)?.Url ?? "";
                Logger.Error($"WebView process failed for tab {id}: {args.Reason}, url={crashUrl}");
                if (_webViews.TryGetValue(id, out var wv))
                {
                        try
                        {
                            var crashVars = _themeService.GetCssVariables();
                            crashVars.TryGetValue("--bg-app", out var bg);
                            crashVars.TryGetValue("--text-main", out var fg);
                            crashVars.TryGetValue("--text-muted", out var muted);
                            var html = $"<html><body style='background:{bg ?? "#13141a"};color:{fg ?? "#c8cdd8"};display:flex;flex-direction:column;align-items:center;justify-content:center;font-family:sans-serif;gap:12px;'><h2>Renderer crashed</h2><p style='color:{muted ?? "#6b7280"};cursor:pointer;' onclick='window.location=\"{crashUrl}\"'>Click to reload</p></body></html>";
                            wv.CoreWebView2?.NavigateToString(html);
                            Logger.Info($"Crash page shown for tab {id}, will reload to: {crashUrl}");
                        } catch (Exception crashEx)
                        {
                            Logger.Error($"Crash page render failed for tab {id}: {crashEx.Message}");
                        }
                }
            };

            webView.CoreWebView2.NewWindowRequested += (s, args) =>
            {
                args.Handled = true;
                var uri = args.Uri;
                if (!string.IsNullOrEmpty(uri))
                    _ = CreateTabAsync(uri);
            };

            webView.CoreWebView2.ContextMenuRequested += async (sender, args) =>
            {
                args.Handled = true;
                if (_disposed) return;
                try
                {
                    var pt = new Point(args.Location.X, args.Location.Y);

                    string mode = "standard", linkUrl = "", imgSrc = "";
                    try
                    {
                        var js = $@"
(function(){{
    var el = document.elementFromPoint({args.Location.X}, {args.Location.Y});
    if (!el) return {{m:'standard'}};
    var link = el.closest('a');
    var img = el.closest('img');
    var input = el.closest('input,textarea,[contenteditable]');
    if (input) return {{m:'text'}};
    if (img) return {{m:'image',s:img.src}};
    if (link) return {{m:'link',u:link.href}};
    return {{m:'standard'}};
}})()";
                        var json = await webView.CoreWebView2.ExecuteScriptAsync(js);
                        if (_disposed) return;
                        if (webView.CoreWebView2 == null) return;
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;
                        mode = root.GetProperty("m").GetString() ?? "standard";
                        if (mode == "link") linkUrl = root.GetProperty("u").GetString() ?? "";
                        if (mode == "image") imgSrc = root.GetProperty("s").GetString() ?? "";
                    }
                    catch { /* fallback to standard */ }

                    var capturedMode = mode;
                    var capturedLinkUrl = linkUrl;
                    var capturedImgSrc = imgSrc;

                    webView.DispatcherQueue.TryEnqueue(() =>
                    {
                        try
                        {
                            var presenterStyle = (Style)Application.Current.Resources["HeliumMenuFlyoutPresenterStyle"];
                            var itemStyle = (Style)Application.Current.Resources["HeliumMenuFlyoutItemStyle"];
                            var flyout = new MenuFlyout { MenuFlyoutPresenterStyle = presenterStyle };
                            var cwv = webView.CoreWebView2;
                            if (cwv == null) return;

                            void AddItem(string text, Action action)
                            {
                                var mi = new MenuFlyoutItem { Text = text, Style = itemStyle };
                                mi.Click += (o, e) => action();
                                flyout.Items.Add(mi);
                            }

                            if (capturedMode == "text")
                            {
                                AddItem("Undo", () => { var _ = cwv.ExecuteScriptAsync("document.execCommand('undo')"); });
                                AddItem("Redo", () => { var _ = cwv.ExecuteScriptAsync("document.execCommand('redo')"); });
                                flyout.Items.Add(new MenuFlyoutSeparator());
                                AddItem("Cut", () => { var _ = cwv.ExecuteScriptAsync("document.execCommand('cut')"); });
                                AddItem("Copy", () => { var _ = cwv.ExecuteScriptAsync("document.execCommand('copy')"); });
                                AddItem("Paste", () => { var _ = cwv.ExecuteScriptAsync("document.execCommand('paste')"); });
                                AddItem("Select All", () => { var _ = cwv.ExecuteScriptAsync("document.execCommand('selectAll')"); });
                                flyout.Items.Add(new MenuFlyoutSeparator());
                                AddItem("Inspect Element", () => { try { cwv.OpenDevToolsWindow(); } catch { } });
                            }
                            else if (capturedMode == "link")
                            {
                                var url = capturedLinkUrl;
                                AddItem("Open Link in New Tab", () => _ = CreateTabAsync(url));
                                AddItem("Copy Link Address", () =>
                                {
                                    try { var dp = new DataPackage(); dp.SetText(url); Clipboard.SetContent(dp); } catch { }
                                });
                                flyout.Items.Add(new MenuFlyoutSeparator());
                                AddItem("Inspect Element", () => { try { cwv.OpenDevToolsWindow(); } catch { } });
                            }
                            else if (capturedMode == "image")
                            {
                                var src = capturedImgSrc;
                                AddItem("Open Image in New Tab", () => _ = CreateTabAsync(src));
                                AddItem("Copy Image Link", () =>
                                {
                                    try { var dp = new DataPackage(); dp.SetText(src); Clipboard.SetContent(dp); } catch { }
                                });
                                AddItem("Save Image As...", async () =>
                                {
                                    try
                                    {
                                        var http = new HttpClient();
                                        var bytes = await http.GetByteArrayAsync(src);
                                        var file = await Windows.Storage.DownloadsFolder.CreateFileAsync(
                                            "image.png", Windows.Storage.CreationCollisionOption.GenerateUniqueName);
                                        await Windows.Storage.FileIO.WriteBytesAsync(file, bytes);
                                        Logger.Info($"Image saved to: {file.Path}");
                                    }
                                    catch (Exception ex) { Logger.Warning($"Save image failed: {ex.Message}"); }
                                });
                                flyout.Items.Add(new MenuFlyoutSeparator());
                                AddItem("Inspect Element", () => { try { cwv.OpenDevToolsWindow(); } catch { } });
                            }
                            else // standard
                            {
                                if (cwv.CanGoBack)
                                    AddItem("Back", () => webView.GoBack());
                                if (cwv.CanGoForward)
                                    AddItem("Forward", () => webView.GoForward());
                                AddItem("Reload", () => webView.Reload());
                                flyout.Items.Add(new MenuFlyoutSeparator());
                                AddItem("Inspect Element", () => { try { cwv.OpenDevToolsWindow(); } catch { } });
                            }

                            flyout.ShowAt(webView, pt);
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning($"Page context menu error: {ex.Message}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Page context menu setup error: {ex.Message}");
                }
            };

            var accelSub = WebView2ControllerAccessor.TrySubscribeAcceleratorKeyPressed(webView, key => _keyboardHandler.HandleKey(key));
            if (accelSub != null)
                _acceleratorSubscriptions[id] = accelSub;

            webView.KeyDown += (_, e) =>
            {
                var key = (Windows.System.VirtualKey)e.Key;
                if (_keyboardHandler.HandleKey(key))
                    e.Handled = true;
            };

            var tabItem = new TabItem
            {
                Id = id,
                Url = url,
                Title = UrlResolver.GetTabTitle(url),
                Zoom = Defaults.DefaultZoom
            };

            webView.CoreWebView2.NavigationStarting += (_, args) =>
            {
                if (id == _activeId)
                {
                    UrlChanged?.Invoke(this, new UrlEventArgs { Url = args.Uri });
                    NavigationStarted?.Invoke(this, EventArgs.Empty);
                }
            };

            webView.CoreWebView2.NavigationCompleted += (_, _) =>
            {
                if (id == _activeId && !_disposed)
                {
                    var tab = _tabs.FirstOrDefault(t => t.Id == id);
                    if (tab != null)
                        tab.Title = webView.CoreWebView2?.DocumentTitle ?? tab.Title;
                    NavigationCompleted?.Invoke(this, EventArgs.Empty);
                }
                FireNavState(id);
            };

            webView.CoreWebView2.DocumentTitleChanged += (_, _) =>
            {
                var newTitle = webView.CoreWebView2?.DocumentTitle ?? "";
                var tab = _tabs.FirstOrDefault(t => t.Id == id);
                if (tab != null && !tab.IsInternalPage)
                {
                    tab.Title = newTitle;
                    tab.Url = webView.CoreWebView2?.Source?.ToString() ?? tab.Url;
                    _ = SaveSessionAsync();
                }
            };

            webView.CoreWebView2.SourceChanged += (_, _) =>
            {
                if (id == _activeId)
                {
                    var src = webView.CoreWebView2?.Source?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(src))
                        UrlChanged?.Invoke(this, new UrlEventArgs { Url = src });
                }
                var srcTab = _tabs.FirstOrDefault(t => t.Id == id);
                if (srcTab != null)
                {
                    var src = webView.CoreWebView2?.Source?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(src) && src != srcTab.Url)
                    {
                        srcTab.Url = src;
                        _ = SaveSessionAsync();
                    }
                }
            };

            var _lastFaviconTimestamp = new Dictionary<int, long>();
            webView.CoreWebView2.FaviconChanged += async (_, _) =>
            {
                var now = Stopwatch.GetTimestamp();
                var ts = _lastFaviconTimestamp.GetValueOrDefault(id);
                if (now - ts < Stopwatch.Frequency) return;
                _lastFaviconTimestamp[id] = now;

                try
                {
                    using var stream = await webView.CoreWebView2.GetFaviconAsync(CoreWebView2FaviconImageFormat.Png);
                    if (stream == null) return;

                    var bitmap = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
                    await bitmap.SetSourceAsync(stream);
                    FaviconUpdated?.Invoke(this, new FaviconEventArgs { TabId = id, Favicon = bitmap });
                }
                catch (ObjectDisposedException) { }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed loading favicon for tab {id}: {ex.Message}");
                }
            };

        if (UrlResolver.IsInternalUrl(url))
        {
            tabItem.IsInternalPage = true;
            var themeVars = _themeService.GetCssVariables();
            var varsJson = JsonSerializer.Serialize(themeVars);
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync($"window.__themeVariables = {varsJson};");
            webView.Source = new Uri(UrlResolver.Resolve(url, _wwwrootPath));
            SetupInternalPageIpc(webView, id, url);
            _internalPageTabs.Add(id);
        }
        else
        {
                webView.Source = new Uri(url);
            }

            _tabs.Add(tabItem);
            _webViews[id] = webView;
            _zoomLevels[id] = Defaults.DefaultZoom;

            TabCreated?.Invoke(this, new TabEventArgs { Id = id, Title = tabItem.Title, Url = url });
            SwitchTab(id);
            await SaveSessionAsync();
            AssertConsistent();
        }
        catch
        {
            _contentGrid?.Children.Remove(webView);
            try { webView.Close(); } catch { }
            throw;
        }
    }

    public void SwitchTab(int id)
    {
        if (_disposed || !_webViews.ContainsKey(id)) return;

        foreach (var wv in _webViews.Values)
            wv.Visibility = Visibility.Collapsed;

        _webViews[id].Visibility = Visibility.Visible;
        _activeId = id;

        var tab = _tabs.FirstOrDefault(t => t.Id == id);
        TabSwitched?.Invoke(this, new TabEventArgs { Id = id, Title = tab?.Title ?? "", Url = tab?.Url ?? "" });
        try { UrlChanged?.Invoke(this, new UrlEventArgs { Url = _webViews[id]?.Source?.ToString() ?? "" }); } catch { }
        FireNavState(id);
        _ = SaveSessionAsync();
    }

    public void SwitchToIndex(int index)
    {
        if (_tabs.Count == 0) return;
        var target = index == 9 ? _tabs.Count - 1 : index - 1;
        if (target >= 0 && target < _tabs.Count)
            SwitchTab(_tabs[target].Id);
    }

    public void CloseTab(int id)
    {
        if (_disposed || !_webViews.TryGetValue(id, out var wv)) return;

        var closedTab = _tabs.FirstOrDefault(t => t.Id == id);
        if (closedTab is not null && !string.IsNullOrEmpty(closedTab.Url))
        {
            _lastClosedUrls.Add(closedTab.Url);
            if (_lastClosedUrls.Count > 10)
                _lastClosedUrls.RemoveAt(0);
        }

        if (_ipcHandlers.TryGetValue(id, out var ipcHandler))
        {
            try { wv.CoreWebView2.WebMessageReceived -= ipcHandler; } catch { }
            _ipcHandlers.Remove(id);
        }

        if (_acceleratorSubscriptions.TryGetValue(id, out var accelSub))
        {
            accelSub.Dispose();
            _acceleratorSubscriptions.Remove(id);
        }

        _contentGrid?.Children.Remove(wv);
        wv.Close();

        _webViews.Remove(id);
        _zoomLevels.Remove(id);
        _internalPageTabs.Remove(id);
        var ownerKeys = _downloadOwners.Where(kv => kv.Value == id).Select(kv => kv.Key).ToList();
        foreach (var key in ownerKeys)
            _downloadOwners.Remove(key);
        var closedIdx = _tabs.FindIndex(t => t.Id == id);
        _tabs.RemoveAll(t => t.Id == id);

        AssertConsistent();
        _ = SaveSessionAsync();
        TabClosed?.Invoke(this, new TabEventArgs { Id = id });

        if (_activeId == id && _tabs.Count > 0)
        {
            // Switch to the tab that was just to the right, or the last one
            var nextIdx = Math.Min(closedIdx, _tabs.Count - 1);
            SwitchTab(_tabs[Math.Max(0, nextIdx)].Id);
        }
        else if (_tabs.Count == 0)
        {
            TabsCleared?.Invoke(this, EventArgs.Empty);
            BeforeShutdown?.Invoke(this, EventArgs.Empty);
            Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()
                ?.TryEnqueue(Application.Current.Exit);
        }
    }

    public void CloseActiveTab() => CloseTab(_activeId);

    public void CloseOtherTabs(int id)
    {
        var others = _tabs.Where(t => t.Id != id).ToList();
        foreach (var tab in others)
            CloseTab(tab.Id);
    }

    public void CloseTabsToTheRight(int id)
    {
        var idx = _tabs.FindIndex(t => t.Id == id);
        if (idx < 0) return;
        var right = _tabs.Skip(idx + 1).ToList();
        foreach (var tab in right)
            CloseTab(tab.Id);
    }

    public void NextTab()
    {
        if (_tabs.Count < 2) return;
        var idx = _tabs.FindIndex(t => t.Id == _activeId);
        SwitchTab(_tabs[(idx + 1) % _tabs.Count].Id);
    }

    public void PrevTab()
    {
        if (_tabs.Count < 2) return;
        var idx = _tabs.FindIndex(t => t.Id == _activeId);
        SwitchTab(_tabs[(idx - 1 + _tabs.Count) % _tabs.Count].Id);
    }

    public void NavigateActiveTab(string url)
    {
        if (_disposed || _activeId == -1 || !_webViews.TryGetValue(_activeId, out var wv)) return;

        var tab = _tabs.FirstOrDefault(t => t.Id == _activeId);
        if (tab == null) return;

        if (UrlResolver.IsInternalUrl(url))
        {
            wv.Source = new Uri(UrlResolver.Resolve(url, _wwwrootPath));
            tab.Url = url;
            tab.Title = UrlResolver.GetTabTitle(url);
            tab.IsInternalPage = true;
            if (_internalPageTabs.Add(_activeId))
                SetupInternalPageIpc(wv, _activeId, url);
        }
        else
        {
            if (_internalPageTabs.Remove(_activeId) && _ipcHandlers.TryGetValue(_activeId, out var prevHandler))
            {
                try { wv.CoreWebView2.WebMessageReceived -= prevHandler; } catch { }
                _ipcHandlers.Remove(_activeId);
            }

            wv.Source = new Uri(url);
            tab.Url = url;
            tab.IsInternalPage = false;
        }
        _ = SaveSessionAsync();
    }

    public void Reload()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv))
            wv.Reload();
    }

    public void ReloadTab(int id)
    {
        if (!_disposed && _webViews.TryGetValue(id, out var wv))
            wv.Reload();
    }

    public void Stop()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv))
            wv.CoreWebView2?.Stop();
    }

    public void Back()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) &&
            wv.CoreWebView2?.CanGoBack == true)
            wv.GoBack();
    }

    public void Forward()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) &&
            wv.CoreWebView2?.CanGoForward == true)
            wv.GoForward();
    }

    public async Task SetZoomAsync(int tabId, double zoom)
    {
        if (!_webViews.TryGetValue(tabId, out var wv)) return;
        zoom = Math.Clamp(zoom, Defaults.MinZoom, Defaults.MaxZoom);
        _zoomLevels[tabId] = zoom;
        await wv.CoreWebView2.ExecuteScriptAsync($"document.body.style.zoom='{zoom}'");
    }

    public Task ZoomInAsync() =>
        SetZoomAsync(_activeId, GetZoom(_activeId) + Defaults.ZoomStep);

    public Task ZoomOutAsync() =>
        SetZoomAsync(_activeId, GetZoom(_activeId) - Defaults.ZoomStep);

    public Task ResetZoomAsync() =>
        SetZoomAsync(_activeId, Defaults.DefaultZoom);

    public async Task HardReloadAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv))
            await wv.CoreWebView2.ExecuteScriptAsync("location.reload(true);");
    }

    public async Task PrintAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv))
            await wv.CoreWebView2.ExecuteScriptAsync("window.print();");
    }

    public void ViewSource()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv))
            wv.CoreWebView2?.OpenDevToolsWindow();
    }

    public void FocusActiveTab()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv))
            wv.Focus(FocusState.Programmatic);
    }

    public async Task DuplicateTabAsync(int id)
    {
        if (_disposed) return;
        var tab = _tabs.FirstOrDefault(t => t.Id == id);
        var url = tab?.Url ?? Defaults.HomeUrl;
        Logger.Info($"Duplicating tab {id}: {url}");
        await CreateTabAsync(url);
    }

    // Chrome Parity Keyboard Shortcuts
    private List<string> _lastClosedUrls = new();

    public async Task ReopenLastClosedTabAsync()
    {
        if (_lastClosedUrls.Count == 0) return;
        
        string lastUrl = _lastClosedUrls[^1];
        _lastClosedUrls.RemoveAt(_lastClosedUrls.Count - 1);
        
        try
        {
            await CreateTabAsync(lastUrl);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to reopen tab with URL {lastUrl}: {ex.Message}");
        }
    }

    public void GoToTab(int index)
    {
        if (index < 1 || index > 9) return;
        SwitchToIndex(index);
    }

    public void GoToLastTab()
    {
        SwitchToIndex(9);
    }

    public void MoveTabLeft()
    {
        if (_activeId == -1) return;
        var activeIdx = _tabs.FindIndex(t => t.Id == _activeId);
        if (activeIdx > 0)
        {
            var activeTab = _tabs[activeIdx];
            _tabs.RemoveAt(activeIdx);
            _tabs.Insert(activeIdx - 1, activeTab);
            TabMoved?.Invoke(this, new TabMovedEventArgs { TabId = _activeId, FromIndex = activeIdx, ToIndex = activeIdx - 1 });
            _ = SaveSessionAsync();
        }
    }

    public void MoveTabRight()
    {
        if (_activeId == -1) return;
        var activeIdx = _tabs.FindIndex(t => t.Id == _activeId);
        if (activeIdx >= 0 && activeIdx < _tabs.Count - 1)
        {
            var activeTab = _tabs[activeIdx];
            _tabs.RemoveAt(activeIdx);
            _tabs.Insert(activeIdx + 1, activeTab);
            TabMoved?.Invoke(this, new TabMovedEventArgs { TabId = _activeId, FromIndex = activeIdx, ToIndex = activeIdx + 1 });
            _ = SaveSessionAsync();
        }
    }

    public async Task OpenFeedbackWindowAsync()
    {
        var feedbackUrl = Defaults.FeedbackUrl;
        NavigateActiveTab(feedbackUrl);
    }

    public void ToggleBookmarksBar()
    {
        Logger.Info("Toggle bookmarks bar requested (implementation pending)");
    }

    public async Task OpenBookmarksManagerAsync()
    {
        var bookmarksUrl = Defaults.BookmarksUrl;
        NavigateActiveTab(bookmarksUrl);
    }

    public async Task OpenHistoryPageAsync()
    {
        var historyUrl = Defaults.HistoryUrl;
        NavigateActiveTab(historyUrl);
    }

    public async Task OpenDownloadsPageAsync()
    {
        var downloadsUrl = Defaults.DownloadsUrl;
        NavigateActiveTab(downloadsUrl);
    }

    public async Task OpenTaskManagerAsync()
    {
        var taskManagerUrl = Defaults.TaskManagerUrl;
        NavigateActiveTab(taskManagerUrl);
    }

    public async Task OpenDeveloperToolsAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv))
            wv.CoreWebView2?.OpenDevToolsWindow();
    }

    public async Task OpenChromeMenuAsync()
    {
        Logger.Info("Open Chrome menu requested (implementation pending)");
    }

    public async Task OpenClearBrowsingDataDialogAsync()
    {
        var clearDataUrl = Defaults.ClearDataUrl;
        NavigateActiveTab(clearDataUrl);
    }

    private string GetFindBarScript()
    {
        var vars = _themeService.GetCssVariables();
        vars.TryGetValue("--bg-app", out var bg);
        vars.TryGetValue("--bg-tab-hover", out var surface);
        vars.TryGetValue("--text-main", out var text);
        vars.TryGetValue("--text-muted", out var subtext);
        vars.TryGetValue("--accent", out var accent);
        vars.TryGetValue("--border-crisp", out var border);
        var cLine = "var C={bg:'" + (bg ?? "#13141a")
            + "',surface:'" + (surface ?? "#1a1b24")
            + "',text:'" + (text ?? "#c8cdd8")
            + "',subtext:'" + (subtext ?? "#6b7280")
            + "',accent:'" + (accent ?? "#5b9cf6")
            + "',border:'" + (border ?? "#252636") + "'};";
        return "(function(){\n" + cLine + "\n" + FindBarScriptBody;
    }

    private const string FindBarScriptBody = @"if(document.getElementById('tb-find-bar')){
var inp=document.getElementById('tb-find-input');if(inp){inp.focus();inp.select();}
return;}
var marks=[],cur=-1,bar=document.createElement('div');
bar.id='tb-find-bar';
bar.style.cssText='position:fixed;top:61px;right:8px;width:340px;height:48px;z-index:99999;display:flex;align-items:center;padding:0 8px;background:'+C.bg+';border:1px solid '+C.border+';border-radius:4px;font-family:system-ui,sans-serif;box-sizing:border-box;';
var inp=document.createElement('input');
inp.id='tb-find-input';inp.type='text';inp.placeholder='Find in page...';
inp.style.cssText='flex:1;height:34px;box-sizing:border-box;background:transparent;border:none;outline:none;color:'+C.text+';font-size:13px;';
inp.addEventListener('input',function(){search(inp.value);});
inp.addEventListener('keydown',function(e){
if(e.key==='Enter'&&e.shiftKey){e.preventDefault();findPrev();}
else if(e.key==='Enter'){e.preventDefault();findNext();}
});
var cnt=document.createElement('span');
cnt.id='tb-find-counter';
cnt.style.cssText='font-size:11px;color:'+C.subtext+';margin-right:12px;white-space:nowrap;';
cnt.textContent='0 of 0';
function mkBtn(html,title,fn){
var b=document.createElement('button');
b.innerHTML=html;b.title=title;
b.style.cssText='width:22px;height:22px;box-sizing:border-box;border:none;border-radius:50%;background:transparent;color:'+C.subtext+';cursor:pointer;font-size:11px;display:flex;align-items:center;justify-content:center;transition:background 0ms,color 0ms;';
b.addEventListener('click',fn);
b.addEventListener('mouseenter',function(){b.style.background='rgba(255,255,255,0.15)';b.style.color=C.text;});
b.addEventListener('mouseleave',function(){b.style.background='transparent';b.style.color=C.subtext;});
return b;
}
var prevBtn=mkBtn('&#9650;','Previous (Shift+Enter)',findPrev);
prevBtn.style.marginRight='4px';
var nextBtn=mkBtn('&#9660;','Next (Enter)',findNext);
nextBtn.style.marginRight='8px';
var closeBtn=document.createElement('button');
closeBtn.innerHTML='&#10005;';closeBtn.title='Close (Esc)';
closeBtn.style.cssText='width:22px;height:22px;box-sizing:border-box;border:none;border-radius:50%;background:transparent;color:'+C.subtext+';cursor:pointer;font-size:11px;display:flex;align-items:center;justify-content:center;transition:background 0ms,color 0ms;';
closeBtn.addEventListener('click',closeBar);
closeBtn.addEventListener('mouseenter',function(){closeBtn.style.background='rgba(255,255,255,0.15)';closeBtn.style.color=C.text;});
closeBtn.addEventListener('mouseleave',function(){closeBtn.style.background='transparent';closeBtn.style.color=C.subtext;});
bar.appendChild(inp);bar.appendChild(cnt);bar.appendChild(prevBtn);bar.appendChild(nextBtn);bar.appendChild(closeBtn);
document.body.appendChild(bar);
inp.focus();inp.select();
function search(t){
document.querySelectorAll('.tb-find-wrap').forEach(function(w){
var txt=document.createTextNode(w.textContent);
w.parentNode.replaceChild(txt,w);
});
marks=[];cur=-1;
if(!t){cnt.textContent='0 of 0';return;}
var rx=new RegExp(t.replace(/[.*+?^${}()|[\]\\]/g,'\\$&'),'gi');
var walker=document.createTreeWalker(document.body,4,null,false);
var nodes=[],node;
while(node=walker.nextNode()){
if(node.parentElement&&node.parentElement.closest&&(node.parentElement.closest('#tb-find-bar')||node.parentElement.tagName==='STYLE'||node.parentElement.tagName==='SCRIPT'))continue;
rx.lastIndex=0;if(rx.test(node.textContent))nodes.push(node);
}
nodes.forEach(function(tn){
var txt=tn.textContent;rx.lastIndex=0;var m,last=0,frag=document.createDocumentFragment();
while(m=rx.exec(txt)){
if(m.index>last)frag.appendChild(document.createTextNode(txt.substring(last,m.index)));
var mk=document.createElement('mark');
mk.style.cssText='background:'+C.accent+'33;color:inherit;border-radius:2px;padding:0;';
mk.textContent=m[0];marks.push(mk);frag.appendChild(mk);last=rx.lastIndex;
}
if(last<txt.length)frag.appendChild(document.createTextNode(txt.substring(last)));
var wrap=document.createElement('span');
wrap.className='tb-find-wrap';
wrap.style.cssText='display:inline;';
wrap.appendChild(frag);
tn.parentNode.replaceChild(wrap,tn);
});
if(marks.length>0){cur=0;marks[0].style.background=C.accent+'99';}
cnt.textContent=(cur+1)+' of '+marks.length;
}
function findNext(){
if(marks.length===0)return;
if(cur>=0)marks[cur].style.background=C.accent+'33';
cur=(cur+1)%marks.length;
marks[cur].style.background=C.accent+'99';
marks[cur].scrollIntoView({behavior:'smooth',block:'center'});
cnt.textContent=(cur+1)+' of '+marks.length;
}
function findPrev(){
if(marks.length===0)return;
if(cur>=0)marks[cur].style.background=C.accent+'33';
cur=(cur-1+marks.length)%marks.length;
marks[cur].style.background=C.accent+'99';
marks[cur].scrollIntoView({behavior:'smooth',block:'center'});
cnt.textContent=(cur+1)+' of '+marks.length;
}
function closeBar(){
document.querySelectorAll('.tb-find-wrap').forEach(function(w){
var txt=document.createTextNode(w.textContent);
w.parentNode.replaceChild(txt,w);
});
marks=[];cur=-1;
if(bar.parentNode)bar.parentNode.removeChild(bar);
}
window.__findNext=findNext;window.__findPrev=findPrev;window.__closeFindBar=closeBar;window.__tbFindActive=true;
})();";

    public async Task OpenFindBarAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            await wv.CoreWebView2.ExecuteScriptAsync(GetFindBarScript());
    }

    public async Task FindNextAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
        {
            await wv.CoreWebView2.ExecuteScriptAsync(GetFindBarScript());
            await wv.CoreWebView2.ExecuteScriptAsync("window.__findNext && window.__findNext();");
        }
    }

    public async Task FindPreviousAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
        {
            await wv.CoreWebView2.ExecuteScriptAsync(GetFindBarScript());
            await wv.CoreWebView2.ExecuteScriptAsync("window.__findPrev && window.__findPrev();");
        }
    }

    public async Task CloseFindBarAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            await wv.CoreWebView2.ExecuteScriptAsync("window.__closeFindBar && window.__closeFindBar();");
    }

    public void AddressBarEnd()
    {
        FocusActiveTab();
    }

    public Task PageTopAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            return wv.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, 0);").AsTask();
        return Task.CompletedTask;
    }

    public Task PageBottomAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            return wv.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, document.body.scrollHeight);").AsTask();
        return Task.CompletedTask;
    }

    public void ToggleFullScreen()
    {
        Logger.Info("Toggle full screen requested (implementation pending)");
    }

    public Task CursorWordPreviousAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            return wv.CoreWebView2.ExecuteScriptAsync("var selection = window.getSelection(); var range = selection.getRangeAt(0); var newRange = document.createRange(); var node = range.startContainer; while (node.previousSibling) { newRange.setStart(node.previousSibling, 0); break; } selection.removeAllRanges(); selection.addRange(newRange);".ToLower()).AsTask();
        return Task.CompletedTask;
    }

    public Task CursorWordNextAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            return wv.CoreWebView2.ExecuteScriptAsync("var selection = window.getSelection(); var range = selection.getRangeAt(0); var newRange = document.createRange(); var node = range.startContainer; while (node.nextSibling) { newRange.setStart(node.nextSibling, 0); break; } selection.removeAllRanges(); selection.addRange(newRange);".ToLower()).AsTask();
        return Task.CompletedTask;
    }

    public Task DeleteWordPreviousAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            return wv.CoreWebView2.ExecuteScriptAsync("var selection = window.getSelection(); var range = selection.getRangeAt(0); var node = range.startContainer; if (node.nodeType === Node.TEXT_NODE) { var text = node.textContent; var before = text.substring(0, range.startOffset - 1); var after = text.substring(range.startOffset); node.textContent = before + after; range.setStart(node, before.length); selection.removeAllRanges(); selection.addRange(range); }".ToLower()).AsTask();
        return Task.CompletedTask;
    }

    public void SelectMultipleTabs()
    {
        Logger.Info("Select multiple tabs requested (implementation pending)");
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        Logger.Info($"Shutting down: saving session ({_tabs.Count} tabs)");
        await SaveSessionAsync();

        foreach (var id in _tabs.Select(t => t.Id).ToList())
        {
            if (_ipcHandlers.TryGetValue(id, out var ipcHandler) && _webViews.TryGetValue(id, out var wv))
            {
                try { wv.CoreWebView2.WebMessageReceived -= ipcHandler; } catch { }
                _ipcHandlers.Remove(id);
            }

            if (_acceleratorSubscriptions.TryGetValue(id, out var accelSub))
            {
                accelSub.Dispose();
                _acceleratorSubscriptions.Remove(id);
            }

            if (_webViews.TryGetValue(id, out var wv2))
            {
                _contentGrid?.Children.Remove(wv2);
                try { wv2.Close(); } catch (ObjectDisposedException) { }
            }
        }

        _webViews.Clear();
        _zoomLevels.Clear();
        _internalPageTabs.Clear();
        _ipcHandlers.Clear();
        _acceleratorSubscriptions.Clear();
        _tabs.Clear();
    }

    private void FireNavState(int id)
    {
        if (_disposed || !_webViews.TryGetValue(id, out var wv)) return;
        try
        {
            var core = wv.CoreWebView2;
            NavStateChanged?.Invoke(this, new NavStateEventArgs
            {
                CanGoBack = core?.CanGoBack ?? false,
                CanGoForward = core?.CanGoForward ?? false,
                Title = _tabs.FirstOrDefault(t => t.Id == id)?.Title ?? ""
            });
        }
        catch (ObjectDisposedException) { }
    }

    private void EnsureInitialized()
    {
        if (!_initialized)
            throw new InvalidOperationException("TabManager not initialized. Call InitializeAsync first.");
    }

    [Conditional("DEBUG")]
    private void AssertConsistent()
    {
        var tabIds = new HashSet<int>(_tabs.Select(t => t.Id));
        var viewIds = new HashSet<int>(_webViews.Keys);
        var zoomIds = new HashSet<int>(_zoomLevels.Keys);

        Debug.Assert(tabIds.SetEquals(viewIds), $"Tab IDs and WebView IDs out of sync (tabs={_tabs.Count}, views={_webViews.Count})");
        Debug.Assert(tabIds.SetEquals(zoomIds), $"Tab IDs and zoom levels out of sync (tabs={_tabs.Count}, zooms={_zoomLevels.Count})");
        Debug.Assert(_activeId == -1 || _webViews.ContainsKey(_activeId), $"Active tab {_activeId} has no WebView");
        foreach (var id in _internalPageTabs)
            Debug.Assert(_webViews.ContainsKey(id), $"Internal page tab {id} has no WebView");
    }

    private async Task SaveSessionAsync()
    {
        if (_isRestoring) return;
        await _sessionSaveLock.WaitAsync();
        try
        {
            var state = new SessionState
            {
                ActiveTabIndex = _activeId == -1 ? -1 : _tabs.FindIndex(t => t.Id == _activeId),
                Tabs = _tabs.Select(t => new TabEntry { Url = t.Url, Title = t.Title }).ToList()
            };
            var dir = Path.GetDirectoryName(_sessionPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_sessionPath, json);
            Logger.Debug($"Session saved: {_tabs.Count} tabs, active={state.ActiveTabIndex}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to save session: {ex.Message}");
        }
        finally
        {
            _sessionSaveLock.Release();
        }
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
            Logger.Info($"Restoring session: {state.Tabs.Count} tabs, active={state.ActiveTabIndex}");
            _isRestoring = true;
            foreach (var entry in state.Tabs)
            {
                var url = string.IsNullOrEmpty(entry.Url) ? Defaults.HomeUrl : entry.Url;
                await CreateTabAsync(url);
            }
            _isRestoring = false;
            await SaveSessionAsync();
            if (state.ActiveTabIndex >= 0 && state.ActiveTabIndex < _tabs.Count)
                SwitchTab(_tabs[state.ActiveTabIndex].Id);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load session: {ex.Message}");
            _isRestoring = false;
            return false;
        }
    }

    private double GetZoom(int id) =>
        _zoomLevels.GetValueOrDefault(id, Defaults.DefaultZoom);

    private void SubscribeToDownloadStarting(CoreWebView2 coreWebView2, int tabId)
    {
        coreWebView2.DownloadStarting += (_, args) =>
        {
            try
            {
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var fileName = Path.GetFileName(args.ResultFilePath);
                if (string.IsNullOrEmpty(fileName))
                    fileName = "download_" + DateTime.Now.Ticks;
                var dlPath = Path.Combine(basePath, "AppData", "Downloads", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(dlPath)!);
                args.ResultFilePath = dlPath;

                var dlItem = _downloads.AddDownload(args.DownloadOperation.Uri, fileName, dlPath, (long)args.DownloadOperation.TotalBytesToReceive);
                _downloadOwners[dlItem.Id] = tabId;
                args.Handled = true;

                var op = args.DownloadOperation;
                op.StateChanged += (_, _) =>
                {
                    if (op.State == CoreWebView2DownloadState.Completed)
                        _downloads.CompleteDownload(dlItem.Id);
                    else if (op.State == CoreWebView2DownloadState.Interrupted)
                        _downloads.FailDownload(dlItem.Id, "Download interrupted");
                };
                op.BytesReceivedChanged += (_, _) =>
                    _downloads.UpdateProgress(dlItem.Id, (long)op.BytesReceived);
            }
            catch (Exception ex)
            {
                Logger.Error($"Download error: {ex.Message}");
            }
        };
    }

    private void SetupInternalPageIpc(WebView2 wv, int tabId, string page)
    {
        TypedEventHandler<CoreWebView2, CoreWebView2WebMessageReceivedEventArgs> handler = async (s, e) =>
        {
            try
            {
                if (!_internalPageTabs.Contains(tabId))
                    return;

                string origin = e.Source ?? "";
                if (!origin.StartsWith("tb://", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Warning($"IPC message from untrusted origin: {origin}");
                    return;
                }

                using var doc = JsonDocument.Parse(e.WebMessageAsJson);
                var root = doc.RootElement;
                if (!root.TryGetProperty("action", out var actionProp) || actionProp.ValueKind != JsonValueKind.String)
                {
                    Logger.Warning($"IPC message missing or invalid 'action' field");
                    return;
                }
                string action = actionProp.GetString() ?? "";

                switch (action)
                {
                    case "SETTINGS_READY":
                        var settingsService = _settingsService;
                        var settingsJson = settingsService.GetAllJson();
                        await wv.CoreWebView2.ExecuteScriptAsync(
                            $"window.dispatchEvent(new MessageEvent('message', {{ data: {{ action: 'SETTINGS_DATA', settings: {settingsJson} }} }}))");
                        break;

                    case "SAVE_SETTING":
                        if (!root.TryGetProperty("key", out var keyProp) || keyProp.ValueKind != JsonValueKind.String)
                        { Logger.Warning("SAVE_SETTING missing 'key'"); break; }
                        if (!root.TryGetProperty("value", out var valueEl))
                        { Logger.Warning("SAVE_SETTING missing 'value'"); break; }
                        string key = keyProp.GetString() ?? "";
                        var settingsSvc = _settingsService;
                        if (valueEl.ValueKind == JsonValueKind.True || valueEl.ValueKind == JsonValueKind.False)
                            settingsSvc.Set(key, valueEl.GetBoolean());
                        else if (valueEl.ValueKind == JsonValueKind.Number)
                            settingsSvc.Set(key, valueEl.GetInt32());
                        else if (valueEl.ValueKind == JsonValueKind.String)
                            settingsSvc.Set(key, valueEl.GetString() ?? "");
                        break;

                    case "GET_DOWNLOADS":
                        var downloadsService = _downloads;
                        var list = downloadsService.Downloads.Select(DownloadViewModel.FromItem).ToList();
                        var json = JsonSerializer.Serialize(new { action = "DOWNLOADS_LIST", downloads = list });
                        await wv.CoreWebView2.ExecuteScriptAsync(
                            $"window.dispatchEvent(new MessageEvent('message', {{ data: {json} }}))");
                        break;

                    case "REMOVE_DOWNLOAD":
                        if (!root.TryGetProperty("id", out var rmIdProp) || rmIdProp.ValueKind != JsonValueKind.Number)
                        { Logger.Warning("REMOVE_DOWNLOAD missing/invalid 'id'"); break; }
                        var rmId = rmIdProp.GetInt32();
                        _downloadOwners.Remove(rmId);
                        _downloads.RemoveDownload(rmId);
                        break;

                    case "CLEAR_DOWNLOADS":
                        _downloadOwners.Clear();
                        _downloads.ClearAll();
                        break;

                    case "CLOSE_SETTINGS":
                        CloseTab(tabId);
                        break;

                    case "BROWSE_FOLDER":
                        Logger.Info("Browse folder requested (OS dialog not yet implemented)");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Internal page IPC error: {ex.Message}");
            }
        };

        wv.CoreWebView2.WebMessageReceived += handler;
        _ipcHandlers[tabId] = handler;
    }

    private void OnThemeChanged()
    {
        var themeVars = _themeService.GetCssVariables();
        var json = JsonSerializer.Serialize(new { action = "THEME_UPDATE", variables = themeVars });
        foreach (var id in _internalPageTabs)
        {
            if (_webViews.TryGetValue(id, out var wv))
            {
                try
                {
                    wv.CoreWebView2?.PostWebMessageAsJson(json);
                }
                catch (ObjectDisposedException) { }
                catch (COMException) { }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed sending theme update to tab {id}: {ex.Message}");
                }
            }
        }
    }

    private void SendToDownloadOwner(int downloadId, object data)
    {
        var json = JsonSerializer.Serialize(data);
        var targetIds = new HashSet<int>();

        if (_downloadOwners.TryGetValue(downloadId, out var ownerId))
            targetIds.Add(ownerId);

        foreach (var id in _internalPageTabs)
        {
            if (_tabs.Any(t => t.Id == id && t.Url == "tb://downloads"))
                targetIds.Add(id);
        }

        foreach (var id in targetIds)
        {
            if (_webViews.TryGetValue(id, out var wv))
            {
                try { wv.CoreWebView2?.PostWebMessageAsJson(json); }
                catch (ObjectDisposedException) { }
                catch (COMException) { }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed sending download message to tab {id}: {ex.Message}");
                }
            }
        }
    }
}
