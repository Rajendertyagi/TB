using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TB.Helpers;
using TB.Infrastructure;
using TB.Models;
using TB.Services.Interfaces;
using Windows.Foundation;

namespace TB.Services;

public partial class TabManager
{
    internal class WebView2EventHandlers
    {
        public TypedEventHandler<CoreWebView2, CoreWebView2ProcessFailedEventArgs>? ProcessFailed;
        public TypedEventHandler<CoreWebView2, CoreWebView2NewWindowRequestedEventArgs>? NewWindowRequested;
        public TypedEventHandler<CoreWebView2, CoreWebView2NavigationStartingEventArgs>? NavigationStarting;
        public TypedEventHandler<CoreWebView2, CoreWebView2NavigationCompletedEventArgs>? NavigationCompleted;
        public TypedEventHandler<CoreWebView2, object>? DocumentTitleChanged;
        public TypedEventHandler<CoreWebView2, CoreWebView2SourceChangedEventArgs>? SourceChanged;
        public TypedEventHandler<CoreWebView2, object>? FaviconChanged;
        public TypedEventHandler<CoreWebView2, CoreWebView2ContextMenuRequestedEventArgs>? ContextMenuRequested;
    }

    internal WebView2EventHandlers CreateEventHandlers(int id, WebView2 webView)
    {
        return new WebView2EventHandlers
        {
            ProcessFailed = (s, args) => HandleProcessFailed(id, args),
            NewWindowRequested = (s, args) => HandleNewWindowRequested(id, args),
            NavigationStarting = (s, args) => HandleNavigationStarting(id, args),
            NavigationCompleted = (s, args) => HandleNavigationCompleted(id),
            DocumentTitleChanged = (s, args) => HandleDocumentTitleChanged(id, webView),
            SourceChanged = (s, args) => HandleSourceChanged(id),
            FaviconChanged = async (s, args) => await HandleFaviconChanged(id, webView),
            ContextMenuRequested = (s, args) => HandleContextMenuRequested(id, args)
        };
    }

    internal void AttachCoreEvents(CoreWebView2 core, WebView2EventHandlers handlers, int tabId)
    {
        core.ProcessFailed += handlers.ProcessFailed;
        core.NewWindowRequested += handlers.NewWindowRequested;
        core.NavigationStarting += handlers.NavigationStarting;
        core.NavigationCompleted += handlers.NavigationCompleted;
        core.DocumentTitleChanged += handlers.DocumentTitleChanged;
        core.SourceChanged += handlers.SourceChanged;
        core.FaviconChanged += handlers.FaviconChanged;
        core.ContextMenuRequested += handlers.ContextMenuRequested;
        core.DownloadStarting += (s, args) => HandleDownloadStarting(s, args, tabId);

        try
        {
            var find = core.Find;
            find.MatchCountChanged += (s, args) =>
            {
                if (tabId == _activeId)
                {
                    FindResultReceived?.Invoke(this, new FindResultEventArgs
                    {
                        ActiveMatchIndex = find.ActiveMatchIndex,
                        MatchCount = find.MatchCount
                    });
                }
            };
            find.ActiveMatchIndexChanged += (s, args) =>
            {
                if (tabId == _activeId)
                {
                    FindResultReceived?.Invoke(this, new FindResultEventArgs
                    {
                        ActiveMatchIndex = find.ActiveMatchIndex,
                        MatchCount = find.MatchCount
                    });
                }
            };
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to subscribe to Find events for tab {tabId}", ex);
        }
    }

    private void HandleProcessFailed(int id, CoreWebView2ProcessFailedEventArgs args)
    {
        if (GetWebView(id) == null) return;
        Logger.Error($"WebView process failed for tab {id}: {args.Reason}");
        var wv = GetWebView(id); if (wv != null)
        {
            try
            {
                var crashUrl = _tabs.FirstOrDefault(t => t.Id == id)?.Url ?? "";
                var safeUrl = JsonSerializer.Serialize(crashUrl);
                var html = _crashPage.Value.Replace("{{URL}}", safeUrl);
                wv.CoreWebView2?.NavigateToString(html);
            }
            catch (Exception ex) { Logger.Error($"Crash page render failed: {ex.Message}"); }
        }
    }

    private void HandleNewWindowRequested(int id, CoreWebView2NewWindowRequestedEventArgs args)
    {
        if (GetWebView(id) == null) return;
        args.Handled = true;
        if (!string.IsNullOrEmpty(args.Uri)) _ = CreateTabInternalAsync(args.Uri, false);
    }

    private void HandleNavigationStarting(int id, CoreWebView2NavigationStartingEventArgs args)
    {
        var tab = _tabs.FirstOrDefault(t => t.Id == id);
        if (tab == null) return;

        // ??? SECURITY & STATE MANAGEMENT
        // If the user clicks a standard web link INSIDE an internal page (e.g., clicking "Chromium" in the About page),
        // we must downgrade the tab from "Internal" to "Standard Web" so it behaves normally.
        if (tab.IsInternalPage && !UrlResolver.IsInternalUrl(args.Uri) && !args.Uri.StartsWith("file:///"))
        {
            tab.IsInternalPage = false;
            _internalPageTabs.Remove(id);

            // Unregister internal IPC handlers since it's now a public web page
            if (_ipcHandlers.TryGetValue(id, out var handler))
            {
                var wv = GetWebView(id); if (wv != null)
                    wv.CoreWebView2.WebMessageReceived -= handler;
                _ipcHandlers.Remove(id);
            }
        }

        // Optional Security: Block users from manually typing file:/// paths in the Omnibox
        if (args.Uri.StartsWith("file:///") && !tab.IsInternalPage)
        {
            args.Cancel = true;
        }
    }

    private void HandleNavigationCompleted(int id)
    {
        if (GetWebView(id) == null) return;
        
        var tab = _tabs.FirstOrDefault(t => t.Id == id);
        if (tab != null && (GetWebView(id) is {} wv))
        {
            var oldTitle = tab.Title;
            var newTitle = wv.CoreWebView2?.DocumentTitle ?? tab.Title;
            if (oldTitle != newTitle)
            {
                tab.Title = newTitle;
                TabTitleChanged?.Invoke(this, new TabEventArgs { Id = id, Title = newTitle, Url = tab.Url });
            }
        }

        if (id == _activeId && !_disposed)
        {
            NavigationCompleted?.Invoke(this, EventArgs.Empty);
        }
        FireNavState(id);
    }

    private void HandleDocumentTitleChanged(int id, WebView2 webView)
    {
        if (GetWebView(id) == null) return;
        var tab = _tabs.FirstOrDefault(t => t.Id == id);
        if (tab != null && !tab.IsInternalPage)
        {
            var oldTitle = tab.Title;
            var newTitle = webView.CoreWebView2?.DocumentTitle ?? tab.Title;
            if (oldTitle != newTitle)
            {
                tab.Title = newTitle;
                tab.Url = webView.CoreWebView2?.Source?.ToString() ?? tab.Url;
                TabTitleChanged?.Invoke(this, new TabEventArgs { Id = id, Title = newTitle, Url = tab.Url });
                ScheduleSaveSession();
            }
        }
    }

    private void HandleSourceChanged(int id)
    {
        var wv = GetWebView(id);
        if (wv == null) return;
        var tab = _tabs.FirstOrDefault(t => t.Id == id);
        if (tab == null) return;

        var physicalUrl = wv.Source?.OriginalString ?? "";

        if (tab.IsInternalPage)
        {
            UrlChanged?.Invoke(this, new UrlEventArgs { Url = tab.Url });
        }
        else
        {
            tab.Url = physicalUrl;
            UrlChanged?.Invoke(this, new UrlEventArgs { Url = physicalUrl });
        }

        FireNavState(id);
    }

    private async Task HandleFaviconChanged(int id, WebView2 webView)
    {
        if (GetWebView(id) == null) return;

        // ??? Thread marshalling: BitmapImage is a DependencyObject, must be created on UI thread
        if (!_dispatcherQueue.HasThreadAccess)
        {
            _dispatcherQueue.TryEnqueue(async () => await HandleFaviconChanged(id, webView));
            return;
        }

        // Rate limit: Max 1 update per second per tab
        var now = Stopwatch.GetTimestamp();
        if (now - _lastFaviconTimestamp.GetValueOrDefault(id) < Stopwatch.Frequency) return;
        _lastFaviconTimestamp[id] = now;

        try
        {
            using var stream = await webView.CoreWebView2.GetFaviconAsync(CoreWebView2FaviconImageFormat.Png);
            if (stream == null) return;

            var bitmap = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
            await bitmap.SetSourceAsync(stream);

            FaviconUpdated?.Invoke(this, new FaviconEventArgs { TabId = id, Favicon = bitmap });
        }
        catch (Exception ex)
        {
            Logger.Warning($"Failed loading favicon for tab {id}: {ex.Message}");
        }
    }

    private void HandleContextMenuRequested(int id, CoreWebView2ContextMenuRequestedEventArgs args)
    {
        if (GetWebView(id) == null) return;

        args.Handled = true;

        var presenterStyle = (Style)Application.Current.Resources["TbMenuFlyoutPresenterStyle"];
        var itemStyle = (Style)Application.Current.Resources["TbMenuFlyoutItemStyle"];
        var flyout = new MenuFlyout { MenuFlyoutPresenterStyle = presenterStyle };

        var target = args.ContextMenuTarget;

        if (target.HasLinkUri)
        {
            var linkUri = target.LinkUri;
            flyout.Items.Add(new MenuFlyoutItem
            {
                Text = "Open link in new tab",
                Style = itemStyle,
                Command = new RelayCommand(() => _ = CreateTabAsync(linkUri))
            });
            flyout.Items.Add(new MenuFlyoutSeparator());
        }

        if (target.Kind == CoreWebView2ContextMenuTargetKind.Image)
        {
                var src = target.SourceUri;
            flyout.Items.Add(new MenuFlyoutItem
            {
                Text = "Copy image address",
                Style = itemStyle,
                Command = new RelayCommand(() =>
                {
                    if (!string.IsNullOrEmpty(src))
                    {
                        var pkg = new Windows.ApplicationModel.DataTransfer.DataPackage();
                        pkg.SetText(src);
                        Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(pkg);
                    }
                })
            });
            flyout.Items.Add(new MenuFlyoutSeparator());
        }

        flyout.Items.Add(new MenuFlyoutItem { Text = "Back", Command = new RelayCommand(Back), Style = itemStyle });
        flyout.Items.Add(new MenuFlyoutItem { Text = "Forward", Command = new RelayCommand(Forward), Style = itemStyle });
        flyout.Items.Add(new MenuFlyoutItem { Text = "Reload", Command = new RelayCommand(Reload), Style = itemStyle });
        flyout.Items.Add(new MenuFlyoutSeparator());
        flyout.Items.Add(new MenuFlyoutItem
        {
            Text = "Inspect Element",
            Style = itemStyle,
            Command = new RelayCommand(() =>
            {
                var wv = GetWebView(id); if (wv != null)
                    wv.CoreWebView2?.OpenDevToolsWindow();
            })
        });

        var wv = GetWebView(id); if (wv != null)
        {
            var pt = new Point(args.Location.X, args.Location.Y);
            flyout.ShowAt(wv, pt);
        }
    }

    internal void FireNavState(int id)
    {
        if (_disposed) return; var wv = GetWebView(id); if (wv == null) return;
        try
        {
            var core = wv.CoreWebView2;
            NavStateChanged?.Invoke(this, new NavStateEventArgs { CanGoBack = core?.CanGoBack ?? false, CanGoForward = core?.CanGoForward ?? false, Title = _tabs.FirstOrDefault(t => t.Id == id)?.Title ?? "" });
        }
        catch (Exception ex) { Logger.Warn("FireNavState failed", ex); }
    }
}

