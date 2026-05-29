using CommunityToolkit.Mvvm.ComponentModel;

namespace TB.ViewModels;

public sealed partial class BrowserViewModel : ObservableObject
{
    [ObservableProperty]
    private string address = "https://www.google.com";
}
