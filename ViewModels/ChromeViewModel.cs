using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using TB.Helpers;
using TB.Services.Interfaces;

namespace TB.ViewModels;

public partial class ChromeViewModel : ObservableObject
{
    private readonly ITabManager _tabManager;
    private readonly INavigationService _navigationService;

    public ObservableCollection<TabItemViewModel> Tabs { get; } = new();

    private string _urlText = "";
    public string UrlText
    {
        get => _urlText;
        set => SetProperty(ref _urlText, value);
    }

    private string _displayUrlText = "";
    public string DisplayUrlText
    {
        get => _displayUrlText;
        set => SetProperty(ref _displayUrlText, value);
    }

    private string _title = "TB Browser";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private bool _canGoBack;
    public bool CanGoBack
    {
        get => _canGoBack;
        set => SetProperty(ref _canGoBack, value);
    }

    private bool _canGoForward;
    public bool CanGoForward
    {
        get => _canGoForward;
        set => SetProperty(ref _canGoForward, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
                OnPropertyChanged(nameof(ReloadIconData));
        }
    }

    public string ReloadIconData => IsLoading ? "M 18 6 L 6 18 M 6 6 l 12 12" : "M 3 12 a 9 9 0 1 0 2.6 -6.4 L 2 9 M 9 9 H 2 V 2";

    private TabItemViewModel? _activeTab;
    public TabItemViewModel? ActiveTab
    {
        get => _activeTab;
        set => SetProperty(ref _activeTab, value);
    }

    private double _tabWidth = 180;
    public double TabWidth
    {
        get => _tabWidth;
        set => SetProperty(ref _tabWidth, value);
    }

    public ChromeViewModel(ITabManager tabManager, INavigationService navigationService)
    {
        _tabManager = tabManager;
        _navigationService = navigationService;

        _tabManager.TabCreated += OnTabCreated;
        _tabManager.TabSwitched += OnTabSwitched;
        _tabManager.TabClosed += OnTabClosed;
        _tabManager.UrlChanged += OnUrlChanged;
        _tabManager.NavigationStarted += OnNavigationStarted;
        _tabManager.NavigationCompleted += OnNavigationCompleted;
        _tabManager.FaviconUpdated += OnFaviconUpdated;
        _tabManager.NavStateChanged += OnNavStateChanged;
        _tabManager.TabMoved += OnTabMoved;
    }

    private void OnTabMoved(object? sender, TabMovedEventArgs e)
    {
        var dq = DispatcherQueue.GetForCurrentThread();
        if (dq is null) return;
        dq.TryEnqueue(() =>
        {
            if (e.FromIndex >= 0 && e.FromIndex < Tabs.Count && e.ToIndex >= 0 && e.ToIndex < Tabs.Count)
                Tabs.Move(e.FromIndex, e.ToIndex);
        });
    }

    private void OnNavStateChanged(object? sender, NavStateEventArgs e)
    {
        var dq = DispatcherQueue.GetForCurrentThread();
        if (dq is null) return;
        dq.TryEnqueue(() =>
        {
            CanGoBack = e.CanGoBack;
            CanGoForward = e.CanGoForward;
            GoBackCommand.NotifyCanExecuteChanged();
            GoForwardCommand.NotifyCanExecuteChanged();
            if (!string.IsNullOrEmpty(e.Title))
            {
                Title = e.Title;
                // Update active tab title in strip
                var active = Tabs.FirstOrDefault(t => t.IsActive);
                if (active != null) active.Title = e.Title;
            }
        });
    }

    public void RecalculateTabWidth(double containerWidth)
    {
        if (Tabs.Count == 0)
        {
            TabWidth = 180;
            return;
        }
        double availableWidth = Math.Max(0, containerWidth - 32);
        double calculated = availableWidth / Tabs.Count;
        TabWidth = Math.Max(32, Math.Min(180, calculated));
    }

    [RelayCommand]
    private void Navigate()
    {
        var resolved = _navigationService.ResolveUrl(UrlText);
        if (!string.IsNullOrEmpty(resolved))
            _navigationService.Navigate(resolved);
    }

    [RelayCommand]
    private void GoHome()
    {
        _navigationService.Navigate(Defaults.HomeUrl);
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack()
    {
        _navigationService.GoBack();
    }

    [RelayCommand(CanExecute = nameof(CanGoForward))]
    private void GoForward()
    {
        _navigationService.GoForward();
    }

    [RelayCommand]
    private void Reload()
    {
        if (IsLoading)
            _navigationService.Stop();
        else
            _navigationService.Reload();
    }

    [RelayCommand]
    private void OpenSettings()
    {
        _navigationService.Navigate(Routes.Settings);
    }

    [RelayCommand]
    private void OpenDownloads()
    {
        _navigationService.Navigate(Routes.Downloads);
    }

    [RelayCommand]
    private void OpenFlags()
    {
        _navigationService.Navigate(Routes.Flags);
    }

    [RelayCommand]
    private void NewTab()
    {
        _tabManager.CreateTabAsync().FireAndForget();
    }

    [RelayCommand]
    private void ReopenClosedTab()
    {
        _tabManager.ReopenLastClosedTabAsync().FireAndForget();
    }

    [RelayCommand]
    private void CloseActiveTab()
    {
        _tabManager.CloseActiveTab();
    }

    private void OnTabCreated(object? sender, TabEventArgs e)
    {
        var dq = DispatcherQueue.GetForCurrentThread();
        if (dq is null) return;
        dq.TryEnqueue(() =>
        {
            var vm = new TabItemViewModel(e.Id, _tabManager)
            {
                Title = e.Title,
                Url = e.Url
            };
            Tabs.Add(vm);
        });
    }

    private void OnTabSwitched(object? sender, TabEventArgs e)
    {
        var dq = DispatcherQueue.GetForCurrentThread();
        if (dq is null) return;
        dq.TryEnqueue(() =>
        {
            foreach (var tab in Tabs)
                tab.IsActive = tab.Id == e.Id;
            ActiveTab = Tabs.FirstOrDefault(t => t.Id == e.Id);
            if (!string.IsNullOrEmpty(e.Title))
            {
                var target = Tabs.FirstOrDefault(t => t.Id == e.Id);
                if (target != null) target.Title = e.Title;
                Title = e.Title;
            }
        });
    }

    private void OnTabClosed(object? sender, TabEventArgs e)
    {
        var dq = DispatcherQueue.GetForCurrentThread();
        if (dq is null) return;
        dq.TryEnqueue(() =>
        {
            var tab = Tabs.FirstOrDefault(t => t.Id == e.Id);
            if (tab != null)
                Tabs.Remove(tab);
        });
    }

    private void OnUrlChanged(object? sender, UrlEventArgs e)
    {
        var dq = DispatcherQueue.GetForCurrentThread();
        if (dq is null) return;
        dq.TryEnqueue(() =>
        {
            UrlText = e.Url;
            if (Uri.TryCreate(e.Url, UriKind.Absolute, out var uri))
                DisplayUrlText = uri.Host.Replace("www.", "");
            else
                DisplayUrlText = e.Url;
        });
    }

    private void OnFaviconUpdated(object? sender, FaviconEventArgs e)
    {
        var dq = DispatcherQueue.GetForCurrentThread();
        if (dq is null) return;
        dq.TryEnqueue(() =>
        {
            var tab = Tabs.FirstOrDefault(t => t.Id == e.TabId);
            if (tab != null)
                tab.Favicon = e.Favicon;
        });
    }

    private void OnNavigationStarted(object? sender, EventArgs e)
    {
        var dq = DispatcherQueue.GetForCurrentThread();
        if (dq is null) return;
        dq.TryEnqueue(() => IsLoading = true);
    }

    private void OnNavigationCompleted(object? sender, EventArgs e)
    {
        var dq = DispatcherQueue.GetForCurrentThread();
        if (dq is null) return;
        dq.TryEnqueue(() => IsLoading = false);
    }
}
