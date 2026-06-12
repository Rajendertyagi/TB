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

        if (Application.Current.Resources.TryGetValue(brushKey, out var brush) && brush is SolidColorBrush sb)
            return sb;

        return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
