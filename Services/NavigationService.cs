using TB.Helpers;
using TB.Services.Interfaces;

namespace TB.Services;

public class NavigationService : INavigationService
{
    private readonly ITabManager _tabManager;

    public NavigationService(ITabManager tabManager)
    {
        _tabManager = tabManager;
    }

    public void Navigate(string url)
    {
        _tabManager.NavigateActiveTab(url);
    }

    public void GoBack()
    {
        _tabManager.Back();
    }

    public void GoForward()
    {
        _tabManager.Forward();
    }

    public void Reload()
    {
        _tabManager.Reload();
    }

    public void Stop()
    {
        _tabManager.Stop();
    }

    public string ResolveUrl(string input)
    {
        return UrlResolver.ResolveOmnibar(input);
    }
}
