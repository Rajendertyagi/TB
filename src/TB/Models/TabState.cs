using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace TB.Models
{
    public partial class TabState : ObservableObject
    {
        [ObservableProperty]
        private Guid _tabId = Guid.NewGuid();

        [ObservableProperty]
        private string _url = "about:blank";

        [ObservableProperty]
        private string _title = "New Tab";

        [ObservableProperty]
        private string _faviconUrl;

        [ObservableProperty]
        private bool _isActive;

        [ObservableProperty]
        private bool _isPinned;

        [ObservableProperty]
        private bool _isSuspended;

        // Restoration Data (Does not need UI binding notifications)
        public double ScrollX { get; set; }
        public double ScrollY { get; set; }
        public double ZoomLevel { get; set; } = 1.0;

        // Serialized JSON string for the Back/Forward button history
        public string NavigationStack { get; set; } = "[]";
    }
}
