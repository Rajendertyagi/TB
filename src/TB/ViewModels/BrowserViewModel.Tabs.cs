using System.Windows;
using CommunityToolkit.Mvvm.Input;
using TB.Models;
using TB.Modules.Logging.Services;

namespace TB.ViewModels;

public sealed partial class BrowserViewModel
{
    [RelayCommand]
    private void AddTab()
    {
        CommandLogger.Requested(
            "AddTab");

        CommandLogger.Started(
            "AddTab");

        ActiveTab =
            _tabManager.AddTab();

        CommandLogger.Completed(
            "AddTab");
    }

    [RelayCommand]
    private void CloseTab(
        BrowserTab? tab)
    {
        CommandLogger.Requested(
            "CloseTab");

        if (tab is null)
        {
            CommandLogger.Warning(
                "CloseTab",
                "Tab was null");

            return;
        }

        CommandLogger.Started(
            "CloseTab");

        if (_tabManager.Tabs.Count == 1)
        {
            CommandLogger.Warning(
                "CloseTab",
                "Closing final tab, shutting down application");

            Application.Current.Shutdown();

            CommandLogger.Completed(
                "CloseTab");

            return;
        }

        _webViewManager.Remove(
            tab.Id);

        _tabManager.CloseTab(
            tab.Id);

        ActiveTab =
            _tabManager.ActiveTab;

        CommandLogger.Completed(
            "CloseTab");
    }
}