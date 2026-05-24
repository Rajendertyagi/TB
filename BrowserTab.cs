using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.UI2.Core;

namespace TB
{
    public class BrowserTab : ObservableObject
    {
        private string _title;
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        
        public string Url { get; set; }
        
        public WebView2 WebView { get; } = new WebView2();
        
        public IconSource Icon => new SymbolIconSource { Symbol = Symbol.World };
    }
}
