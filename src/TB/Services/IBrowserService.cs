namespace TB.Services;

public interface IBrowserService
{
    void Navigate(string url);
    void GoBack();
    void GoForward();
    void Refresh();
}
