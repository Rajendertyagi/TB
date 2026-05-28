using System;

namespace RTBrowser.Core
{
    public sealed class BrowserTabEventArgs
        : EventArgs
    {
        public BrowserTabEventArgs(
            BrowserTab tab)
        {
            Tab = tab;
        }

        public BrowserTab Tab { get; }
    }
}
