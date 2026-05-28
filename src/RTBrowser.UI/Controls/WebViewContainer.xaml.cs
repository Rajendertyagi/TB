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

        public WebView2 Browser
        {
            get
            {
                return BrowserHost;
            }
        }
    }
}
