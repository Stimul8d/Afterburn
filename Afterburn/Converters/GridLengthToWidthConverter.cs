using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Afterburn.Converters
{
    public class GridLengthToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var gl = (GridLength)value;
            return gl.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
