using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TB;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _status = "Ready";

    [RelayCommand]
    private void UpdateStatus()
    {
        Status = "Application is running!";
    }
}
