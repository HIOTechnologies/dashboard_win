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
    /// Interaction logic for TSearchbox.xaml
    /// </summary>
    public partial class TSearchbox : UserControl
    {
        public TSearchbox()
        {
            InitializeComponent();
        }

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(TSearchbox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));




        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(TSearchbox), new PropertyMetadata("Search"));





        private void Button_Click(object sender, RoutedEventArgs e)
        {
            txt.Text = null;
        }
    }
    public class TextLengthToImageConverter : MarkupExtension, IValueConverter
    {
        public ImageSource Empty { get; set; }
        public ImageSource Full { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                string str = (string)value;
                if (str.IsNullOrEmpty())
                {
                    return Empty;
                }
                else
                {
                    return Full;
                }
            }
            return Empty;
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
