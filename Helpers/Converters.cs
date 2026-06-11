using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace TB.Helpers;

public partial class BoolToActiveBgConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var themeKey = (value is bool isActive && isActive) ? "bgTabActiveBrush" : "bgTabBrush";
        return ((ResourceDictionary)Application.Current.Resources.ThemeDictionaries["Dark"])[themeKey] as SolidColorBrush
               ?? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public partial class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool boolVal = value is bool b && b;
        if (parameter is string s && s == "invert")
            boolVal = !boolVal;
        return boolVal ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => value is Visibility v && v == Visibility.Visible;
}
