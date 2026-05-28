using Microsoft.Web.WebView2.Core;

using RTBrowser.Services;

using System;
using System.Windows;
using System.Windows.Input;

namespace RTBrowser.App
{
    public partial class MainWindow : Window
    {
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

                NavigateTo("https://www.google.com");
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
                    $"Navigating to {url}");

                BrowserView.CoreWebView2.Navigate(url);
            }
            catch (Exception ex)
            {
                LoggerService.Error(
                    "Navigation",
                    ex.ToString());
            }
        }

        private void OnNavigationStarting(
            object? sender,
            CoreWebView2NavigationStartingEventArgs e)
        {
            LoggerService.Info(
                "Navigation",
                $"Navigation started: {e.Uri}");
        }

        private void OnNavigationCompleted(
            object? sender,
            CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                LoggerService.Info(
                    "Navigation",
                    $"Navigation completed: {BrowserView.Source}");
            }
            else
            {
                LoggerService.Error(
                    "Navigation",
                    $"Navigation failed: {e.WebErrorStatus}");
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
