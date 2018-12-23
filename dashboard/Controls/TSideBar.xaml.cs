using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HIO.Controls
{
    /// <summary>
    /// Interaction logic for TSideBar.xaml
    /// </summary>
    public partial class TSideBar : UserControl
    {
        public TSideBar()
        {
            InitializeComponent();
        }


        #region Fields
        double ExpandWidth = 180;
        double CollapseWidth = 80;
        private bool _IsAnimating = false;
        #endregion

        #region Properties
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(TSideBar), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsExpandedChanged));



        public bool IsConnected
        {
            get { return (bool)GetValue(IsConnectedProperty); }
            set { SetValue(IsConnectedProperty, value); }
        }

        public static readonly DependencyProperty IsConnectedProperty =
            DependencyProperty.Register("IsConnected", typeof(bool), typeof(TSideBar), new PropertyMetadata(false));



        #endregion


        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateIsExpanded();
        }

        private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TSideBar).UpdateIsExpanded();
        }

        private void UpdateIsExpanded()
        {
            _IsAnimating = true;

            if (IsExpanded)
            {
                Img_HIO_Container.Width = new GridLength(1, GridUnitType.Star);
                Grd_Logo_Container.HorizontalAlignment = HorizontalAlignment.Left;
                DoubleAnimation DA = new DoubleAnimation(ExpandWidth, new Duration(TimeSpan.FromMilliseconds(200)));
                DA.FillBehavior = FillBehavior.HoldEnd;
                DA.Completed += (a, b) => { _IsAnimating = false; };
                BeginAnimation(WidthProperty, DA);
                //AnimateOpacity(Img_Logo_Collapsed, 0);
                //AnimateOpacity(Img_Logo_Expanded, 1);
            }
            else
            {
                //Img_HIO.Visibility = Visibility.Collapsed;
                DoubleAnimation DA = new DoubleAnimation(CollapseWidth, new Duration(TimeSpan.FromMilliseconds(200)));
                DA.FillBehavior = FillBehavior.HoldEnd;
                DA.Completed += (a, b) =>
                {
                    Img_HIO_Container.Width = new GridLength(0);
                    Grd_Logo_Container.HorizontalAlignment = HorizontalAlignment.Center;
                    _IsAnimating = false;
                };
                this.BeginAnimation(WidthProperty, DA);
                //AnimateOpacity(Img_Logo_Collapsed, 1);
                //AnimateOpacity(Img_Logo_Expanded, 0);
            }
        }

        private void AnimateOpacity(FrameworkElement ctrl, double toValue)
        {
            DoubleAnimation DA1 = new DoubleAnimation(toValue, new Duration(TimeSpan.FromMilliseconds(300)));
            DA1.FillBehavior = FillBehavior.HoldEnd;
            ctrl.BeginAnimation(OpacityProperty, DA1);
        }
        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_IsAnimating) return;
            IsExpanded = !IsExpanded;
        }

        #endregion


    }
}
