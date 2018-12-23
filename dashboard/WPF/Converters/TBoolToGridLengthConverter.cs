﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace HIO.Controls
{
    public class TBoolToGridLengthConverter : MarkupExtension, IValueConverter
    {
        public GridLength TruePart { get; set; }
        public GridLength FalsePart { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                bool Final = (bool)value;
                if (Final)
                {
                    return TruePart;
                }
                else
                {
                    return FalsePart;
                }
            }
            return TruePart;
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
