using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RTBrowser.WebView
{
    public sealed class WebViewHost : Grid, IDisposable
    {
        private readonly WebView2 _webView;

        public CoreWebView2? Core =>
            _webView.CoreWebView2;

        public string CurrentUrl =>
            _webView.Source?.ToString() ?? "about:blank";

        public bool IsInitialized { get; private set; }

        public WebViewHost()
        {
            _webView = new WebView2();

            Children.Add(_webView);
        }

        public async Task InitializeAsync(
            string userDataFolder)
        {
            if (IsInitialized)
                return;

            var environment =
                await CoreWebView2Environment.CreateAsync(
                    null,
                    userDataFolder);

            await _webView.EnsureCoreWebView2Async(environment);

            ConfigureSettings();

            IsInitialized = true;
        }

        private void ConfigureSettings()
        {
            if (_webView.CoreWebView2 == null)
                return;

            var settings = _webView.CoreWebView2.Settings;

            settings.IsStatusBarEnabled = false;
            settings.AreDefaultContextMenusEnabled = true;
            settings.AreDevToolsEnabled = true;
            settings.IsZoomControlEnabled = true;
            settings.IsGeneralAutofillEnabled = false;
            settings.IsPasswordAutosaveEnabled = false;
            settings.AreBrowserAcceleratorKeysEnabled = true;
        }

        public void Navigate(string url)
        {
            if (!IsInitialized)
                return;

            if (string.IsNullOrWhiteSpace(url))
                return;

            _webView.CoreWebView2.Navigate(url);
        }

        public void Reload()
        {
            if (!IsInitialized)
                return;

            _webView.Reload();
        }

        public void GoBack()
        {
            if (!IsInitialized)
                return;

            if (_webView.CanGoBack)
            {
                _webView.GoBack();
            }
        }

        public void GoForward()
        {
            if (!IsInitialized)
                return;

            if (_webView.CanGoForward)
            {
                _webView.GoForward();
            }
        }

        public void Stop()
        {
            if (!IsInitialized)
                return;

            _webView.CoreWebView2.Stop();
        }

        public void Dispose()
        {
            _webView.Dispose();
        }
    }
}
