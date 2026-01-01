using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BlitzTroubleshooter.Converters;

public class CollectionEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ICollection collection)
        {
            return collection.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        
        if (value is int count)
        {
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
