using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Web.WebView2.Core;

namespace TradingBrowser
{
    public partial class TabContext : ObservableObject
    {
        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private string _currentUrl;

        [ObservableProperty]
        private bool _isLoading = false;

        // Tracks the raw running rendering element for this explicit tab context
        public Microsoft.UI.Xaml.Controls.WebView2 BrowserInstance { get; set; }

        public TabContext(string initialTitle, string initialUrl)
        {
            _title = initialTitle;
            _currentUrl = initialUrl;
        }
    }
}
