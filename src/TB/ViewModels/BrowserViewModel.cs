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

    [ObservableProperty]
    private BrowserTab? activeTab;

    public BrowserViewModel(
        IBrowserService browserService,
        ITabManager tabManager)
    {
        _browserService = browserService;
        _tabManager = tabManager;

        if (_tabManager.Tabs.Count == 0)
        {
            ActiveTab = _tabManager.AddTab();
        }
        else
        {
            ActiveTab = _tabManager.ActiveTab;
        }
    }

    partial void OnActiveTabChanged(BrowserTab? value)
    {
        if (value is null)
        {
            return;
        }

        _tabManager.SetActiveTab(value.Id);

        Address = value.Address;
    }

    public ObservableCollection<BrowserTab> Tabs => _tabManager.Tabs;

    [ObservableProperty]
    private string address = "https://www.google.com";

    [RelayCommand]
    private void AddTab()
    {
        ActiveTab = _tabManager.AddTab();
    }

    [RelayCommand]
    private void Navigate()
    {
        if (ActiveTab is not null)
        {
            ActiveTab.Address = Address;
            ActiveTab.LastVisitedUtc = DateTime.UtcNow;

            if (ActiveTab.Title == "New Tab")
            {
                ActiveTab.Title = Address;
            }
        }

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