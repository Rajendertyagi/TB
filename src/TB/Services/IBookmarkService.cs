using TB.Models;

namespace TB.Services;

public interface IBookmarkService
{
    IReadOnlyList<Bookmark> Load();

    void Save(IReadOnlyList<Bookmark> bookmarks);
}