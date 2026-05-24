using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace TradingBrowser
{
    public static class NavigationController
    {
        public static void ProcessOmniboxQuery(AutoSuggestBox sender, WebView2 activeBrowser)
        {
            if (activeBrowser == null) return;

            string rawInput = sender.Text.Trim();
            if (string.IsNullOrEmpty(rawInput)) return;

            string targetUrl;

            // Check if input looks like a valid web address or needs a search fallback
            if (rawInput.StartsWith("http://") || 
                rawInput.StartsWith("https://") || 
                (rawInput.Contains(".") && !rawInput.Contains(" ")))
            {
                targetUrl = rawInput.StartsWith("http") ? rawInput : "https://" + rawInput;
            }
            else
            {
                targetUrl = "https://www.google.com/search?q=" + Uri.EscapeDataString(rawInput);
            }

            try
            {
                activeBrowser.Source = new Uri(targetUrl);
            }
            catch (Exception ex)
            {
                Logger.WriteLog("Global Errors", $"URL Generation Error: {ex.Message}");
            }
        }
    }
}
