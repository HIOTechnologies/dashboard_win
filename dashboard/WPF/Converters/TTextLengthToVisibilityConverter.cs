using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace HIO.Controls
{
    public class TTextLengthToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public bool Reverse { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool hasText = value != null && value is string && ((string)value).Length > 0;
            if (Reverse) hasText = !hasText;
            if (hasText)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
