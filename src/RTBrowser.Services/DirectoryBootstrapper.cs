using System.IO;

namespace RTBrowser.Services
{
    public static class DirectoryBootstrapper
    {
        public static void Initialize()
        {
            Create(
                BrowserPaths.Logs);

            Create(
                BrowserPaths.State);

            Create(
                BrowserPaths.Cache);

            Create(
                BrowserPaths.Sessions);

            Create(
                BrowserPaths.WebViewData);

            Create(
                BrowserPaths.Downloads);
        }

        private static void Create(
            string path)
        {
            if (Directory.Exists(path))
            {
                return;
            }

            Directory.CreateDirectory(path);
        }
    }
}
