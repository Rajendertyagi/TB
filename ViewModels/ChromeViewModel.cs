using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using TB.Helpers;
using TB.Services.Interfaces;

namespace TB.ViewModels;

public partial class ChromeViewModel : ObservableObject
{
    private readonly ITabManager _tabManager;

    [ObservableProperty]
    private string urlText = "";

    [ObservableProperty]
    private double tabWidth = 180;

    public ObservableCollection<TabItemViewModel> Tabs { get; } = new();

    public ChromeViewModel(ITabManager tabManager)
    {
        _tabManager = tabManager;

        _tabManager.TabCreated += (_, e) =>
        {
            var tabVm = new TabItemViewModel(e.Id, e.Title, e.Url, _tabManager);
            Tabs.Add(tabVm);
        };

        _tabManager.TabClosed += (_, e) =>
        {
            var tabVm = Tabs.FirstOrDefault(t => t.Id == e.Id);
            if (tabVm != null)
                Tabs.Remove(tabVm);
        };

        _tabManager.TabSwitched += (_, e) =>
        {
            UrlText = e.Url;

            foreach (var tab in Tabs)
                tab.IsActive = tab.Id == e.Id;
        };

        _tabManager.UrlChanged += (_, e) =>
        {
            UrlText = e.Url;
        };
    }

    [RelayCommand]
    private void Back() => _tabManager.Back();

    [RelayCommand]
    private void Forward() => _tabManager.Forward();

    [RelayCommand]
    private void Reload() => _tabManager.Reload();

    [RelayCommand]
    private void Home() => _tabManager.NavigateActiveTab(Defaults.HomeUrl);

    [RelayCommand]
    private void NewTab() => _ = _tabManager.CreateTabAsync();

    [RelayCommand]
    private void Navigate()
    {
        if (string.IsNullOrWhiteSpace(UrlText))
            return;

        _tabManager.NavigateActiveTab(UrlText);
    }

    [RelayCommand]
    private void Downloads() => _tabManager.NavigateActiveTab(Routes.Downloads);

    [RelayCommand]
    private void History() => _tabManager.NavigateActiveTab(Routes.History);

    [RelayCommand]
    private void Settings() => _tabManager.NavigateActiveTab(Routes.Settings);

    [RelayCommand]
    private void Flags() => _tabManager.NavigateActiveTab(Routes.Flags);

    public void RecalculateTabWidth(double availableWidth)
    {
        if (Tabs.Count == 0)
            return;

        double maxWidth = Layout.TabMaxWidth;
        double minWidth = Layout.TabMinWidth;
        double gap = Layout.TabInterTabGap;

        double totalGaps = (Tabs.Count - 1) * gap;
        double spaceForTabs = availableWidth - totalGaps - 40;

        TabWidth = System.Math.Clamp(spaceForTabs / Tabs.Count, minWidth, maxWidth);
    }
}
