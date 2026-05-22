using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Web.WebView2.Wpf;

namespace MinimalBrowser
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeWebView();
            
            BackButton.Click += (s, e) => { if (webView.CanGoBack) webView.GoBack(); };
            ForwardButton.Click += (s, e) => { if (webView.CanGoForward) webView.GoForward(); };
            RefreshButton.Click += (s, e) => webView.Reload();
        }

        private async void InitializeWebView()
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.Navigate("https://www.google.com");
            
            webView.CoreWebView2.NavigationCompleted += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    AddressBar.Text = webView.Source?.ToString() ?? "";
                });
            };
        }

        private void AddressBar_TextChanged(object sender, System.EventArgs e)
        {
        }

        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string url = AddressBar.Text.Trim();
                if (!string.IsNullOrEmpty(url))
                {
                    if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                        url = "https://" + url;
                    
                    webView.CoreWebView2?.Navigate(url);
                }
            }
        }
    }
}
