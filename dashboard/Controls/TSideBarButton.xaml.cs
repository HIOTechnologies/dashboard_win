using HIO.ViewModels;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HIO.Controls
{
    /// <summary>
    /// Interaction logic for TSideBarButton.xaml
    /// </summary>
    public partial class TSideBarButton : UserControl
    {
        public TSideBarButton()
        {
            InitializeComponent();
            this.Loaded += TSideBarButton_Loaded;
        }



        #region Properties
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(TSideBarButton), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsExpandedChanged));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(TSideBarButton), new PropertyMetadata(null));

        public Brush HoverBrush
        {
            get { return (Brush)GetValue(HoverBrushProperty); }
            set { SetValue(HoverBrushProperty, value); }
        }

        public static readonly DependencyProperty HoverBrushProperty =
            DependencyProperty.Register("HoverBrush", typeof(Brush), typeof(TSideBarButton), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(147, 185, 194))));

        public ImageSource NormalImage
        {
            get { return (ImageSource)GetValue(NormalImageProperty); }
            set { SetValue(NormalImageProperty, value); }
        }

        public static readonly DependencyProperty NormalImageProperty =
            DependencyProperty.Register("NormalImage", typeof(ImageSource), typeof(TSideBarButton), new PropertyMetadata(null, OnNormalImageChanged));

        public ImageSource HoverImage
        {
            get { return (ImageSource)GetValue(HoverImageProperty); }
            set { SetValue(HoverImageProperty, value); }
        }

        public static readonly DependencyProperty HoverImageProperty =
            DependencyProperty.Register("HoverImage", typeof(ImageSource), typeof(TSideBarButton), new PropertyMetadata(null));



        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(TSideBarButton), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSelectedChanged));


        #endregion

        private void TSideBarButton_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateIsExpandedView();
            UpdateIsSelectedView();
        }
        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TSideBarButton SBB = (TSideBarButton)d;
            if (SBB != null)
            {
                if (SBB.IsSelected && !SBB.IsKeyboardFocusWithin) SBB.Focus();
                SBB.UpdateIsSelectedView();
            }
        }
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            IsSelected = true;
        }
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            IsSelected = false;
        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!IsSelected)
            {
                IsSelected = true;
                e.Handled = true;
            }
            base.OnMouseDown(e);
        }


        private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TSideBarButton).UpdateIsExpandedView();
        }


        private static void OnNormalImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TSideBarButton).UpdateIsExpandedView();
        }

        private void UpdateIsExpandedView()
        {
            if (IsExpanded)
            {
                Grd_InnerContainer.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                Grd_InnerContainer.HorizontalAlignment = HorizontalAlignment.Center;
            }
        }
        public void UpdateIsSelectedView()
        {
            if (IsSelected)
            {
                SetImageUrl(HoverImage);
                LeftBorder.Visibility = Visibility.Visible;
            }
            else
            {
                SetImageUrl(NormalImage);
                LeftBorder.Visibility = Visibility.Collapsed;
            }
        }

        void SetImageUrl(ImageSource img)
        {
            if (img != null) Img_Main.Source = img;
        }



    }
    
}
