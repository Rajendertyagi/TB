using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TB.Models;
using TB.Services;

namespace TB.ViewModels;

public sealed partial class TabsViewModel : ObservableObject
{
    private readonly ITabManager _tabManager;

    [ObservableProperty]
    private BrowserTab? activeTab;

    public TabsViewModel(ITabManager tabManager)
    {
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

    public ObservableCollection<BrowserTab> Tabs => _tabManager.Tabs;

    [RelayCommand]
    private void AddTab()
    {
        ActiveTab = _tabManager.AddTab();
    }
}
