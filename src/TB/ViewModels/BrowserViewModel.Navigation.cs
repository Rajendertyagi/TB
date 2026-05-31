using CommunityToolkit.Mvvm.Input;
using TB.Modules.Logging.Services;

namespace TB.ViewModels;

public sealed partial class BrowserViewModel
{
    [RelayCommand]
    private void Navigate()
    {
        CommandLogger.Requested(
            "Navigate");

        CommandLogger.Started(
            "Navigate");

        if (ActiveTab is not null)
        {
            ActiveTab.Address = Address;

            if (ActiveTab.Title == "New Tab")
            {
                ActiveTab.Title = Address;
            }
        }

        _browserService.Navigate(
            Address);

        CommandLogger.Completed(
            "Navigate");
    }

    [RelayCommand]
    private void Back()
    {
        CommandLogger.Requested(
            "Back");

        CommandLogger.Started(
            "Back");

        _browserService.GoBack();

        CommandLogger.Completed(
            "Back");
    }

    [RelayCommand]
    private void Forward()
    {
        CommandLogger.Requested(
            "Forward");

        CommandLogger.Started(
            "Forward");

        _browserService.GoForward();

        CommandLogger.Completed(
            "Forward");
    }

    [RelayCommand]
    private void Refresh()
    {
        CommandLogger.Requested(
            "Refresh");

        CommandLogger.Started(
            "Refresh");

        _browserService.Refresh();

        CommandLogger.Completed(
            "Refresh");
    }
}