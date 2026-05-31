using System.Windows;
using Microsoft.Web.WebView2.Wpf;
using TB.Modules.Logging.Services;

namespace TB;

public partial class MainWindow
{
    private async void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        var activeTab =
            _tabManager.ActiveTab;

        if (activeTab is null)
        {
            return;
        }

        var browser =
            _webViewManager.Create(
                activeTab.Id);

        BrowserHost.Content = browser;

        await InitializeWebViewAsync(
            browser);

        _browserService.Attach(
            browser);

        _browserService.SetActiveBrowser(
            browser);

        TbLogger.FontVerification(
            FontFamily.Source,
            AddressBar.FontFamily.Source,
            GoButton.FontFamily.Source);

        _browserService.Navigate(
            _viewModel.Address);
    }
}
