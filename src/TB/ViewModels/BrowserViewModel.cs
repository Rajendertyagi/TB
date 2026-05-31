using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TB.Models;
using TB.Services;

namespace TB.ViewModels;

public sealed partial class BrowserViewModel : ObservableObject
{
    private readonly IBrowserService _browserService;
    private readonly ITabManager _tabManager;
    private readonly IBookmarkService _bookmarkService;
    private readonly IWebViewManager _webViewManager;

    [ObservableProperty]
    private BrowserTab? activeTab;

    public BrowserViewModel(
        IBrowserService browserService,
        ITabManager tabManager,
        IBookmarkService bookmarkService,
        IWebViewManager webViewManager)
    {
        _browserService = browserService;
        _tabManager = tabManager;
        _bookmarkService = bookmarkService;
        _webViewManager = webViewManager;

        Bookmarks = new ObservableCollection<Bookmark>(
            _bookmarkService.Load());

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

        Address = value.Address;
    }

    public ObservableCollection<BrowserTab> Tabs => _tabManager.Tabs;

    public ObservableCollection<Bookmark> Bookmarks { get; }

    [ObservableProperty]
    private string address = "https://www.google.com";

    [RelayCommand]
    private void AddTab()
    {
        ActiveTab = _tabManager.AddTab();
    }

    [RelayCommand]
    private void CloseTab(BrowserTab? tab)
    {
        if (tab is null)
        {
            return;
        }

        if (_tabManager.Tabs.Count == 1)
        {
            Application.Current.Shutdown();
            return;
        }

        _webViewManager.Remove(
            tab.Id);

        _tabManager.CloseTab(
            tab.Id);

        ActiveTab =
            _tabManager.ActiveTab;
    }

    [RelayCommand]
    private void AddBookmark()
    {
        if (ActiveTab is null)
        {
            return;
        }

        var bookmark = new Bookmark
        {
            Title = ActiveTab.Title,
            Url = ActiveTab.Address
        };

        Bookmarks.Add(bookmark);

        _bookmarkService.Save(Bookmarks.ToList());
    }

    [RelayCommand]
    private void RemoveBookmark(Bookmark? bookmark)
    {
        if (bookmark is null)
        {
            return;
        }

        Bookmarks.Remove(bookmark);

        _bookmarkService.Save(Bookmarks.ToList());
    }

    [RelayCommand]
    private void Navigate()
    {
        if (ActiveTab is not null)
        {
            ActiveTab.Address = Address;

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
