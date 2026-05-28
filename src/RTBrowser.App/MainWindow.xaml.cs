using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

using RTBrowser.Core;
using RTBrowser.Services;
using RTBrowser.UI.Controls;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace RTBrowser.App
{
    public partial class MainWindow : Window
    {
        private const string HomeUrl =
            "https://www.google.com";

        private readonly List<BrowserTab> _tabs =
            new();

        private readonly Dictionary<Guid, WebView2> _webViews =
            new();

        private BrowserTab? _activeTab;

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

        private async Task CreateNewTab(
            string url)
        {
            if (WebViewContainer.Browser.CoreWebView2 == null)
            {
                await WebViewContainer.Browser
                    .EnsureCoreWebView2Async();
            }

            WebViewContainer.Browser
                .CoreWebView2
                .NavigationStarting +=
                OnNavigationStarting;

            WebViewContainer.Browser
                .CoreWebView2
                .NavigationCompleted +=
                OnNavigationCompleted;

            WebViewContainer.Browser
                .CoreWebView2
                .DocumentTitleChanged +=
                OnDocumentTitleChanged;

            var tab = new BrowserTab
            {
                Url = url,
                Title = "New Tab",
                IsActive = true
            };

            foreach (var existingTab in _tabs)
            {
                existingTab.IsActive = false;
            }

            _tabs.Add(tab);

            _webViews[tab.Id] =
                WebViewContainer.Browser;

            _activeTab = tab;

            NavigateTo(url);

            LoggerService.Info(
                "Tabs",
                $"Created tab: {tab.Id}");
        }

        private WebView2? ActiveWebView
        {
            get
            {
                if (_activeTab == null)
                {
                    return null;
                }

                return _webViews[_activeTab.Id];
            }
        }

        private void OnNavigateRequested(
            string input)
        {
            string url =
                NormalizeInput(input);

            NavigateTo(url);
        }

        private void OnBackRequested()
        {
            if (ActiveWebView?.CoreWebView2?.CanGoBack == true)
            {
                ActiveWebView.CoreWebView2.GoBack();

                LoggerService.Info(
                    "Navigation",
                    "Back pressed");
            }
        }

        private void OnForwardRequested()
        {
            if (ActiveWebView?.CoreWebView2?.CanGoForward == true)
            {
                ActiveWebView.CoreWebView2.GoForward();

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

            LoggerService.Info(
                "Navigation",
                $"Navigate: {url}");
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
                    input = "https://" + input;
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

            if (_activeTab != null)
            {
                _activeTab.Title = title;
            }

            Title = $"{title} - RTBrowser";

            LoggerService.Info(
                "Tabs",
                $"Title changed: {title}");
        }

        private void RestoreWindowState()
        {
            var state =
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
            var state =
                new WindowStateModel
                {
                    Width = Width,
                    Height = Height,
                    Left = Left,
                    Top = Top,
                    IsMaximized =
                        WindowState == WindowState.Maximized
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
