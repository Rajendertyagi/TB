using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

using RTBrowser.Core;
using RTBrowser.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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

        private async Task CreateNewTab(
            string url)
        {
            if (BrowserHost.CoreWebView2 == null)
            {
                await BrowserHost.EnsureCoreWebView2Async();
            }

            BrowserHost.CoreWebView2.Settings
                .AreDefaultContextMenusEnabled = false;

            BrowserHost.CoreWebView2.NavigationStarting +=
                OnNavigationStarting;

            BrowserHost.CoreWebView2.NavigationCompleted +=
                OnNavigationCompleted;

            BrowserHost.CoreWebView2.DocumentTitleChanged +=
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

            _webViews[tab.Id] = BrowserHost;

            _activeTab = tab;

            NavigateTo(url);

            LoggerService.Info(
                "Tabs",
                $"Tab created: {tab.Id}");
        }

        private WebView2? ActiveWebView
        {
            get
            {
                if (_activeTab == null)
                    return null;

                return _webViews[_activeTab.Id];
            }
        }

        private void NavigateTo(
            string url)
        {
            if (ActiveWebView?.CoreWebView2 == null)
                return;

            AddressBar.Text = url;

            ActiveWebView.CoreWebView2.Navigate(url);

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

        private void OnAddressBarKeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            NavigateTo(
                NormalizeInput(AddressBar.Text));
        }

        private void OnNavigationStarting(
            object? sender,
            CoreWebView2NavigationStartingEventArgs e)
        {
            LoadingBar.Visibility =
                Visibility.Visible;

            StatusText.Text =
                "Loading...";
        }

        private void OnNavigationCompleted(
            object? sender,
            CoreWebView2NavigationCompletedEventArgs e)
        {
            LoadingBar.Visibility =
                Visibility.Collapsed;

            if (ActiveWebView?.Source != null)
            {
                AddressBar.Text =
                    ActiveWebView.Source.ToString();
            }

            StatusText.Text =
                e.IsSuccess
                ? "Ready"
                : "Navigation failed";
        }

        private void OnDocumentTitleChanged(
            object? sender,
            object e)
        {
            if (ActiveWebView?.CoreWebView2 == null)
                return;

            string title =
                ActiveWebView.CoreWebView2.DocumentTitle;

            _activeTab!.Title = title;

            PageTitleText.Text = title;

            Title = $"{title} - RTBrowser";
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
        }

        private void OnBackClicked(
            object sender,
            RoutedEventArgs e)
        {
            if (ActiveWebView?.CoreWebView2?.CanGoBack == true)
            {
                ActiveWebView.CoreWebView2.GoBack();
            }
        }

        private void OnForwardClicked(
            object sender,
            RoutedEventArgs e)
        {
            if (ActiveWebView?.CoreWebView2?.CanGoForward == true)
            {
                ActiveWebView.CoreWebView2.GoForward();
            }
        }

        private void OnRefreshClicked(
            object sender,
            RoutedEventArgs e)
        {
            ActiveWebView?.CoreWebView2?.Reload();
        }

        private void OnHomeClicked(
            object sender,
            RoutedEventArgs e)
        {
            NavigateTo(HomeUrl);
        }

        private void OnStopClicked(
            object sender,
            RoutedEventArgs e)
        {
            ActiveWebView?.CoreWebView2?.Stop();
        }

        private async void OnAddTabClicked(
            object sender,
            RoutedEventArgs e)
        {
            await CreateNewTab(HomeUrl);
        }

        private void OnMinimizeClicked(
            object sender,
            RoutedEventArgs e)
        {
            WindowState =
                WindowState.Minimized;
        }

        private void OnMaximizeClicked(
            object sender,
            RoutedEventArgs e)
        {
            WindowState =
                WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void OnCloseClicked(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }

        private void OnWindowDrag(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.LeftButton ==
                MouseButtonState.Pressed)
            {
                DragMove();
            }
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
