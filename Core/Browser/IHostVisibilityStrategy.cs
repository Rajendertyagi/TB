namespace TB.Core.Browser;

public interface IHostVisibilityStrategy
{
    void Apply(WebViewHost host, bool isActive);
}