using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace HIO.Controls
{
    public class TWindow : Window
    {
        public virtual bool canCloseonDC
        {

            get { return true; }

        }
        static TWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TWindow), new FrameworkPropertyMetadata(typeof(TWindow)));
        }
        public TWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            var mainVindow = Application.Current.MainWindow as TMainView;
            if ((mainVindow?.IsActive ?? false) && !(this is TMainView) && !IsExtensionWindow && !mainVindow.IsExtensionWindow)
                Owner = mainVindow;
        }


        #region Properties

        private bool IsExtensionWindow
        {
            get
            {
                return Backend.HIOStaticValues.AdminExtention.AllExtentionsViewTypes.Any(t => t == GetType());
            }
        }
        //public bool AllowResize
        //{
        //    get { return (bool)GetValue(AllowResizeProperty); }
        //    set { SetValue(AllowResizeProperty, value); }
        //}

        //public static readonly DependencyProperty AllowResizeProperty =
        //    DependencyProperty.Register("AllowResize", typeof(bool), typeof(TWindow), new PropertyMetadata(true));

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register("IsBusy", typeof(bool), typeof(TWindow), new PropertyMetadata(false));


        public bool AllowShowToolbox
        {
            get { return (bool)GetValue(AllowShowToolboxProperty); }
            set { SetValue(AllowShowToolboxProperty, value); }
        }

        public static readonly DependencyProperty AllowShowToolboxProperty =
            DependencyProperty.Register("AllowShowToolbox", typeof(bool), typeof(TWindow), new PropertyMetadata(true));

        public bool AllowShowMinimize
        {
            get { return (bool)GetValue(AllowShowMinimizeProperty); }
            set { SetValue(AllowShowMinimizeProperty, value); }
        }

        public static readonly DependencyProperty AllowShowMinimizeProperty =
            DependencyProperty.Register("AllowShowMinimize", typeof(bool), typeof(TWindow), new PropertyMetadata(false));



        public bool AllowShowMaximizeRestore
        {
            get { return (bool)GetValue(AllowShowMaximizeRestoreProperty); }
            set { SetValue(AllowShowMaximizeRestoreProperty, value); }
        }

        public static readonly DependencyProperty AllowShowMaximizeRestoreProperty =
            DependencyProperty.Register("AllowShowMaximizeRestore", typeof(bool), typeof(TWindow), new PropertyMetadata(false));



        public bool AllowShowClose
        {
            get { return (bool)GetValue(AllowShowCloseProperty); }
            set { SetValue(AllowShowCloseProperty, value); }
        }

        public static readonly DependencyProperty AllowShowCloseProperty =
            DependencyProperty.Register("AllowShowClose", typeof(bool), typeof(TWindow), new PropertyMetadata(true));

        #endregion

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            try
            {
                var originalSource = ((FrameworkElement)e.OriginalSource);
                if (!e.Handled &&
                    !Keyboard.IsKeyDown(Key.RightCtrl) &&
                    !Keyboard.IsKeyDown(Key.LeftCtrl) &&
                    e.ChangedButton == MouseButton.Left &&
                    (!(originalSource is Image img) || img.Name != "Img_Logo") &&
                    !(originalSource?.Parent is TInteractiveButton))
                {
                    DragMove();
                }
            }
            catch { }
            finally
            {
                e.Handled = false;
                base.OnMouseLeftButtonDown(e);
            }
        }
        //protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        //{
        //    try
        //    {
        //        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        //            DragMove();
        //    }
        //    catch { }
        //    finally
        //    {
        //        e.Handled = false;
        //        base.OnPreviewMouseDown(e);
        //    }
        //}
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //ResizeMode = ResizeMode.
        }
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            Grid grd = (Grid)GetTemplateChild("MainContainer");
            if (WindowState == WindowState.Maximized)
            {
                grd.Margin = new Thickness(0);
            }
            else if (WindowState == WindowState.Normal)
            {
                grd.Margin = new Thickness(20);
            }
        }
    }
    public class TResizeModeToVisibility : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ResizeMode)
            {
                ResizeMode R = (ResizeMode)value;
                if (R == ResizeMode.CanResize || R == ResizeMode.CanResizeWithGrip)
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
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
    public static class WindowResizeBehavior
    {
        public static Window GetResize(DependencyObject obj)
        {
            return (Window)obj.GetValue(Resize);
        }

        public static void SetResize(DependencyObject obj, Window window)
        {
            obj.SetValue(Resize, window);
        }

        public static readonly DependencyProperty Resize = DependencyProperty.RegisterAttached("Resize",
            typeof(Window), typeof(WindowResizeBehavior),
            new UIPropertyMetadata(null, OnResizeChanged));

        private static void OnResizeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var thumb = sender as Thumb;

            if (thumb != null)
            {
                thumb.DragDelta += Thumb_DragDelta;
                thumb.DragCompleted += DragCompleted;
            }
        }

        private static void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            var window = thumb.GetValue(Resize) as Window;

            var deltaX = e.HorizontalChange;
            var deltaY = e.VerticalChange;
            var minWidth = window.MinWidth;
            var minHeight = window.MinHeight;

            if (deltaY != 0 && (thumb.Name.ToLower().Contains("top") || thumb.Name.ToLower().Contains("bottom")))
            {
                var newHeight = window.Height;
                if (thumb.Name.ToLower().Contains("top"))
                {
                    window.Top += deltaY;
                    newHeight -= deltaY;
                }
                else
                {
                    newHeight += deltaY;
                }
                window.Height = Math.Max(newHeight, minHeight);
            }
            if (deltaX != 0 && (thumb.Name.ToLower().Contains("left") || thumb.Name.ToLower().Contains("right")))
            {
                var newWidth = window.Width;
                var newLeft = window.Left;

                if (thumb.Name.ToLower().Contains("left"))
                {
                    newLeft += deltaX;
                    newWidth -= deltaX;
                }
                else
                {
                    newWidth += deltaX;
                }

                window.Left = newLeft;
                window.Width = Math.Max(newWidth, minWidth);
            }

        }

        private static void DragCompleted(object sender, DragCompletedEventArgs e)
        {
            //var thumb = sender as Thumb;
            //var window = thumb.GetValue(Resize) as Window;

            //if (window != null)
            //{
            //    var verticalChange = window.SafeHeightChange(e.VerticalChange);
            //    var horizontalChange = window.SafeWidthChange(e.HorizontalChange);

            //    window.Width += horizontalChange;
            //    window.Height += verticalChange;
            //}
        }
        private static double SafeWidthChange(this Window window, double change, bool positive = true)
        {
            var result = positive ? window.Width + change : window.Width - change;

            if (result <= window.MinWidth)
            {
                if (positive)
                    return window.MinWidth - window.Width;
                else
                    return window.Width - window.MinWidth;
            }
            else if (result >= window.MaxWidth)
            {
                return 0;
            }
            else if (result < 0)
            {
                return 0;
            }
            else
            {
                return change;
            }
        }

        private static double SafeHeightChange(this Window window, double change, bool positive = true)
        {
            var result = positive ? window.Height + change : window.Height - change;

            if (result <= window.MinHeight)
            {
                if (positive)
                    return window.MinHeight - window.Height;
                else
                    return window.Height - window.MinHeight;
            }
            else if (result >= window.MaxHeight)
            {
                return 0;
            }
            else if (result < 0)
            {
                return 0;
            }
            else
            {
                return change;
            }
        }
    }

}
