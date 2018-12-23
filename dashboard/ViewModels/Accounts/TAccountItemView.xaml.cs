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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HIO.ViewModels.Accounts
{
    /// <summary>
    /// Interaction logic for TAccountItemView.xaml
    /// </summary>
    public partial class TAccountItemView : UserControl
    {
        public TAccountItemView()
        {
            InitializeComponent();
        }
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            DoubleAnimation DA_FontSize = new DoubleAnimation(Txt_MainTitle.FontSize, new Duration(TimeSpan.FromMilliseconds(300)));
            Txt_SubTitle1.BeginAnimation(TextBlock.FontSizeProperty, DA_FontSize);

            TBrushAnimation DA_Foreground = new TBrushAnimation(Txt_MainTitle.Foreground, new Duration(TimeSpan.FromMilliseconds(300)));
            Txt_SubTitle1.BeginAnimation(TextBlock.ForegroundProperty, DA_Foreground);

            DoubleAnimation DA_CanvasTop = new DoubleAnimation(Txt_MainTitle.ActualHeight * -1, new Duration(TimeSpan.FromMilliseconds(300)));
            Grd_Animator.BeginAnimation(Canvas.TopProperty, DA_CanvasTop);

            DoubleAnimation DA_Opacity = new DoubleAnimation(1, new Duration(TimeSpan.FromMilliseconds(300)));
            Txt_SubTitleContainer.BeginAnimation(OpacityProperty, DA_Opacity);
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            DoubleAnimation DA_FontSize = new DoubleAnimation(Txt_SubTitle2.FontSize, new Duration(TimeSpan.FromMilliseconds(300)));
            Txt_SubTitle1.BeginAnimation(TextBlock.FontSizeProperty, DA_FontSize);

            TBrushAnimation DA_Foreground = new TBrushAnimation(Txt_SubTitle2.Foreground, new Duration(TimeSpan.FromMilliseconds(300)));
            Txt_SubTitle1.BeginAnimation(TextBlock.ForegroundProperty, DA_Foreground);

            DoubleAnimation DA_CanvasTop = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(300)));
            Grd_Animator.BeginAnimation(Canvas.TopProperty, DA_CanvasTop);

            DoubleAnimation DA_Opacity = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(300)));
            Txt_SubTitleContainer.BeginAnimation(OpacityProperty, DA_Opacity);
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

        private void Img_Context_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Cmnu_Main.IsOpen = false;
        }

        //private CustomPopupPlacement[] Test(Size popupSize, Size targetSize, Point offset)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
