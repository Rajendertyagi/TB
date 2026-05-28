
using System;
using System.Collections.Generic;

namespace RTBrowser.Services
{
    public sealed class SearchProviderManager
    {
        private readonly Dictionary<string, string> _providers;

        public string CurrentProvider { get; private set; }

        public SearchProviderManager()
        {
            _providers = new Dictionary<string, string>
            {
                {
                    "Google",
                    "https://www.google.com/search?q={0}"
                },
                {
                    "DuckDuckGo",
                    "https://duckduckgo.com/?q={0}"
                },
                {
                    "Bing",
                    "https://www.bing.com/search?q={0}"
                },
                {
                    "Brave",
                    "https://search.brave.com/search?q={0}"
                },
                {
                    "Startpage",
                    "https://www.startpage.com/search?q={0}"
                }
            };

            CurrentProvider = "Google";
        }

        public IReadOnlyDictionary<string, string> Providers
            => _providers;

        public void SetProvider(string providerName)
        {
            if (!_providers.ContainsKey(providerName))
                return;

            CurrentProvider = providerName;
        }

        public string BuildSearchUrl(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return "about:blank";

            string encodedQuery =
                Uri.EscapeDataString(query);

            string template =
                _providers[CurrentProvider];

            return string.Format(
                template,
                encodedQuery);
        }

        public bool IsUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            return Uri.TryCreate(
                input,
                UriKind.Absolute,
                out _);
        }

        public string ResolveInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "about:blank";

            input = input.Trim();

            if (IsUrl(input))
                return input;

            if (input.Contains(".") &&
                !input.Contains(" "))
            {
                return $"https://{input}";
            }

            return BuildSearchUrl(input);
        }
    }
}
