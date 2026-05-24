using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace TB
{
    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isActive = value is bool b && b;
            // Active tabs blend with the Omnibox (#252525), inactive ones stay dark (#2D2D2D)
            var hex = isActive ? Color.FromArgb(255, 37, 37, 37) : Color.FromArgb(255, 45, 45, 45);
            return new SolidColorBrush(hex);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
