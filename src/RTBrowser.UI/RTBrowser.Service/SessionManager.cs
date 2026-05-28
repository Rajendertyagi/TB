using RTBrowser.Runtime;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace RTBrowser.Services
{
    public sealed class SessionManager
    {
        private readonly string _sessionDirectory;

        private readonly string _sessionFilePath;

        public SessionManager()
        {
            _sessionDirectory = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Session");

            Directory.CreateDirectory(_sessionDirectory);

            _sessionFilePath = Path.Combine(
                _sessionDirectory,
                "session.json");
        }

        public void SaveSession(IEnumerable<TabRuntime> tabs)
        {
            try
            {
                var sessionTabs = new List<SessionTab>();

                foreach (var tab in tabs)
                {
                    sessionTabs.Add(new SessionTab
                    {
                        Id = tab.Id,
                        Title = tab.Title,
                        Url = tab.Url,
                        IsPinned = tab.IsPinned
                    });
                }

                var json = JsonSerializer.Serialize(
                    sessionTabs,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                File.WriteAllText(_sessionFilePath, json);
            }
            catch
            {
                // Prevent session failures from crashing runtime
            }
        }

        public List<SessionTab> LoadSession()
        {
            try
            {
                if (!File.Exists(_sessionFilePath))
                    return new List<SessionTab>();

                var json = File.ReadAllText(_sessionFilePath);

                var tabs = JsonSerializer.Deserialize<List<SessionTab>>(json);

                return tabs ?? new List<SessionTab>();
            }
            catch
            {
                return new List<SessionTab>();
            }
        }
    }

    public sealed class SessionTab
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = "New Tab";

        public string Url { get; set; } = "about:blank";

        public bool IsPinned { get; set; }
    }
}
