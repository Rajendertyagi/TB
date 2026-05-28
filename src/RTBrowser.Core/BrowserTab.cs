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

        public bool IsActive { get; set; }
    }
}
