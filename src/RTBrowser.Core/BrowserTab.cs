using Microsoft.Web.WebView2.Wpf;

using System;

namespace RTBrowser.Core
{
    public sealed class BrowserTab
    {
        public Guid Id { get; init; } =
            Guid.NewGuid();

        public string Title { get; set; } =
            "New Tab";

        public string Url { get; set; } =
            "about:blank";

        public bool IsActive { get; set; }

        public bool IsLoading { get; set; }

        public DateTime CreatedAt { get; init; } =
            DateTime.UtcNow;

        public WebView2 WebView { get; set; } =
            null!;
    }
}
