using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TB.Services;

namespace TB.ViewModels;

public sealed partial class BrowserViewModel : ObservableObject
{
    private readonly IBrowserService _browserService;

    public BrowserViewModel(IBrowserService browserService)
    {
        _browserService = browserService;
    }

    [ObservableProperty]
    private string address = "https://www.google.com";

    [RelayCommand]
    private void Navigate()
    {
        _browserService.Navigate(Address);
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
