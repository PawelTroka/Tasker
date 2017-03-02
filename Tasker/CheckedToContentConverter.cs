using System;
using System.Globalization;
using System.Windows.Data;

namespace Tasker
{
    [ValueConversion(typeof (bool), typeof (String))]
    public class CheckedToContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool) value)
                return "★";
            return "☆";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string) value == "★")
                return true;
            return false;
        }
    }
}