using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TB.Helpers;
using TB.Services.Interfaces;

namespace TB.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ChromeViewModel Chrome { get; }

    private string _windowTitle = "TB Browser";
    public string WindowTitle
    {
        get => _windowTitle;
        set => SetProperty(ref _windowTitle, value);
    }

    public MainViewModel(ChromeViewModel chrome, ITabManager tabManager)
    {
        Chrome = chrome;

        tabManager.TabSwitched += (_, e) =>
        {
            WindowTitle = string.IsNullOrEmpty(e.Title) || e.Title == "New Tab"
                ? "TB Browser"
                : $"{e.Title} - TB Browser";
        };

        tabManager.TabTitleChanged += (_, e) =>
        {
            if (e.Id == tabManager.ActiveTabId)
            {
                WindowTitle = string.IsNullOrEmpty(e.Title) || e.Title == "New Tab"
                    ? "TB Browser"
                    : $"{e.Title} - TB Browser";
            }
        };

        tabManager.TabCreated += (_, _) => { };
        tabManager.TabsCleared += (_, _) => WindowTitle = "TB Browser";
    }

    [RelayCommand]
    private void NewWindow()
    {
        // Launch a new instance
        var exePath = Environment.ProcessPath;
        if (exePath != null)
            System.Diagnostics.Process.Start(exePath);
    }
}
