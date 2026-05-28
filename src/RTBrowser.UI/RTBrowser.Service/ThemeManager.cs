
using System;
using System.Collections.Generic;
using System.Windows;

namespace RTBrowser.Services
{
    public sealed class ThemeManager
    {
        private readonly Dictionary<string, Uri> _themes;

        public string CurrentTheme { get; private set; }

        public ThemeManager()
        {
            _themes = new Dictionary<string, Uri>
            {
                {
                    "DarkGraphite",
                    new Uri(
                        "/RTBrowser.UI;component/Themes/Colors.xaml",
                        UriKind.Relative)
                }
            };

            CurrentTheme = "DarkGraphite";
        }

        public IReadOnlyDictionary<string, Uri> Themes
            => _themes;

        public void ApplyTheme(string themeName)
        {
            if (!_themes.ContainsKey(themeName))
                return;

            var application = Application.Current;

            if (application == null)
                return;

            var dictionaries =
                application.Resources.MergedDictionaries;

            for (int i = dictionaries.Count - 1; i >= 0; i--)
            {
                var source = dictionaries[i].Source?.ToString();

                if (source == null)
                    continue;

                if (source.Contains("Colors.xaml"))
                {
                    dictionaries.RemoveAt(i);
                }
            }

            dictionaries.Add(
                new ResourceDictionary
                {
                    Source = _themes[themeName]
                });

            CurrentTheme = themeName;
        }

        public void RegisterTheme(
            string themeName,
            Uri resourceUri)
        {
            if (_themes.ContainsKey(themeName))
                return;

            _themes.Add(themeName, resourceUri);
        }
    }
}
