using CommunityToolkit.Mvvm.ComponentModel;
using TB.Services;

namespace TB.ViewModels;

public sealed partial class TabsViewModel : ObservableObject
{
    private readonly ITabManager _tabManager;

    public TabsViewModel(ITabManager tabManager)
    {
        _tabManager = tabManager;
    }

    public int TabCount => _tabManager.Tabs.Count;
}
