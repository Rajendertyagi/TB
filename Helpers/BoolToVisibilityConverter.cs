using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace TB.Helpers;

public partial class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool boolVal = value is bool b && b;

        if (parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase))
            boolVal = !boolVal;

        return boolVal ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => value is Visibility v && v == Visibility.Visible;
}
