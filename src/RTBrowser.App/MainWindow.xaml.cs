using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

using RTBrowser.Core;
using RTBrowser.Runtime;
using RTBrowser.Services;
using RTBrowser.UI.Controls;

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace RTBrowser.App
{
    public partial class MainWindow : Window
    {
        private readonly BrowserSessionManager _sessionManager =
            new();

        public MainWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            Closed += OnClosed;

            PreviewKeyDown +=
                OnPreviewKeyDown;

            NavigationBar.NavigateRequested +=
                OnNavigateRequested;

            NavigationBar.BackRequested +=
                OnBackRequested;

            NavigationBar.ForwardRequested +=
                OnForwardRequested;

            NavigationBar.RefreshRequested +=
                OnRefreshRequested;

            BrowserTitleBar.NewTabRequested +=
                OnNewTabRequested;

            BrowserTitleBar.CloseTabRequested +=
                OnCloseTabRequested;

            BrowserTitleBar.TabSelected +=
                OnTabSelected;
        }

        private async void OnLoaded(
            object sender,
            RoutedEventArgs e)
        {
            RestoreWindowState();

            LoggerService.Info(
                "Window",
                "Main window loaded");

            var persistedTabs =
                TabPersistenceService.Load();

            if (persistedTabs.Count == 0)
            {
                await CreateNewTab(
                    Constants.HomeUrl);

                return;
            }

            foreach (var tab in persistedTabs)
            {
                await CreateNewTab(
                    tab.Url);
            }

            var activeTab =
                persistedTabs
                    .FirstOrDefault(
                        x => x.IsActive);

            if (activeTab != null)
            {
                var session =
                    _sessionManager
                        .Sessions
                        .FirstOrDefault(
                            x => x.Tab.Url == activeTab.Url);

                if (session != null)
                {
                    _sessionManager.SetActiveSession(
                        session.Id);

                    SyncActiveSession();
                }
            }
        }

        private async void OnNewTabRequested()
        {
            await CreateNewTab(
                Constants.HomeUrl);

            LoggerService.Info(
                "Tabs",
                "New tab requested");
        }

        private void OnTabSelected(
            Guid tabId)
        {
            _sessionManager.SetActiveSession(
                tabId);

            if (_sessionManager.ActiveSession == null)
            {
                return;
            }

            SyncActiveSession();

            LoggerService.Info(
                "Tabs",
                $"Activated tab: {tabId}");
        }

        private void OnCloseTabRequested(
            Guid tabId)
        {
            _sessionManager.CloseSession(
                tabId);

            if (!_sessionManager.HasSessions)
            {
                Close();

                return;
            }

            SyncActiveSession();

            LoggerService.Info(
                "Tabs",
                $"Closed tab: {tabId}");
        }

        private async System.Threading.Tasks.Task CreateNewTab(
            string url)
        {
            WebView2 webView =
                CreateWebView();

            CoreWebView2Environment environment =
                await WebViewEnvironmentFactory
                    .GetAsync();

            await webView.EnsureCoreWebView2Async(
                environment);

            ConfigureWebView(
                webView);

            BrowserTab tab =
                new()
                {
                    Title = "New Tab",
                    Url = url,
                    IsActive = true
                };

            TabSession session =
                _sessionManager.CreateSession(
                    tab,
                    webView);

            session.Navigate(url);

            SyncActiveSession();

            LoggerService.Info(
                "Tabs",
                $"Created tab: {tab.Id}");
        }

        private WebView2 CreateWebView()
        {
            return
                new WebView2
                {
                    HorizontalAlignment =
                        HorizontalAlignment.Stretch,

                    VerticalAlignment =
                        VerticalAlignment.Stretch,

                    Focusable = true,

                    DefaultBackgroundColor =
                        System.Drawing.Color.Transparent
                };
        }

        private void ConfigureWebView(
            WebView2 webView)
        {
            if (webView.CoreWebView2 == null)
            {
                return;
            }

            CoreWebView2Settings settings =
                webView.CoreWebView2.Settings;

            settings.AreDefaultContextMenusEnabled =
                false;

            settings.AreDevToolsEnabled =
                false;

            settings.IsStatusBarEnabled =
                false;

            settings.AreBrowserAcceleratorKeysEnabled =
                true;

            settings.IsZoomControlEnabled =
                true;

            webView.NavigationStarting +=
                OnNavigationStarting;

            webView.NavigationCompleted +=
                OnNavigationCompleted;

            webView.CoreWebView2.DocumentTitleChanged +=
                OnDocumentTitleChanged;
        }

        private void SyncActiveSession()
        {
            if (ActiveSession == null)
            {
                return;
            }

            WebViewContainer.SetBrowser(
                ActiveSession.WebView);

            NavigationBar.SetAddress(
                ActiveSession.Tab.Url);

            Title =
                $"{ActiveSession.Tab.Title} - {Constants.BrowserName}";

            RenderTabs();
        }

        private void RenderTabs()
        {
            BrowserTitleBar.RenderTabs(
                _sessionManager
                    .Sessions
                    .Select(x => x.Tab)
                    .ToList());
        }

        private TabSession? ActiveSession =>
            _sessionManager.ActiveSession;

        private void OnNavigateRequested(
            string input)
        {
            string url =
                UrlHelper.Normalize(input);

            if (ActiveSession == null)
            {
                return;
            }

            ActiveSession.Navigate(url);

            NavigationBar.SetAddress(url);

            LoggerService.Info(
                "Navigation",
                $"Navigate: {url}");
        }

        private void OnBackRequested()
        {
            ActiveSession?.GoBack();

            LoggerService.Info(
                "Navigation",
                "Back pressed");
        }

        private void OnForwardRequested()
        {
            ActiveSession?.GoForward();

            LoggerService.Info(
                "Navigation",
                "Forward pressed");
        }

        private void OnRefreshRequested()
        {
            ActiveSession?.Reload();

            LoggerService.Info(
                "Navigation",
                "Refresh pressed");
        }

        private void OnPreviewKeyDown(
            object sender,
            KeyEventArgs e)
        {
            BrowserKeyboardShortcuts.Handle(
                this,
                e,
                _sessionManager,
                () => OnNewTabRequested(),
                () =>
                {
                    if (ActiveSession == null)
                    {
                        return;
                    }

                    OnCloseTabRequested(
                        ActiveSession.Id);
                },
                () =>
                {
                    NavigationBar.FocusAddressBar();
                },
                () =>
                {
                    OnRefreshRequested();
                });
        }

        private void OnNavigationStarting(
            object? sender,
            CoreWebView2NavigationStartingEventArgs e)
        {
            TabSession? session =
                FindSession(sender);

            if (session == null)
            {
                return;
            }

            session.SetLoading(true);

            session.Tab.Url =
                e.Uri;

            if (session.Tab.IsActive)
            {
                NavigationBar.SetAddress(
                    e.Uri);

                StatusBar.SetStatus(
                    "Loading...");
            }

            RenderTabs();

            LoggerService.Info(
                "Navigation",
                $"Navigation started: {e.Uri}");
        }

        private void OnNavigationCompleted(
            object? sender,
            CoreWebView2NavigationCompletedEventArgs e)
        {
            TabSession? session =
                FindSession(sender);

            if (session == null)
            {
                return;
            }

            session.SetLoading(false);

            if (session.Tab.IsActive)
            {
                StatusBar.SetStatus(
                    e.IsSuccess
                        ? "Ready"
                        : "Navigation Failed");
            }

            RenderTabs();

            LoggerService.Info(
                "Navigation",
                e.IsSuccess
                    ? "Navigation completed"
                    : $"Navigation failed: {e.WebErrorStatus}");
        }

        private void OnDocumentTitleChanged(
            object? sender,
            object e)
        {
            if (sender is not CoreWebView2 coreWebView)
            {
                return;
            }

            TabSession? session =
                _sessionManager
                    .Sessions
                    .FirstOrDefault(
                        x => x.WebView.CoreWebView2 == coreWebView);

            if (session == null)
            {
                return;
            }

            session.UpdateTitle(
                coreWebView.DocumentTitle);

            if (session.Tab.IsActive)
            {
                Title =
                    $"{session.Tab.Title} - {Constants.BrowserName}";
            }

            RenderTabs();

            LoggerService.Info(
                "Tabs",
                $"Title changed: {session.Tab.Title}");
        }

        private TabSession? FindSession(
            object? sender)
        {
            if (sender is not WebView2 webView)
            {
                return null;
            }

            return
                _sessionManager
                    .Sessions
                    .FirstOrDefault(
                        x => x.WebView == webView);
        }

        private void RestoreWindowState()
        {
            RTBrowser.Core.WindowStateModel state =
                WindowStateService.Load();

            Width =
                state.Width;

            Height =
                state.Height;

            Left =
                state.Left;

            Top =
                state.Top;

            WindowState =
                state.IsMaximized
                    ? WindowState.Maximized
                    : WindowState.Normal;

            LoggerService.Info(
                "Window",
                "Window state restored");
        }

        private void SaveWindowState()
        {
            RTBrowser.Core.WindowStateModel state =
                new()
                {
                    Width = Width,
                    Height = Height,
                    Left = Left,
                    Top = Top,
                    IsMaximized =
                        WindowState ==
                        WindowState.Maximized
                };

            WindowStateService.Save(
                state);

            LoggerService.Info(
                "Window",
                "Window state saved");
        }

        private void OnClosed(
            object? sender,
            EventArgs e)
        {
            TabPersistenceService.Save(
                _sessionManager.Sessions);

            foreach (TabSession session in _sessionManager.Sessions)
            {
                session.Dispose();
            }

            SaveWindowState();

            LoggerService.Info(
                "Window",
                "Main window closed");
        }
    }
}
