using System.Linq;
using CommunityToolkit.Mvvm.Input;
using TB.Models;

namespace TB.ViewModels;

public sealed partial class BrowserViewModel
{
    [RelayCommand]
    private void AddBookmark()
    {
        if (ActiveTab is null)
        {
            return;
        }

        var bookmark = new Bookmark
        {
            Title = ActiveTab.Title,
            Url = ActiveTab.Address
        };

        Bookmarks.Add(bookmark);

        _bookmarkService.Save(
            Bookmarks.ToList());
    }

    [RelayCommand]
    private void RemoveBookmark(
        Bookmark? bookmark)
    {
        if (bookmark is null)
        {
            return;
        }

        Bookmarks.Remove(
            bookmark);

        _bookmarkService.Save(
            Bookmarks.ToList());
    }
}