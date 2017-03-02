using System;
using System.Globalization;
using System.Windows.Data;

namespace Tasker
{
    [ValueConversion(typeof (long), typeof (double))]
    internal class BytesToMBytesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double bytes = (long) value;
            double mbytes = (bytes/1024.0)/1024.0;
            return mbytes;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mbytes = (double) value;
            long bytes = (long) (mbytes)*1024*1024;
            return bytes;
        }
    }

    [ValueConversion(typeof (int), typeof (double))]
    internal class IntBytesToMBytesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double bytes = (int) value;
            double mbytes = (bytes/1024.0)/1024.0;
            return mbytes;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mbytes = (double) value;
            long bytes = (int) (mbytes)*1024*1024;
            return bytes;
        }
    }
}