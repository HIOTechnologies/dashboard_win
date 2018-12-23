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

namespace HIO.Extentions
{
    /// <summary>
    /// Interaction logic for TLinkItemView.xaml
    /// </summary>
    public partial class TLinkItemView : UserControl
    {
        public TLinkItemView()
        {
            InitializeComponent();
            Loaded += TLinkItemView_Loaded;
        }

        private void TLinkItemView_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateAll();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Cmnu_Main.PlacementTarget = Img_Context;
            Cmnu_Main.Placement = PlacementMode.Relative;
            Cmnu_Main.HorizontalOffset = -135;
            Cmnu_Main.VerticalOffset = -20;
            //Cmnu_Main.off = PlacementMode.Custom;
            //Cmnu_Main.CustomPopupPlacementCallback = Test;
            Cmnu_Main.IsOpen = true;
        }


        public ICommand LaunchCommand
        {
            get { return (ICommand)GetValue(LaunchCommandProperty); }
            set { SetValue(LaunchCommandProperty, value); }
        }

        public static readonly DependencyProperty LaunchCommandProperty =
            DependencyProperty.Register("LaunchCommand", typeof(ICommand), typeof(TLinkItemView), new PropertyMetadata(null, OnLaunchCommandChanged));

        private static void OnLaunchCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TLinkItemView).UpdateAll();
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(TLinkItemView), new PropertyMetadata(false));



        public bool ShowCommand
        {
            get { return (bool)GetValue(ShowCommandProperty); }
            set { SetValue(ShowCommandProperty, value); }
        }

        public static readonly DependencyProperty ShowCommandProperty =
            DependencyProperty.Register("ShowCommand", typeof(bool), typeof(TLinkItemView), new PropertyMetadata(true));

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            UpdateAll();
            //Btn_Launch.CaptureMouse();
            //e.Handled = true;
        }

        private void Root_MouseLeave(object sender, MouseEventArgs e)
        {
            UpdateAll();
            //Btn_Launch.ReleaseMouseCapture();
            //e.Handled = true;
        }

        private void UpdateAll()
        {
            if (IsMouseOver)
            {
                Brd_Hilighter.Opacity = 1;
            }
            else
            {
                Brd_Hilighter.Opacity = 0;
            }
            if (IsMouseOver && LaunchCommand != null)
            {
                Btn_Launch.Opacity = 1;

                Txt_Description.Opacity = 0;
                
            }
            else
            {
                Btn_Launch.Opacity = 0;
                Txt_Description.Opacity = 1;
            }

        }


    }
}
