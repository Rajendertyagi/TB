using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace TB
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _status = "Application is initialized and ready.";

        public MainViewModel()
        {
            Log.Information("MainViewModel initialized.");
        }

        [RelayCommand]
        public void UpdateStatus()
        {
            Status = $"Status updated at {System.DateTime.Now:HH:mm:ss}";
            Log.Information("Status updated by user.");
        }
    }
}
