using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TB.Models;
using TB.Services;

namespace TB.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService;
        private readonly Dictionary<Guid, TabState> _tabLookup = new();

        [ObservableProperty]
        private ObservableCollection<TabState> _tabs = new();

        [ObservableProperty]
        private TabState _currentTab;

        [ObservableProperty]
        private string _omniboxText;

        public MainViewModel()
        {
            _dbService = new DatabaseService();
            InitializeSession();
        }

        private void InitializeSession()
        {
            // TODO: In a full implementation, query _dbService for the last session's tabs.
            // For now, we boot with a fresh tab to ensure the UI has something to render.
            CreateTab();
        }

        // Triggered automatically by CommunityToolkit when CurrentTab changes
        partial void OnCurrentTabChanged(TabState value)
        {
            if (value != null)
            {
                OmniboxText = value.Url;
            }
        }

        [RelayCommand]
        public void CreateTab()
        {
            var newTab = new TabState { Title = "New Tab", Url = "about:blank" };
            
            _tabLookup[newTab.TabId] = newTab;
            Tabs.Add(newTab);
            
            SelectTab(newTab.TabId);
        }

        [RelayCommand]
        public void CloseTab(Guid tabId)
        {
            if (!_tabLookup.TryGetValue(tabId, out var tabToRemove)) return;

            // Determine fallback tab if we are closing the active one
            if (CurrentTab?.TabId == tabId)
            {
                int index = Tabs.IndexOf(tabToRemove);
                var nextTab = Tabs.ElementAtOrDefault(index + 1) ?? Tabs.ElementAtOrDefault(index - 1);
                
                if (nextTab != null)
                {
                    SelectTab(nextTab.TabId);
                }
                else
                {
                    // Don't leave the browser empty, spawn a new tab
                    CreateTab();
                }
            }

            Tabs.Remove(tabToRemove);
            _tabLookup.Remove(tabId);
            
            // TODO: Instruct _dbService to delete this tab from SQLite
        }

        [RelayCommand]
        public void SelectTab(Guid tabId)
        {
            if (!_tabLookup.TryGetValue(tabId, out var incomingTab) || CurrentTab?.TabId == tabId) 
                return;

            // 1. Suspend outgoing tab
            if (CurrentTab != null)
            {
                CurrentTab.IsActive = false;
                if (!CurrentTab.IsPinned)
                {
                    CurrentTab.IsSuspended = true;
                }
                _dbService.SaveTabState(CurrentTab);
            }

            // 2. Activate incoming tab
            incomingTab.IsActive = true;
            incomingTab.IsSuspended = false;
            CurrentTab = incomingTab;
            
            // Note: The actual WebView2 DOM injection/extraction will be handled in the View's code-behind,
            // reacting to the CurrentTab change, because ViewModels should not directly touch UI controls.
        }

        [RelayCommand]
        public void Navigate(string url)
        {
            if (CurrentTab == null) return;
            
            // Basic URL formatting fallback
            if (!url.StartsWith("http") && !url.StartsWith("about:"))
            {
                url = $"https://{url}";
            }

            CurrentTab.Url = url;
            OmniboxText = url;
            
            _dbService.SaveTabState(CurrentTab);
        }

        [RelayCommand]
        public void ProcessFailed()
        {
            // If WebView2 crashes, we preserve the state and signal the UI to respawn the control
            if (CurrentTab != null)
            {
                _dbService.SaveTabState(CurrentTab);
            }
            
            // In a production app, we would log this to Serilog here
        }

        [RelayCommand]
        public void SaveSession()
        {
            // Flush all current states to SQLite before teardown
            foreach (var tab in _tabLookup.Values)
            {
                _dbService.SaveTabState(tab);
            }
        }
    }
}
