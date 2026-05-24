using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using TB.Models;

namespace TB.ViewModels;

public partial class BrowserViewModel : ObservableObject
{
    // The Virtualized Tab Store
    private readonly Dictionary<string, TabState> _tabs = new();

    [ObservableProperty]
    private string _currentTabId;

    public void SwitchTab(string newTabId)
    {
        // 1. Save current tab state (if one exists)
        if (!string.IsNullOrEmpty(CurrentTabId) && _tabs.ContainsKey(CurrentTabId))
        {
            var currentState = _tabs[CurrentTabId];
            // Capture current URL and scroll position from WebView2
            // currentState.Url = WebView2.Source.ToString();
        }

        // 2. Load target tab state
        if (_tabs.TryGetValue(newTabId, out var targetState))
        {
            CurrentTabId = newTabId;
            // Navigate the SINGLE WebView2 instance to targetState.Url
        }
    }

    public void AddTab(string url)
    {
        // Create new TabState and add to Dictionary
    }

    public void CloseTab(string tabId)
    {
        // Strict cleanup: Dispose, remove from Dictionary, 
        // unsubscribe from WebView2 events to prevent memory leaks
    }
}
