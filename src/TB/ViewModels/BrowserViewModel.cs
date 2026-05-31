using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TB.Models;
using TB.Modules.Logging.Services;
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

    [ObservableProperty]
    private string address = "https://www.google.com";

    public ObservableCollection<BrowserTab> Tabs => _tabManager.Tabs;

    public ObservableCollection<Bookmark> Bookmarks { get; }

    public BrowserViewModel(
        IBrowserService browserService,
        ITabManager tabManager,
        IBookmarkService bookmarkService,
        IWebViewManager webViewManager)
    {
        ViewModelLogger.Created(
            nameof(BrowserViewModel));

        _browserService = browserService;
        _tabManager = tabManager;
        _bookmarkService = bookmarkService;
        _webViewManager = webViewManager;

        Bookmarks = new ObservableCollection<Bookmark>(
            _bookmarkService.Load());

        BookmarkLogger.Loaded(
            Bookmarks.Count);

        if (_tabManager.Tabs.Count == 0)
        {
            ActiveTab = _tabManager.AddTab();
        }
        else
        {
            ActiveTab = _tabManager.ActiveTab;
        }

        ViewModelLogger.Initialized(
            nameof(BrowserViewModel));
    }

    partial void OnActiveTabChanged(
        BrowserTab? value)
    {
        ViewModelLogger.PropertyChanged(
            nameof(BrowserViewModel),
            nameof(ActiveTab));

        if (value is null)
        {
            ViewModelLogger.Warning(
                nameof(BrowserViewModel),
                "ActiveTab changed to null");

            return;
        }

        Address = value.Address;

        ViewModelLogger.PropertyChanged(
            nameof(BrowserViewModel),
            nameof(Address));
    }
}

