namespace TB.Services.Interfaces;

public interface INavigationService
{
    void Navigate(string url);
    void GoBack();
    void GoForward();
    void Reload();
    void Stop();
    string ResolveUrl(string input);
}
