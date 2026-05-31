using CommunityToolkit.Mvvm.Input;

namespace TB.ViewModels;

public sealed partial class BrowserViewModel
{
    [RelayCommand]
    private void Navigate()
    {
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
    }

    [RelayCommand]
    private void Back()
    {
        _browserService.GoBack();
    }

    [RelayCommand]
    private void Forward()
    {
        _browserService.GoForward();
    }

    [RelayCommand]
    private void Refresh()
    {
        _browserService.Refresh();
    }
}