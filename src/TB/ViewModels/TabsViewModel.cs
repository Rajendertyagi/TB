using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TB.Models;
using TB.Modules.Logging.Services;
using TB.Services;

namespace TB.ViewModels;

public sealed partial class TabsViewModel : ObservableObject
{
    private readonly ITabManager _tabManager;

    [ObservableProperty]
    private BrowserTab? activeTab;

    public TabsViewModel(
        ITabManager tabManager)
    {
        LifecycleLogger.Created(
            nameof(TabsViewModel));

        _tabManager = tabManager;

        if (_tabManager.Tabs.Count == 0)
        {
            ActiveTab =
                _tabManager.AddTab();
        }
        else
        {
            ActiveTab =
                _tabManager.ActiveTab;
        }

        LifecycleLogger.Initialized(
            nameof(TabsViewModel));
    }

    partial void OnActiveTabChanged(
        BrowserTab? value)
    {
        if (value is null)
        {
            ViewModelLogger.Warning(
                nameof(TabsViewModel),
                "ActiveTab became null");

            return;
        }

        ViewModelLogger.PropertyChanged(
            nameof(TabsViewModel),
            nameof(ActiveTab));

        _tabManager.SetActiveTab(
            value.Id);
    }

    public ObservableCollection<BrowserTab> Tabs =>
        _tabManager.Tabs;

    [RelayCommand]
    private void AddTab()
    {
        CommandLogger.Requested(
            "AddTab");

        ActiveTab =
            _tabManager.AddTab();

        CommandLogger.Completed(
            "AddTab");
    }
}
