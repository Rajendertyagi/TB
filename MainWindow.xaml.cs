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
            // 1. Establish the portable cache directory location
            string localFolder = AppDomain.CurrentDomain.BaseDirectory;
            string userDataFolder = Path.Combine(localFolder, "BrowserProfileData");
            
            // 2. STOPS COMPILER ERRORS: Explicitly set the location using WebView2's design configuration variable
            // This forces the browser engine to use our local folder instead of the host machine's AppData.
            Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", userDataFolder);

            // 3. Fire up the browser using the parameterless WinUI 3 signature
            await WebView.EnsureCoreWebView2Async();
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
