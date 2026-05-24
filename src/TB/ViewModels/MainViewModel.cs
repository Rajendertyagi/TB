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
            _dbService.PurgeOldHistory();

            var savedTabs = _dbService.GetSavedTabs();
            TabState activeTabToRestore = null;

            foreach (var tab in savedTabs)
            {
                if (!tab.IsActive && !tab.IsPinned)
                {
                    tab.IsSuspended = true;
                }

                _tabLookup[tab.TabId] = tab;
                Tabs.Add(tab);

                if (tab.IsActive)
                {
                    activeTabToRestore = tab;
                }
            }

            if (Tabs.Count > 0)
            {
                CurrentTab = activeTabToRestore ?? Tabs.First();
                OmniboxText = CurrentTab.Url;
            }
            else
            {
                CreateTab();
            }
        }

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
                    CreateTab();
                }
            }

            Tabs.Remove(tabToRemove);
            _tabLookup.Remove(tabId);
            
            _dbService.DeleteTabState(tabId); 
        }

        [RelayCommand]
        public void SelectTab(Guid tabId)
        {
            if (!_tabLookup.TryGetValue(tabId, out var incomingTab) || CurrentTab?.TabId == tabId) 
                return;

            if (CurrentTab != null)
            {
                CurrentTab.IsActive = false;
                if (!CurrentTab.IsPinned)
                {
                    CurrentTab.IsSuspended = true;
                }
                _dbService.SaveTabState(CurrentTab);
            }

            incomingTab.IsActive = true;
            incomingTab.IsSuspended = false;
            CurrentTab = incomingTab;
        }

        [RelayCommand]
        public void Navigate(string url)
        {
            if (CurrentTab == null) return;
            
            if (!url.StartsWith("http") && !url.StartsWith("about:"))
            {
                url = $"https://{url}";
            }

            CurrentTab.Url = url;
            OmniboxText = url;
            
            _dbService.SaveTabState(CurrentTab);
            
            if (!url.StartsWith("about:"))
            {
                _dbService.AddToHistory(url, CurrentTab.Title);
            }
        }

        [RelayCommand]
        public void ProcessFailed()
        {
            if (CurrentTab != null)
            {
                _dbService.SaveTabState(CurrentTab);
            }
        }

        [RelayCommand]
        public void SaveSession()
        {
            foreach (var tab in _tabLookup.Values)
            {
                _dbService.SaveTabState(tab);
            }
        }
    }
}
