using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;
using System.Linq;
using TB.Helpers;
using TB.Services.Interfaces;

namespace TB.ViewModels;

public partial class ChromeViewModel : ObservableObject
{
    private readonly ITabManager _tabManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly DispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    private string urlText = "";

    public ObservableCollection<TabItemViewModel> Tabs { get; } = new();

    public ChromeViewModel(ITabManager tabManager, IServiceProvider serviceProvider)
    {
        _tabManager = tabManager;
        _serviceProvider = serviceProvider;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread()!;

        _tabManager.TabCreated += (_, e) =>
        {
            var tabVm = ActivatorUtilities.CreateInstance<TabItemViewModel>(_serviceProvider, e.Id, e.Title, e.Url);
            MutateTabs(() => Tabs.Add(tabVm));
        };

        _tabManager.TabClosed += (_, e) =>
        {
            var tabVm = Tabs.FirstOrDefault(t => t.Id == e.Id);
            if (tabVm != null)
                MutateTabs(() => Tabs.Remove(tabVm));
        };

        _tabManager.TabTitleChanged += (_, e) =>
        {
            var tabVm = Tabs.FirstOrDefault(t => t.Id == e.Id);
            if (tabVm != null)
                DispatchUI(() => tabVm.Title = e.Title);
        };

        _tabManager.TabSwitched += (_, e) =>
        {
            DispatchUI(() =>
            {
                UrlText = e.Url;
                foreach (var tab in Tabs)
                    tab.IsActive = tab.Id == e.Id;
            });
        };

        _tabManager.UrlChanged += (_, e) =>
        {
            DispatchUI(() => UrlText = e.Url);
        };

        _tabManager.FaviconUpdated += (_, e) =>
        {
            var tabVm = Tabs.FirstOrDefault(t => t.Id == e.TabId);
            if (tabVm != null)
                DispatchUI(() => tabVm.Favicon = e.Favicon);
        };
    }

    private void MutateTabs(DispatcherQueueHandler action)
    {
        if (_dispatcherQueue.HasThreadAccess)
            action();
        else
            _dispatcherQueue.TryEnqueue(action);
    }

    private void DispatchUI(DispatcherQueueHandler action)
    {
        if (_dispatcherQueue.HasThreadAccess)
            action();
        else
            _dispatcherQueue.TryEnqueue(action);
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

        _tabManager.NavigateActiveTab(UrlResolver.ParseInput(UrlText));
    }

    [RelayCommand]
    private void Downloads() => _tabManager.NavigateActiveTab(Routes.Downloads);

    [RelayCommand]
    private void History() => _tabManager.NavigateActiveTab(Routes.History);

    [RelayCommand]
    private void Settings() => _tabManager.NavigateActiveTab(Routes.Settings);

    [RelayCommand]
    private void Flags() => _tabManager.NavigateActiveTab(Routes.Flags);
}
