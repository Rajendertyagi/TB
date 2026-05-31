using System.Windows;
using CommunityToolkit.Mvvm.Input;
using TB.Models;

namespace TB.ViewModels;

public sealed partial class BrowserViewModel
{
    [RelayCommand]
    private void AddTab()
    {
        ActiveTab = _tabManager.AddTab();
    }

    [RelayCommand]
    private void CloseTab(
        BrowserTab? tab)
    {
        if (tab is null)
        {
            return;
        }

        if (_tabManager.Tabs.Count == 1)
        {
            Application.Current.Shutdown();
            return;
        }

        _webViewManager.Remove(
            tab.Id);

        _tabManager.CloseTab(
            tab.Id);

        ActiveTab =
            _tabManager.ActiveTab;
    }
}