using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TB.Models;
using TB.Services;

namespace TB.ViewModels;

public sealed partial class TabsViewModel : ObservableObject
{
    private readonly ITabManager _tabManager;

    public TabsViewModel(ITabManager tabManager)
    {
        _tabManager = tabManager;

        if (_tabManager.Tabs.Count == 0)
        {
            _tabManager.AddTab();
        }
    }

    public IReadOnlyList<BrowserTab> Tabs => _tabManager.Tabs;

    public BrowserTab? ActiveTab => _tabManager.ActiveTab;

    [RelayCommand]
    private void AddTab()
    {
        _tabManager.AddTab();

        OnPropertyChanged(nameof(Tabs));
        OnPropertyChanged(nameof(ActiveTab));
    }
}
