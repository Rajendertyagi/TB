using System;
using Microsoft.UI.Xaml.Media;

namespace TB.Models;

public class TabEventArgs : EventArgs
{
    public int Id { get; init; }
    public string Title { get; init; } = "";
    public string Url { get; init; } = "";
}

public class UrlEventArgs : EventArgs
{
    public string Url { get; init; } = "";
}

public class FaviconEventArgs : EventArgs
{
    public int TabId { get; init; }
    public ImageSource? Favicon { get; init; }
}

public class NavStateEventArgs : EventArgs
{
    public bool CanGoBack { get; init; }
    public bool CanGoForward { get; init; }
    public string Title { get; init; } = "";
}

public class TabMovedEventArgs : EventArgs
{
    public int TabId { get; init; }
    public int FromIndex { get; init; }
    public int ToIndex { get; init; }
}

public class FindResultEventArgs : EventArgs
{
    public int ActiveMatchIndex { get; init; }
    public int MatchCount { get; init; }
}
