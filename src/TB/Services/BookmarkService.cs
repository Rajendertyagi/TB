using System.IO;
using System.Text.Json;
using TB.Models;

namespace TB.Services;

public sealed class BookmarkService : IBookmarkService
{
    private const string BookmarksFile = "Settings/bookmarks.json";

    public IReadOnlyList<Bookmark> Load()
    {
        if (!File.Exists(BookmarksFile))
        {
            return [];
        }

        var json = File.ReadAllText(BookmarksFile);

        return JsonSerializer.Deserialize<List<Bookmark>>(json)
               ?? [];
    }

    public void Save(IReadOnlyList<Bookmark> bookmarks)
    {
        Directory.CreateDirectory("Settings");

        var json = JsonSerializer.Serialize(
            bookmarks,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(BookmarksFile, json);
    }
}