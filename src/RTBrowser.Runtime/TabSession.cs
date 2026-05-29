using Microsoft.Web.WebView2.Wpf;

using RTBrowser.Core;

using System;

namespace RTBrowser.Runtime
{
    public sealed class TabSession
        : IDisposable
    {
        public BrowserTab Tab { get; }

        public Guid Id =>
            Tab.Id;

        public WebView2 WebView { get; }

        public TabSession(
            BrowserTab tab,
            WebView2 webView)
        {
            Tab =
                tab;

            WebView =
                webView;
        }

        public void Activate()
        {
            Tab.IsActive = true;
        }

        public void Deactivate()
        {
            Tab.IsActive = false;
        }

        public void SetLoading(
            bool loading)
        {
            Tab.IsLoading =
                loading;
        }

        public void UpdateTitle(
            string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return;
            }

            Tab.Title =
                title;
        }

        public void Navigate(
            string url)
        {
            if (WebView.CoreWebView2 == null)
            {
                return;
            }

            Tab.Url =
                url;

            WebView.CoreWebView2.Navigate(
                url);
        }

        public void Reload()
        {
            WebView.Reload();
        }

        public void GoBack()
        {
            if (WebView.CanGoBack)
            {
                WebView.GoBack();
            }
        }

        public void GoForward()
        {
            if (WebView.CanGoForward)
            {
                WebView.GoForward();
            }
        }

        public void Dispose()
        {
            try
            {
                WebView.Dispose();
            }
            catch
            {
            }
        }
    }
}
