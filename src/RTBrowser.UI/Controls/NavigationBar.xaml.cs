using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

using RTBrowser.Core;
using RTBrowser.Runtime;
using RTBrowser.Services;
using RTBrowser.UI.Controls;

using System;
using System.Linq;
using System.Windows;

namespace RTBrowser.App
{
    public partial class MainWindow : Window
    {
        private const string HomeUrl =
            "https://www.google.com";

        private readonly BrowserSessionManager _sessionManager =
            new();

        public MainWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            Closed += OnClosed;

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

            await CreateNewTab(HomeUrl);
        }

        private async void OnNewTabRequested()
        {
            await CreateNewTab(HomeUrl);

            LoggerService.Info(
                "Tabs",
                "New tab requested");
        }

        private void OnTabSelected(
            Guid tabId)
        {
            _sessionManager.SetActiveSession(tabId);

            if (_sessionManager.ActiveSession == null)
            {
                return;
            }

            WebViewContainer.SetBrowser(
                _sessionManager
                    .ActiveSession
                    .WebView);

            NavigationBar.SetAddress(
                _sessionManager
                    .ActiveSession
                    .Tab
                    .Url);

            RenderTabs();

            LoggerService.Info(
                "Tabs",
                $"Activated tab: {tabId}");
        }

        private void OnCloseTabRequested(
            Guid tabId)
        {
            _sessionManager.CloseSession(tabId);

            if (_sessionManager.ActiveSession == null)
            {
                Close();

                return;
            }

            WebViewContainer.SetBrowser(
                _sessionManager
                    .ActiveSession
                    .WebView);

            NavigationBar.SetAddress(
                _sessionManager
                    .ActiveSession
                    .Tab
                    .Url);

            RenderTabs();

            LoggerService.Info(
                "Tabs",
                $"Closed tab: {tabId}");
        }

        private async System.Threading.Tasks.Task CreateNewTab(
            string url)
        {
            WebView2 webView =
                new()
                {
                    HorizontalAlignment =
                        HorizontalAlignment.Stretch,

                    VerticalAlignment =
                        VerticalAlignment.Stretch
                };

            await webView
                .EnsureCoreWebView2Async();

            ConfigureWebView(webView);

            BrowserTab tab =
                new()
                {
                    Title = "New Tab",
                    Url = url,
                    IsActive = true,
                    WebView = webView
                };

            TabSession session =
                new(tab);

            _sessionManager.AddSession(
                session);

            WebViewContainer.SetBrowser(
                webView);

            session.Navigate(url);

            NavigationBar.SetAddress(url);

            RenderTabs();

            LoggerService.Info(
                "Tabs",
                $"Created tab: {tab.Id}");
        }

        private void ConfigureWebView(
            WebView2 webView)
        {
            if (webView.CoreWebView2 == null)
            {
                return;
            }

            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled =
                false;

            webView.CoreWebView2.Settings.AreDevToolsEnabled =
                false;

            webView.CoreWebView2.Settings.IsStatusBarEnabled =
                false;

            webView.NavigationStarting +=
                OnNavigationStarting;

            webView.NavigationCompleted +=
                OnNavigationCompleted;

            webView.CoreWebView2.DocumentTitleChanged +=
                OnDocumentTitleChanged;
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

        private WebView2? ActiveWebView =>
            ActiveSession?.WebView;

        private void OnNavigateRequested(
            string input)
        {
            string url =
                NormalizeInput(input);

            NavigateTo(url);
        }

        private void NavigateTo(
            string url)
        {
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
            if (ActiveWebView?.CoreWebView2?.CanGoBack == true)
            {
                ActiveWebView
                    .CoreWebView2
                    .GoBack();

                LoggerService.Info(
                    "Navigation",
                    "Back pressed");
            }
        }

        private void OnForwardRequested()
        {
            if (ActiveWebView?.CoreWebView2?.CanGoForward == true)
            {
                ActiveWebView
                    .CoreWebView2
                    .GoForward();

                LoggerService.Info(
                    "Navigation",
                    "Forward pressed");
            }
        }

        private void OnRefreshRequested()
        {
            ActiveWebView?.Reload();

            LoggerService.Info(
                "Navigation",
                "Refresh pressed");
        }

        private string NormalizeInput(
            string input)
        {
            input = input.Trim();

            bool looksLikeUrl =
                input.Contains('.')
                && !input.Contains(' ');

            if (looksLikeUrl)
            {
                if (!input.StartsWith("http://")
                    && !input.StartsWith("https://"))
                {
                    input =
                        "https://" + input;
                }

                return input;
            }

            return
                "https://www.google.com/search?q="
                + Uri.EscapeDataString(input);
        }

        private void OnNavigationStarting(
            object? sender,
            CoreWebView2NavigationStartingEventArgs e)
        {
            NavigationBar.SetAddress(e.Uri);

            LoggerService.Info(
                "Navigation",
                $"Navigation started: {e.Uri}");
        }

        private void OnNavigationCompleted(
            object? sender,
            CoreWebView2NavigationCompletedEventArgs e)
        {
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

            session.Tab.Title =
                coreWebView.DocumentTitle;

            if (session.Tab.IsActive)
            {
                Title =
                    $"{session.Tab.Title} - RTBrowser";
            }

            RenderTabs();

            LoggerService.Info(
                "Tabs",
                $"Title changed: {session.Tab.Title}");
        }

        private void RestoreWindowState()
        {
            WindowStateModel state =
                WindowStateService.Load();

            Width = state.Width;
            Height = state.Height;
            Left = state.Left;
            Top = state.Top;

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
            WindowStateModel state =
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

            WindowStateService.Save(state);

            LoggerService.Info(
                "Window",
                "Window state saved");
        }

        private void OnClosed(
            object? sender,
            EventArgs e)
        {
            SaveWindowState();

            LoggerService.Info(
                "Window",
                "Main window closed");
        }
    }
}
