namespace TB.Services.InternalPages;

public interface IInternalPageService
{
    bool TryGetPage(
        string url,
        out string html);
}