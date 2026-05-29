using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TB.Models;
using TB.Services;

namespace TB.ViewModels;

public sealed partial class BrowserViewModel : ObservableObject
{
    private readonly IBrowserService _browserService;
    private readonly ITabManager _tabManager;

    public BrowserViewModel(
        IBrowserService browserService,
        ITabManager tabManager)
    {
        _browserService = browserService;
        _tabManager = tabManager;

        if (_tabManager.Tabs.Count == 0)
        {
            _tabManager.AddTab();
        }
    }

    public ObservableCollection<BrowserTab> Tabs => _tabManager.Tabs;

    [ObservableProperty]
    private string address = "https://www.google.com";

    [RelayCommand]
    private void AddTab()
    {
        _tabManager.AddTab();

        OnPropertyChanged(nameof(Tabs));
    }

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
