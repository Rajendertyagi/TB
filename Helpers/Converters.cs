using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace TB.Helpers;

public partial class BoolToActiveBgConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isActive = value is bool b && b;
        string brushKey = isActive ? "bgTabActiveBrush" : "bgTabBrush";

        // INDUSTRY STANDARD: Dynamically fetch from the currently active App/OS theme.
        // This ensures the UI doesn't break if the user switches to Light mode or a custom theme.
        string currentTheme = Application.Current.RequestedTheme.ToString();

        if (Application.Current.Resources.ThemeDictionaries.TryGetValue(currentTheme, out var themeObj)
            && themeObj is ResourceDictionary dict
            && dict.TryGetValue(brushKey, out var brush))
        {
            return brush;
        }

        // Absolute fallback if XAML resources haven't loaded yet on startup
        return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public partial class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool boolVal = value is bool b && b;

        // Support for "invert" parameter (e.g., hide when true, show when false)
        if (parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase))
            boolVal = !boolVal;

        return boolVal ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => value is Visibility v && v == Visibility.Visible;
}

