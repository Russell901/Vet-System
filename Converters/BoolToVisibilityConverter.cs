using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Globalization;

namespace Vet_System.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (Visibility)value == Visibility.Visible;
        }

        // Explicit interface implementation with the correct signature
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, targetType, parameter, language);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ConvertBack(value, targetType, parameter, language);
        }
    }

    public class BoolToVisibilityInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (Visibility)value != Visibility.Visible;
        }

        // Explicit interface implementation with the correct signature
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, targetType, parameter, language);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ConvertBack(value, targetType, parameter, language);
        }
    }
}