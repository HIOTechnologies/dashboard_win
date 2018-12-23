using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HIO.ViewModels.Settings
{
    /// <summary>
    /// Interaction logic for TDeviceView.xaml
    /// </summary>
    public partial class TDeviceView : UserControl
    {
        public TDeviceView()
        {
            InitializeComponent();
        }

        private void Btn_Forget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Cmnu_Forget.PlacementTarget = Btn_Forget;
            Cmnu_Forget.Placement = PlacementMode.Relative;
            Cmnu_Forget.HorizontalOffset = -50;
            Cmnu_Forget.VerticalOffset = -90;
            Cmnu_Forget.HorizontalAlignment = HorizontalAlignment.Center;
            Cmnu_Forget.VerticalAlignment = VerticalAlignment.Top;

            //Cmnu_Main.off = PlacementMode.Custom;
            //Cmnu_Main.CustomPopupPlacementCallback = Test;
            Cmnu_Forget.IsOpen = true;
            e.Handled = true;
        }
    }
}
