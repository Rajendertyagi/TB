using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using TB.Helpers;
using TB.Services.Interfaces;

namespace TB.ViewModels;

public partial class TabItemViewModel : ObservableObject
{
    private readonly ITabManager _tabManager;

    public int Id { get; }

    private string _title = "New Tab";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _url = "";
    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (SetProperty(ref _isActive, value))
                OnPropertyChanged(nameof(TabOpacity));
        }
    }

    private double _zoom = 1.0;
    public double Zoom
    {
        get => _zoom;
        set => SetProperty(ref _zoom, value);
    }

    private ImageSource? _favicon;
    public ImageSource? Favicon
    {
        get => _favicon;
        set => SetProperty(ref _favicon, value);
    }

    private bool _showCloseButton = true;
    public bool ShowCloseButton
    {
        get => _showCloseButton;
        set => SetProperty(ref _showCloseButton, value);
    }

    private bool _isHovered;
    public bool IsHovered
    {
        get => _isHovered;
        set
        {
            if (SetProperty(ref _isHovered, value))
            {
                OnPropertyChanged(nameof(TabOpacity));
                OnPropertyChanged(nameof(HoverBorderOpacity));
                OnPropertyChanged(nameof(EffectiveShowCloseButton));
            }
        }
    }

    private bool _isSquashed;
    public bool IsSquashed
    {
        get => _isSquashed;
        set
        {
            if (SetProperty(ref _isSquashed, value))
                OnPropertyChanged(nameof(EffectiveShowCloseButton));
        }
    }

    public double TabOpacity => IsActive ? 1.0 : IsHovered ? 0.85 : 0.40;

    public double HoverBorderOpacity => IsHovered ? 0.3 : 0.0;

    public bool EffectiveShowCloseButton => IsHovered || (_showCloseButton && !IsSquashed);

    public TabItemViewModel(int id, ITabManager tabManager)
    {
        Id = id;
        _tabManager = tabManager;
    }

    [RelayCommand]
    private void NewTab()
    {
        _tabManager.CreateTabAsync().FireAndForget();
    }

    [RelayCommand]
    private void Switch()
    {
        _tabManager.SwitchTab(Id);
    }

    [RelayCommand]
    private void Close()
    {
        _tabManager.CloseTab(Id);
    }

    [RelayCommand]
    private void Duplicate()
    {
        _tabManager.DuplicateTabAsync(Id).FireAndForget();
    }

    [RelayCommand]
    private void ReloadTab()
    {
        _tabManager.ReloadTab(Id);
    }

    [RelayCommand]
    private void CloseOtherTabs()
    {
        _tabManager.CloseOtherTabs(Id);
    }

    [RelayCommand]
    private void CloseTabsToTheRight()
    {
        _tabManager.CloseTabsToTheRight(Id);
    }
}
