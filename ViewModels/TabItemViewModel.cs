using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using TB.Controls;
using TB.Services.Interfaces;

namespace TB.ViewModels;

public partial class TabItemViewModel : ObservableObject
{
    private readonly ITabManager _tabManager;

    // ── Identity ──────────────────────────────────────────────────────────
    public int Id { get; }

    [ObservableProperty] private string title = "";
    [ObservableProperty] private string url   = "";

    // ── Interaction state (set by TabStrip event handlers) ───────────────
    [ObservableProperty] private bool isActive;
    [ObservableProperty] private bool isHovered;

    // ── Compression state (set by TabStrip after TabStripLayout.Compute) ─
    [ObservableProperty] private TabCompressionState compressionState = TabCompressionState.Normal;

    // ── Content ───────────────────────────────────────────────────────────
    [ObservableProperty] private ImageSource? favicon;

    // ── Derived helpers ───────────────────────────────────────────────────

    /// <summary>True when the tab is narrow enough to hide the title (Squashed or Overflow).</summary>
    public bool IsSquashed => CompressionState is TabCompressionState.Squashed
                                               or TabCompressionState.Overflow;

    // ── Opacity pipeline: 0.40 inactive · 0.85 hovered · 1.0 active ─────
    public double TabOpacity => IsActive ? 1.0 : (IsHovered ? 0.85 : 0.40);

    // ── Background brush ──────────────────────────────────────────────────
    public Brush TabBackground
    {
        get
        {
            if (IsActive)
                return Application.Current.Resources["bgAppBrush"] as Brush
                    ?? new SolidColorBrush(Colors.Transparent);
            if (IsHovered)
                return Application.Current.Resources["bgTabHoverBrush"] as Brush
                    ?? new SolidColorBrush(Colors.Transparent);
            return new SolidColorBrush(Colors.Transparent);
        }
    }

    // ── Border brush (shows on hover for inactive tabs) ───────────────────
    public Brush TabBorderBrush
    {
        get
        {
            if (IsHovered && !IsActive)
                return Application.Current.Resources["borderCrispBrush"] as Brush
                    ?? new SolidColorBrush(Colors.Transparent);
            return new SolidColorBrush(Colors.Transparent);
        }
    }

    // ── Close button visibility ───────────────────────────────────────────
    // Squashed col-0 close (over favicon slot): squashed + (hovered or active)
    public bool IsCloseSquashedVisible => IsSquashed && IsCloseButtonVisible;

    // Normal col-2 close (right edge): not squashed + (hovered or active)
    public bool IsCloseNormalVisible   => !IsSquashed && IsCloseButtonVisible;

    // Master gate: show close when hovered at any width, or when active and not squashed
    private bool IsCloseButtonVisible  => IsHovered || (IsActive && !IsSquashed);

    // ── Favicon visibility ────────────────────────────────────────────────
    // Favicon hides when squashed+hovered so the close button can take its center slot
    public bool IsFaviconVisible => !(IsSquashed && IsHovered);

    // ── Title visibility ──────────────────────────────────────────────────
    // Title collapses in Squashed / Overflow states
    public bool IsTitleVisible => !IsSquashed;

    // ── Property change notifications ─────────────────────────────────────
    partial void OnIsActiveChanged(bool value)           => NotifyAll();
    partial void OnIsHoveredChanged(bool value)          => NotifyAll();
    partial void OnCompressionStateChanged(TabCompressionState value) => NotifyAll();

    private void NotifyAll()
    {
        OnPropertyChanged(nameof(TabOpacity));
        OnPropertyChanged(nameof(TabBackground));
        OnPropertyChanged(nameof(TabBorderBrush));
        OnPropertyChanged(nameof(IsSquashed));
        OnPropertyChanged(nameof(IsCloseSquashedVisible));
        OnPropertyChanged(nameof(IsCloseNormalVisible));
        OnPropertyChanged(nameof(IsFaviconVisible));
        OnPropertyChanged(nameof(IsTitleVisible));
    }

    // ── Constructor ───────────────────────────────────────────────────────
    public TabItemViewModel(int id, string title, string url, ITabManager tabManager)
    {
        Id    = id;
        Title = title;
        Url   = url;
        _tabManager = tabManager;
    }

    // ── Commands ──────────────────────────────────────────────────────────
    [RelayCommand] private void Switch()          => _tabManager.SwitchTab(Id);
    [RelayCommand] private void Close()           => _tabManager.CloseTab(Id);
    [RelayCommand] private void NewTab()          => _ = _tabManager.CreateTabAsync();
    [RelayCommand] private void Duplicate()       => _ = _tabManager.DuplicateTabAsync(Id);
    [RelayCommand] private void ReloadTab()       => _tabManager.ReloadTab(Id);
    [RelayCommand] private void CloseOtherTabs()  => _tabManager.CloseOtherTabs(Id);
}
