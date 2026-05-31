using System.Linq;
using CommunityToolkit.Mvvm.Input;
using TB.Models;
using TB.Modules.Logging.Services;

namespace TB.ViewModels;

public sealed partial class BrowserViewModel
{
    [RelayCommand]
    private void AddBookmark()
    {
        CommandLogger.Requested(
            "AddBookmark");

        if (ActiveTab is null)
        {
            CommandLogger.Warning(
                "AddBookmark",
                "ActiveTab was null");

            return;
        }

        CommandLogger.Started(
            "AddBookmark");

        var bookmark = new Bookmark
        {
            Title = ActiveTab.Title,
            Url = ActiveTab.Address
        };

        BookmarkLogger.AddRequested(
            bookmark.Title,
            bookmark.Url);

        Bookmarks.Add(
            bookmark);

        _bookmarkService.Save(
            Bookmarks.ToList());

        BookmarkLogger.AddCompleted(
            bookmark.Title,
            bookmark.Url);

        BookmarkLogger.Saved(
            Bookmarks.Count);

        CommandLogger.Completed(
            "AddBookmark");
    }

    [RelayCommand]
    private void RemoveBookmark(
        Bookmark? bookmark)
    {
        CommandLogger.Requested(
            "RemoveBookmark");

        if (bookmark is null)
        {
            CommandLogger.Warning(
                "RemoveBookmark",
                "Bookmark was null");

            return;
        }

        CommandLogger.Started(
            "RemoveBookmark");

        BookmarkLogger.RemoveRequested(
            bookmark.Title,
            bookmark.Url);

        Bookmarks.Remove(
            bookmark);

        _bookmarkService.Save(
            Bookmarks.ToList());

        BookmarkLogger.RemoveCompleted(
            bookmark.Title,
            bookmark.Url);

        BookmarkLogger.Saved(
            Bookmarks.Count);

        CommandLogger.Completed(
            "RemoveBookmark");
    }
}

