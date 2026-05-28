using Microsoft.Web.WebView2.Wpf;

using System.Windows;
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
            if (ContentHost.Children.Contains(browser))
            {
                return;
            }

            ContentHost.Children.Clear();

            browser.HorizontalAlignment =
                HorizontalAlignment.Stretch;

            browser.VerticalAlignment =
                VerticalAlignment.Stretch;

            browser.Margin =
                new Thickness(0);

            browser.Focusable =
                true;

            ContentHost.Children.Add(browser);
        }

        public void Clear()
        {
            ContentHost.Children.Clear();
        }
    }
}
