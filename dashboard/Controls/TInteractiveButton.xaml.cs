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
    /// Interaction logic for TInteractiveButton.xaml
    /// </summary>
    public partial class TInteractiveButton : UserControl
    {
        public TInteractiveButton()
        {
            InitializeComponent();
        }


        public ImageSource NormalImage
        {
            get { return (ImageSource)GetValue(NormalImageProperty); }
            set { SetValue(NormalImageProperty, value); }
        }

        public static readonly DependencyProperty NormalImageProperty =
            DependencyProperty.Register("NormalImage", typeof(ImageSource), typeof(TInteractiveButton), new PropertyMetadata(null, OnNormalImageChanged));

        private static void OnNormalImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TInteractiveButton).SetImageUrl();
        }

        private void SetImageUrl()
        {
            SetImageUrl(NormalImage);
        }

        public ImageSource HoverImage
        {
            get { return (ImageSource)GetValue(HoverImageProperty); }
            set { SetValue(HoverImageProperty, value); }
        }

        public static readonly DependencyProperty HoverImageProperty =
            DependencyProperty.Register("HoverImage", typeof(ImageSource), typeof(TInteractiveButton), new PropertyMetadata(null));

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            SetImageUrl(HoverImage);
            if (MouseOverAffectOpacity)
            {
                Opacity = 1;
            }
        }
        void SetImageUrl(ImageSource img)
        {
            if (img != null) Img_Main.Source = img;
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            SetImageUrl(NormalImage);
            if (MouseOverAffectOpacity)
            {
                Opacity = 0.5;
            }
        }
        //protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        //{
        //    base.OnPreviewMouseLeftButtonUp(e);
        //}
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (!e.Handled) RaiseClickEvent();

            base.OnMouseLeftButtonUp(e);
        }



        public bool MouseOverAffectOpacity
        {
            get { return (bool)GetValue(MouseOverAffectOpacityProperty); }
            set { SetValue(MouseOverAffectOpacityProperty, value); }
        }

        public static readonly DependencyProperty MouseOverAffectOpacityProperty =
            DependencyProperty.Register("MouseOverAffectOpacity", typeof(bool), typeof(TInteractiveButton), new PropertyMetadata(false));



        #region Click Routed Event

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
            "Click",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TInteractiveButton));

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        /// <summary>
        /// Invoke this method when you wish to raise a(n) Click event
        /// </summary>
        protected virtual void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(ClickEvent);
            RaiseEvent(newEventArgs);
        }

        #endregion
    }
}
