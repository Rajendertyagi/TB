using Microsoft.Web.WebView2.Wpf;

using System.Windows.Controls;

namespace RTBrowser.UI.Controls
{
    public partial class WebViewContainer : UserControl
    {
        public WebViewContainer()
        {
            InitializeComponent();
        }

        public WebView2? Browser =>
            ContentHost.Children.Count > 0
                ? ContentHost.Children[0] as WebView2
                : null;

        public void SetBrowser(
            WebView2 browser)
        {
            ContentHost.Children.Clear();

            ContentHost.Children.Add(browser);
        }
    }
}
