using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace HIO.Controls
{
    public class TWindowStateToImageConverter : MarkupExtension, IValueConverter
    {
        public ImageSource MaximizeImage { get; set; }
        public ImageSource NormalImage { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WindowState)
            {
                WindowState ws = (WindowState)value;
                if (ws == WindowState.Normal)
                {
                    return NormalImage;
                }
                else
                {
                    return MaximizeImage;
                }
            }
            return NormalImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
