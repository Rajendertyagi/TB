using ABI.System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TB.Services.Interfaces;

namespace TB.ViewModels;

public partial class TabItemViewModel : ObservableObject
{
    private readonly ITabManager _tabManager;

    public int Id { get; }

    [ObservableProperty]
    private string title = "";

    [ObservableProperty]
    private string url = "";

    [ObservableProperty]
    private bool isActive;

    [ObservableProperty]
    private bool isHovered;

    [ObservableProperty]
    private bool isSquashed;

    public TabItemViewModel(int id, string title, string url, ITabManager tabManager)
    {
        Id = id;
        Title = title;
        Url = url;
        _tabManager = tabManager;
    }

    [RelayCommand]
    private void Switch() => _tabManager.SwitchTab(Id);

    [RelayCommand]
    private void Close() => _tabManager.CloseTab(Id);

    [RelayCommand]
    private void NewTab() => _ = _tabManager.CreateTabAsync();

    [RelayCommand]
    private void Duplicate() => _ = _tabManager.DuplicateTabAsync(Id);

    [RelayCommand]
    private void ReloadTab() => _tabManager.ReloadTab(Id);

    [RelayCommand]
    private void CloseOtherTabs() => _tabManager.CloseOtherTabs(Id);
}
