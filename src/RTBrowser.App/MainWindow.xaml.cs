using Microsoft.Web.WebView2.Core;

using RTBrowser.Services;

using System;
using System.Windows;
using System.Windows.Input;

namespace RTBrowser.App
{
    public partial class MainWindow : Window
    {
        private const string HomeUrl =
            "https://www.google.com";

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
            try
            {
                LoggerService.Info(
                    "Window",
                    "Main window loaded");

                await BrowserView.EnsureCoreWebView2Async();

                BrowserView.CoreWebView2.Settings
                    .AreDefaultContextMenusEnabled = false;

                BrowserView.CoreWebView2.Settings
                    .AreDevToolsEnabled = true;

                BrowserView.CoreWebView2.Settings
                    .IsStatusBarEnabled = false;

                BrowserView.CoreWebView2.NavigationStarting +=
                    OnNavigationStarting;

                BrowserView.CoreWebView2.NavigationCompleted +=
                    OnNavigationCompleted;

                BrowserView.CoreWebView2.DocumentTitleChanged +=
                    OnDocumentTitleChanged;

                NavigateTo(HomeUrl);
            }
            catch (Exception ex)
            {
                LoggerService.Error(
                    "Startup",
                    ex.ToString());

                MessageBox.Show(
                    ex.ToString(),
                    "RTBrowser Startup Error");
            }
        }

        private void NavigateTo(
            string url)
        {
            try
            {
                LoggerService.Info(
                    "Navigation",
                    $"Navigate: {url}");

                AddressBar.Text = url;

                BrowserView.CoreWebView2.Navigate(url);

                StatusText.Text =
                    $"Loading {url}";
            }
            catch (Exception ex)
            {
                LoggerService.Error(
                    "Navigation",
                    ex.ToString());
            }
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

            string url =
                NormalizeInput(AddressBar.Text);

            NavigateTo(url);
        }

        private void OnNavigationStarting(
            object? sender,
            CoreWebView2NavigationStartingEventArgs e)
        {
            LoadingBar.Visibility =
                Visibility.Visible;

            StatusText.Text =
                "Loading...";

            LoggerService.Info(
                "Navigation",
                $"Started: {e.Uri}");
        }

        private void OnNavigationCompleted(
            object? sender,
            CoreWebView2NavigationCompletedEventArgs e)
        {
            LoadingBar.Visibility =
                Visibility.Collapsed;

            if (BrowserView.Source != null)
            {
                AddressBar.Text =
                    BrowserView.Source.ToString();
            }

            if (e.IsSuccess)
            {
                StatusText.Text =
                    "Ready";

                LoggerService.Info(
                    "Navigation",
                    $"Completed: {BrowserView.Source}");
            }
            else
            {
                StatusText.Text =
                    "Navigation failed";

                LoggerService.Error(
                    "Navigation",
                    e.WebErrorStatus.ToString());
            }
        }

        private void OnDocumentTitleChanged(
            object? sender,
            object e)
        {
            try
            {
                string title =
                    BrowserView.CoreWebView2.DocumentTitle;

                PageTitleText.Text = title;

                Title = $"{title} - RTBrowser";
            }
            catch
            {
            }
        }

        private void OnBackClicked(
            object sender,
            RoutedEventArgs e)
        {
            LoggerService.Info(
                "UI",
                "Back pressed");

            if (BrowserView.CoreWebView2.CanGoBack)
            {
                BrowserView.CoreWebView2.GoBack();
            }
        }

        private void OnForwardClicked(
            object sender,
            RoutedEventArgs e)
        {
            LoggerService.Info(
                "UI",
                "Forward pressed");

            if (BrowserView.CoreWebView2.CanGoForward)
            {
                BrowserView.CoreWebView2.GoForward();
            }
        }

        private void OnRefreshClicked(
            object sender,
            RoutedEventArgs e)
        {
            LoggerService.Info(
                "UI",
                "Refresh pressed");

            BrowserView.CoreWebView2.Reload();
        }

        private void OnHomeClicked(
            object sender,
            RoutedEventArgs e)
        {
            LoggerService.Info(
                "UI",
                "Home pressed");

            NavigateTo(HomeUrl);
        }

        private void OnStopClicked(
            object sender,
            RoutedEventArgs e)
        {
            LoggerService.Info(
                "UI",
                "Stop pressed");

            BrowserView.CoreWebView2.Stop();
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
            LoggerService.Info(
                "Window",
                "Main window closed");
        }
    }
}
