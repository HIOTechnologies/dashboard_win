using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace HIO.Controls
{
    /// <summary>
    /// Interaction logic for TSignalPresenter.xaml
    /// </summary>
    public partial class TSignalPresenter : UserControl
    {
        public TSignalPresenter()
        {
            InitializeComponent();
        }


        public SignalEnum SignalValue
        {
            get { return (SignalEnum)GetValue(SignalValueProperty); }
            set { SetValue(SignalValueProperty, value); }
        }

        public static readonly DependencyProperty SignalValueProperty =
            DependencyProperty.Register("SignalValue", typeof(SignalEnum), typeof(TSignalPresenter), new PropertyMetadata(SignalEnum.NoConnection));


    }
    public class TSignalValueConverter : MarkupExtension, IValueConverter
    {
        public string BaseUrl { get; set; } = "pack://application:,,,/HIO;component/Resources/Signal/signal";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SignalEnum)
            {
                return TEnumImageUriAttribute.GetImageUri((SignalEnum)value);
            }
            return TEnumImageUriAttribute.GetImageUri(SignalEnum.NoConnection);
            
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
    public enum SignalEnum
    {
        [TEnumImageUri("pack://application:,,,/HIO;component/Resources/signal/No Connection_32px.png")]
        NoConnection = 0,
        [TEnumImageUri("pack://application:,,,/HIO;component/Resources/signal/Low Connection_32px.png")]
        Low = 1,
        [TEnumImageUri("pack://application:,,,/HIO;component/Resources/signal/Signal2_32px.png")]
        Medium = 2,
        [TEnumImageUri("pack://application:,,,/HIO;component/Resources/signal/Signal_32px.png")]
        Full = 3
    }
}
