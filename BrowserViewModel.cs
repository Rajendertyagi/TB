using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace TradingBrowser
{
    public partial class BrowserViewModel : ObservableObject
    {
        // Thread-safe collection that automatically updates the UI TabStrip on addition/removal
        public ObservableCollection<TabContext> OpenTabs { get; } = new ObservableCollection<TabContext>();

        [ObservableProperty]
        private TabContext _activeTab;

        [ObservableProperty]
        private string _omniboxAddressText;

        [RelayCommand]
        public void OpenNewTab()
        {
            var newTab = new TabContext("New Workspace", "https://www.google.com");
            OpenTabs.Add(newTab);
            ActiveTab = newTab;
        }

        [RelayCommand]
        public void CloseTab(TabContext targetTab)
        {
            if (targetTab == null) return;
            
            OpenTabs.Remove(targetTab);
            
            // If we closed our active tab, shift focus to the next remaining workspace
            if (ActiveTab == targetTab && OpenTabs.Count > 0)
            {
                ActiveTab = OpenTabs[OpenTabs.Count - 1];
            }
        }
    }
}
