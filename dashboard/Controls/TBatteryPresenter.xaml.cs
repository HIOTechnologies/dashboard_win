using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HIO.Controls
{
    /// <summary>
    /// Interaction logic for TBatteryPresenter.xaml
    /// </summary>
    public partial class TBatteryPresenter : UserControl
    {
        public TBatteryPresenter()
        {
            InitializeComponent();
        }


        public BatteryStateEnum BatteryValue
        {
            get { return (BatteryStateEnum)GetValue(BatteryValueProperty); }
            set { SetValue(BatteryValueProperty, value); }
        }

        public static readonly DependencyProperty BatteryValueProperty =
            DependencyProperty.Register("BatteryValue", typeof(BatteryStateEnum), typeof(TBatteryPresenter), new PropertyMetadata(BatteryStateEnum.Empty));


    }
    public class TBatteryValueConverter : MarkupExtension, IValueConverter
    {
        //public string BaseUrl { get; set; } = "pack://application:,,,/HIO;component/Resources/Battery/Battery";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BatteryStateEnum)
            {
                return TEnumImageUriAttribute.GetImageUri((BatteryStateEnum)value);
            }
            return TEnumImageUriAttribute.GetImageUri(BatteryStateEnum.Empty);
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

    public enum BatteryStateEnum
    {
        [TEnumImageUri("pack://application:,,,/HIO;component/Resources/Battery/Empty Battery_32px.png")]
        Empty = 0,
        [TEnumImageUri("pack://application:,,,/HIO;component/Resources/Battery/Low Battery_32px.png")]
        Low = 1,
        [TEnumImageUri("pack://application:,,,/HIO;component/Resources/Battery/Battery Level_32px.png")]
        Medium = 2,
        [TEnumImageUri("pack://application:,,,/HIO;component/Resources/Battery/Charged Battery_32px.png")]
        Charged = 3,
        [TEnumImageUri("pack://application:,,,/HIO;component/Resources/Battery/Full Battery_32px.png")]
        Full = 4,
        /*[TEnumImageUri("pack://application:,,,/HIO;component/Resources/Battery/Charging Battery_32px.png")]
        Charging = 5,*/
    }
}
