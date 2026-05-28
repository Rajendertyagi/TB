using Microsoft.Web.WebView2.Wpf;

using RTBrowser.Core;

using System;

namespace RTBrowser.Runtime
{
    public sealed class TabSession
    {
        public Guid Id =>
            Tab.Id;

        public BrowserTab Tab { get; }

        public WebView2 WebView =>
            Tab.WebView;

        public DateTime LastActivatedAt { get; private set; } =
            DateTime.UtcNow;

        public TabSession(
            BrowserTab tab)
        {
            Tab = tab;
        }

        public void Activate()
        {
            LastActivatedAt =
                DateTime.UtcNow;

            Tab.IsActive = true;
        }

        public void Deactivate()
        {
            Tab.IsActive = false;
        }

        public void Navigate(
            string url)
        {
            Tab.Url = url;

            if (WebView.CoreWebView2 != null)
            {
                WebView.CoreWebView2.Navigate(url);
            }
        }

        public void Dispose()
        {
            WebView.Dispose();
        }
    }
}
