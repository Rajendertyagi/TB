using RTBrowser.Core;
using RTBrowser.Services;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RTBrowser.Runtime
{
    public static class TabPersistenceService
    {
        private static readonly string _sessionFile =
            Path.Combine(
                BrowserPaths.Sessions,
                "tabs.json");

        public static void Save(
            IReadOnlyList<TabSession> sessions)
        {
            try
            {
                List<PersistedTab> tabs =
                    sessions
                        .Select(
                            x =>
                                new PersistedTab
                                {
                                    Url = x.Tab.Url,
                                    Title = x.Tab.Title,
                                    IsActive = x.Tab.IsActive
                                })
                        .ToList();

                string json =
                    JsonSerializer.Serialize(
                        tabs,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });

                File.WriteAllText(
                    _sessionFile,
                    json);
            }
            catch
            {
            }
        }

        public static List<PersistedTab> Load()
        {
            try
            {
                if (!File.Exists(_sessionFile))
                {
                    return new List<PersistedTab>();
                }

                string json =
                    File.ReadAllText(
                        _sessionFile);

                List<PersistedTab>? tabs =
                    JsonSerializer.Deserialize<List<PersistedTab>>(
                        json);

                return
                    tabs
                    ??
                    new List<PersistedTab>();
            }
            catch
            {
                return new List<PersistedTab>();
            }
        }

        public sealed class PersistedTab
        {
            public string Url { get; set; } =
                Constants.HomeUrl;

            public string Title { get; set; } =
                "New Tab";

            public bool IsActive { get; set; }
        }
    }
}
