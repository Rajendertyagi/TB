using CommunityToolkit.Mvvm.ComponentModel;

namespace TB.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "Trading Browser";
}
