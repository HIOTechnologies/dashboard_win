using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace HIO.Setup
{
    /// <summary>
    /// Interaction logic for TSetupPage2View.xaml
    /// </summary>
    public partial class TSetupPage2View : UserControl
    {
        public TSetupPage2View()
        {
            InitializeComponent();
            var path = "pack://application:,,,/HIO;component/Resources/SetupWizard/{0}";
            if (SystemParameters.PrimaryScreenWidth < 1400)
                Image.Source = new BitmapImage(new Uri(string.Format(path, "2_1024.png")));
            else
                Image.Source = new BitmapImage(new Uri(string.Format(path, "2_1366.png")));
        }
    }
}
