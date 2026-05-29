using Microsoft.Web.WebView2.Wpf;

namespace TB.Services;

public interface IBrowserService
{
    void Attach(WebView2 browser);
    void Navigate(string url);
    void GoBack();
    void GoForward();
    void Refresh();
}
