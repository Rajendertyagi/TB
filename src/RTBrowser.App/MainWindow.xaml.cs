using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

using RTBrowser.Core;
using RTBrowser.Runtime;
using RTBrowser.Services;
using RTBrowser.UI.Controls;

using System;
using System.Windows;

namespace RTBrowser.App
{
    public partial class MainWindow : Window
    {
        private const string HomeUrl =
            "https://www.google.com";

        private readonly TabManager _tabManager =
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

        private async System.Threading.Tasks.Task CreateNewTab(
            string url)
        {
            WebView2 webView =
                new();

            await webView
                .EnsureCoreWebView2Async();

            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled =
                false;

            webView.NavigationStarting +=
                OnNavigationStarting;

            webView.NavigationCompleted +=
                OnNavigationCompleted;

            webView.CoreWebView2.DocumentTitleChanged +=
                OnDocumentTitleChanged;

            BrowserTab tab =
                _tabManager.CreateTab(
                    webView,
                    url);

            WebViewContainer.SetBrowser(
                webView);

            webView.CoreWebView2.Navigate(url);

            NavigationBar.SetAddress(url);

            LoggerService.Info(
                "Tabs",
                $"Created tab: {tab.Id}");
        }

        private BrowserTab? ActiveTab =>
            _tabManager.ActiveTab;

        private WebView2? ActiveWebView =>
            ActiveTab?.WebView;

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
            if (ActiveWebView?.CoreWebView2 == null)
            {
                return;
            }

            ActiveWebView
                .CoreWebView2
                .Navigate(url);

            NavigationBar.SetAddress(url);

            if (ActiveTab != null)
            {
                ActiveTab.Url = url;
            }

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
            if (ActiveWebView?.CoreWebView2 == null)
            {
                return;
            }

            string title =
                ActiveWebView
                    .CoreWebView2
                    .DocumentTitle;

            if (ActiveTab != null)
            {
                ActiveTab.Title = title;
            }

            Title =
                $"{title} - RTBrowser";

            LoggerService.Info(
                "Tabs",
                $"Title changed: {title}");
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
