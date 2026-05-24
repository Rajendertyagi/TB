using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace TB
{
    public partial class MainViewModel : ObservableObject
    {
        public ObservableCollection<BrowserTab> Tabs { get; } = new ObservableCollection<BrowserTab>();

        [ObservableProperty]
        private string _status = "Browser Ready";
    }
}
