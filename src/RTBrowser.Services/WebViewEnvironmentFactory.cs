using Microsoft.Web.WebView2.Core;

using System.Threading.Tasks;

namespace RTBrowser.Services
{
    public static class WebViewEnvironmentFactory
    {
        private static CoreWebView2Environment? _environment;

        public static async Task<CoreWebView2Environment> GetAsync()
        {
            if (_environment != null)
            {
                return _environment;
            }

            _environment =
                await CoreWebView2Environment.CreateAsync(
                    userDataFolder:
                    BrowserPaths.WebViewData);

            return _environment;
        }
    }
}
