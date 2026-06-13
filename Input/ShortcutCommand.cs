using System;
using Windows.System;

namespace TB.Input;

public enum BrowserCommand
{
    NewTab,
    NewWindow,
    CloseTab,
    ReopenClosedTab,
    Reload,
    HardReload,
    Back,
    Forward,
    FocusAddressBar,
    ZoomIn,
    ZoomOut,
    ResetZoom,
    NextTab,
    PrevTab,
    GoToTab1,
    GoToTab2,
    GoToTab3,
    GoToTab4,
    GoToTab5,
    GoToTab6,
    GoToTab7,
    GoToTab8,
    GoToTab9,
    MoveTabLeft,
    MoveTabRight,
    CloseWindow,
    ToggleFullScreen,
    Home,
    OpenDownloads,
    OpenHistory,
    OpenBookmarks,
    ToggleBookmarksBar,
    ClearBrowsingData,
    Print,
    ViewSource,
    ToggleDevTools,
    OpenFind,
    FindNext,
    FindPrev,
    SavePage,
    ToggleCommandPalette
}

public readonly struct ShortcutBinding
{
    public bool Ctrl { get; }
    public bool Alt { get; }
    public bool Shift { get; }
    public VirtualKey Key { get; }
    public BrowserCommand Command { get; }

    public ShortcutBinding(VirtualKey key, bool ctrl, bool alt, bool shift, BrowserCommand command)
    {
        Key = key; Ctrl = ctrl; Alt = alt; Shift = shift; Command = command;
    }
}
