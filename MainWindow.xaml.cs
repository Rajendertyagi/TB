using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System;
using System.IO;

namespace MyPortableBrowser
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            InitializeBrowser();
        }

        private async void InitializeBrowser()
        {
            // Portable isolation strategy: Save cookies and cache relative to the executable directory,
            // rather than burying data inside the host computer's C:\Users\AppData directory.
            string localFolder = AppDomain.CurrentDomain.BaseDirectory;
            string userDataFolder = Path.Combine(localFolder, "BrowserProfileData");
            
            var environment = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await WebView.EnsureCoreWebView2Async(environment);
        }

        private void GoButton_Click(object sender, RoutedEventArgs e) => NavigateToUrl();
        private void UrlBar_KeyDown(object sender, KeyRoutedEventArgs e) { if (e.Key == Windows.System.VirtualKey.Enter) NavigateToUrl(); }

        private void NavigateToUrl()
        {
            string rawUrl = UrlBar.Text.Trim();
            if (string.IsNullOrEmpty(rawUrl)) return;

            if (!rawUrl.StartsWith("http://") && !rawUrl.StartsWith("https://"))
            {
                if (rawUrl.Contains(".") && !rawUrl.Contains(" "))
                    rawUrl = "https://" + rawUrl;
                else
                    rawUrl = "https://www.google.com/search?q=" + Uri.EscapeDataString(rawUrl);
            }
            WebView.Source = new Uri(rawUrl);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) { if (WebView.CanGoBack) WebView.GoBack(); }
        private void ForwardButton_Click(object sender, RoutedEventArgs e) { if (WebView.CanGoForward) WebView.GoForward(); }
        private void ReloadButton_Click(object sender, RoutedEventArgs e) => WebView.Reload();
    }
}
