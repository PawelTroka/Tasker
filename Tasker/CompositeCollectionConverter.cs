﻿using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace TopSmartphones
{
    public class CompositeCollectionConverter : IMultiValueConverter
    {
        public object Convert(object[] values
            , Type targetType
            , object parameter
            , CultureInfo culture)
        {
            var res = new CompositeCollection();
            foreach (object item in values)
                if (item is IEnumerable)
                    res.Add(new CollectionContainer
                    {
                        Collection = item as IEnumerable
                    });
                else res.Add(item);
            return res;
        }

        public object[] ConvertBack(object value
            , Type[] targetTypes
            , object parameter
            , CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}