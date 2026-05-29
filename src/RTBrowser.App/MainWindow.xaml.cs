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
        }

        private async System.Threading.Tasks.Task CreateNewTab(
            string url)
        {
            try
            {
                LoggerService.Info("WEBVIEW","CreateNewTab ENTER");

                WebView2 webView = CreateWebView();

                LoggerService.Info("WEBVIEW","WebView created");

                CoreWebView2Environment environment =
                    await WebViewEnvironmentFactory.GetAsync();

                LoggerService.Info("WEBVIEW","Environment created");

                await webView.EnsureCoreWebView2Async(environment);

                LoggerService.Info("WEBVIEW","EnsureCoreWebView2Async SUCCESS");
            }
            catch (Exception ex)
            {
                LoggerService.Error("WEBVIEW", ex.ToString());
                MessageBox.Show(ex.ToString(), "RTBrowser Error");
            }
        }
    }
}
