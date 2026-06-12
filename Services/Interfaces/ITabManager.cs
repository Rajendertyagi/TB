using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using TB.Helpers;
using TB.Models;

namespace TB.Services.Interfaces;

public interface ITabManager : IAsyncDisposable
{
    event EventHandler<TabEventArgs>? TabCreated;
    event EventHandler<TabEventArgs>? TabSwitched;
    event EventHandler<TabEventArgs>? TabClosed;
    event EventHandler<UrlEventArgs>? UrlChanged;
    event EventHandler? NavigationStarted;
    event EventHandler? NavigationCompleted;
    event EventHandler<EventArgs>? TabsCleared;
    event EventHandler<FaviconEventArgs>? FaviconUpdated;
    event EventHandler? BeforeShutdown;
    event EventHandler<NavStateEventArgs>? NavStateChanged;
    event EventHandler<TabMovedEventArgs>? TabMoved;

    int ActiveTabId { get; }
    int TabCount { get; }
    IReadOnlyList<TabItem> Tabs { get; }
    bool IsInitialized { get; }

    Task InitializeAsync(Grid contentGrid, CoreWebView2Environment env);
    Task CreateTabAsync(string url = Defaults.HomeUrl);
    Task DuplicateTabAsync(int id);
    void SwitchTab(int id);
    void SwitchToIndex(int index);
    void CloseTab(int id);
    void CloseOtherTabs(int id);
    void CloseTabsToTheRight(int id);
    void CloseActiveTab();
    void NextTab();
    void PrevTab();
    void NavigateActiveTab(string url);
    void Reload();
    void ReloadTab(int id);
    void Stop();
    void Back();
    void Forward();
    Task SetZoomAsync(int tabId, double zoom);
    Task ZoomInAsync();
    Task ZoomOutAsync();
    Task ResetZoomAsync();
    Task HardReloadAsync();
    Task PrintAsync();
    void ViewSource();
    void FocusActiveTab();

    // Chrome Parity Shortcuts
    Task ReopenLastClosedTabAsync();
    void GoToTab(int index);
    void GoToLastTab();
    void MoveTabLeft();
    void MoveTabRight();
    Task OpenFeedbackWindowAsync();
    void ToggleBookmarksBar();
    Task OpenBookmarksManagerAsync();
    Task OpenHistoryPageAsync();
    Task OpenDownloadsPageAsync();
    Task OpenTaskManagerAsync();
    Task OpenDeveloperToolsAsync();
    Task OpenChromeMenuAsync();
    Task OpenClearBrowsingDataDialogAsync();
    Task OpenFindBarAsync();
    Task FindNextAsync();
    Task FindPreviousAsync();
    Task CloseFindBarAsync();
    void AddressBarEnd();
    Task PageTopAsync();
    Task PageBottomAsync();
    void ToggleFullScreen();
    Task CursorWordPreviousAsync();
    Task CursorWordNextAsync();
    Task DeleteWordPreviousAsync();
    void SelectMultipleTabs();
}

