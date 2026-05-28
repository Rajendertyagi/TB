using Microsoft.Web.WebView2.Wpf;

using System;

namespace RTBrowser.Core
{
    public sealed class BrowserTab
    {
        public Guid Id { get; } =
            Guid.NewGuid();

        public string Title { get; set; } =
            "New Tab";

        public string Url { get; set; } =
            "about:blank";

        public WebView2? WebView { get; set; }

        public bool IsActive { get; set; }
    }
}
