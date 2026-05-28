using Microsoft.Web.WebView2.Core;

using System;
using System.Windows;

namespace RTBrowser.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private async void OnLoaded(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                await BrowserView.EnsureCoreWebView2Async();

                BrowserView.CoreWebView2.Settings
                    .AreDefaultContextMenusEnabled = false;

                BrowserView.CoreWebView2.Settings
                    .AreDevToolsEnabled = true;

                BrowserView.CoreWebView2.Settings
                    .IsStatusBarEnabled = false;

                BrowserView.CoreWebView2.Navigate(
                    "https://www.google.com");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "RTBrowser Startup Error");
            }
        }
    }
}
