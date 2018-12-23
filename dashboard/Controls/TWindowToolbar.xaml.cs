using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HIO.Controls
{
    /// <summary>
    /// Interaction logic for TWindowToolbar.xaml
    /// </summary>
    public partial class TWindowToolbar : UserControl
    {
        public TWindowToolbar()
        {
            InitializeComponent();
        }


        #region Properties

        public ResizeMode ResizeMode
        {
            get { return (ResizeMode)GetValue(ResizeModeProperty); }
            set { SetValue(ResizeModeProperty, value); }
        }

        public static readonly DependencyProperty ResizeModeProperty =
            DependencyProperty.Register("ResizeMode", typeof(ResizeMode), typeof(TWindowToolbar), new PropertyMetadata(ResizeMode.CanResize));



        public bool AllowShowToolbox
        {
            get { return (bool)GetValue(AllowShowToolboxProperty); }
            set { SetValue(AllowShowToolboxProperty, value); }
        }

        public static readonly DependencyProperty AllowShowToolboxProperty =
            DependencyProperty.Register("AllowShowToolbox", typeof(bool), typeof(TWindowToolbar), new PropertyMetadata(true));



        public WindowState WindowState
        {
            get { return (WindowState)GetValue(WindowStateProperty); }
            set { SetValue(WindowStateProperty, value); }
        }

        public static readonly DependencyProperty WindowStateProperty =
            DependencyProperty.Register("WindowState", typeof(WindowState), typeof(TWindowToolbar), new PropertyMetadata(WindowState.Normal));



        public bool AllowShowMinimize
        {
            get { return (bool)GetValue(AllowShowMinimizeProperty); }
            set { SetValue(AllowShowMinimizeProperty, value); }
        }

        public static readonly DependencyProperty AllowShowMinimizeProperty =
            DependencyProperty.Register("AllowShowMinimize", typeof(bool), typeof(TWindowToolbar), new PropertyMetadata(true));



        public bool AllowShowMaximizeRestore
        {
            get { return (bool)GetValue(AllowShowMaximizeRestoreProperty); }
            set { SetValue(AllowShowMaximizeRestoreProperty, value); }
        }

        public static readonly DependencyProperty AllowShowMaximizeRestoreProperty =
            DependencyProperty.Register("AllowShowMaximizeRestore", typeof(bool), typeof(TWindowToolbar), new PropertyMetadata(true));



        public bool AllowShowClose
        {
            get { return (bool)GetValue(AllowShowCloseProperty); }
            set { SetValue(AllowShowCloseProperty, value); }
        }

        public static readonly DependencyProperty AllowShowCloseProperty =
            DependencyProperty.Register("AllowShowClose", typeof(bool), typeof(TWindowToolbar), new PropertyMetadata(true));



        #endregion
        #region Methods

        private void TInteractiveButton_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            if (parentWindow == null) return;

            if (sender == Btn_Close)
            {
                parentWindow?.Close();
                if (HIO.Backend.HIOStaticValues.FirstRun== 0) {
                    Application.Current.Shutdown();
                }
            }
            else if (sender == Btn_MaxRestore)
            {
                SwapMaximizeRestore(parentWindow);

            }
            else
            {
                parentWindow.WindowState = WindowState.Minimized;
            }
        }

        private void SwapMaximizeRestore(Window parentWindow)
        {
            if (parentWindow.WindowState == WindowState.Maximized)
            {
                parentWindow.WindowState = WindowState.Normal;
            }
            else
            {
                parentWindow.WindowState = WindowState.Maximized;
            }
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Handled) return;
            var parentWindow = Window.GetWindow(this);
            if (parentWindow == null) return;

            if (e.ClickCount > 1)
            {
                if (ResizeMode != ResizeMode.NoResize)
                    SwapMaximizeRestore(parentWindow);
            }
            else if (parentWindow.WindowState != WindowState.Maximized)
            {
                parentWindow?.DragMove();
            }
        }
        #endregion

    }

}
