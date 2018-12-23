using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HIO.Controls
{
    public class TPasswordBox : TextBox
    {
        static TPasswordBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TPasswordBox), new FrameworkPropertyMetadata(typeof(TPasswordBox)));
        }

        private CheckBox PART_ShowHide;
        private PasswordBox PART_Password;
        private TextBox PART_TextBox;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PART_Password = (PasswordBox)GetTemplateChild("PART_Password");
            PART_Password.PasswordChanged += PART_Password_PasswordChanged;
            PART_Password.GotFocus += PART_Password_GotFocus;
            PART_Password.LostFocus += PART_Password_LostFocus;

            PART_TextBox = (TextBox)GetTemplateChild("PART_TextBox");
            PART_TextBox.TextChanged += PART_TextBox_TextChanged;
            PART_TextBox.GotFocus += PART_Password_GotFocus;
            PART_TextBox.LostFocus += PART_Password_LostFocus;

            PART_ShowHide = (CheckBox)GetTemplateChild("PART_ShowHide");
            PART_ShowHide.PreviewMouseLeftButtonDown += PART_ShowHide_MouseLeftButtonDown;
            PART_ShowHide.PreviewMouseLeftButtonUp += PART_ShowHide_MouseLeftButtonUp;
            this.TextChanged += TPasswordBox_TextChanged;

            _PlaceHolder = (FrameworkElement)GetTemplateChild("PART_PlaceHolder");
            this.Loaded += (a, b) =>
            {
                if (!string.Equals(PART_Password.Password, Text)) PART_Password.Password = Text;
                if (!string.Equals(PART_TextBox.Text, Text)) PART_TextBox.Text = Text;
                UpdatePlaceholderPosition();
            };
        }

        private void PART_Password_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderPosition();
        }

        private void PART_Password_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderPosition();
        }

        private FrameworkElement _PlaceHolder;

        private void UpdatePlaceholderPosition()
        {
            if (_PlaceHolder == null || Placeholder.IsNullOrEmpty()) return;
            if (!Text.IsNullOrEmpty() || PART_Password.IsKeyboardFocusWithin)
            {
                ThicknessAnimation thicknessAnimation = new ThicknessAnimation();
                thicknessAnimation.To = new Thickness(0, -_PlaceHolder.ActualHeight - 5, 0, 0);
                thicknessAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(500));
                _PlaceHolder.BeginAnimation(MarginProperty, thicknessAnimation);
                ScaleTransform scaleTransform = new ScaleTransform(0.9, 0.9);
                _PlaceHolder.RenderTransform = scaleTransform;

            }
            else
            {
                ThicknessAnimation thicknessAnimation = new ThicknessAnimation();
                thicknessAnimation.To = new Thickness(5, 0, 0, 0);
                thicknessAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(500));
                _PlaceHolder.BeginAnimation(MarginProperty, thicknessAnimation);
                _PlaceHolder.RenderTransform = null;
            }

        }
        private void TPasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.Equals(PART_Password.Password, Text)) PART_Password.Password = Text;
            if (!string.Equals(PART_TextBox.Text, Text)) PART_TextBox.Text = Text;
        }

        private void PART_Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.Equals(PART_Password.Password, Text)) Text = PART_Password.Password;
            UpdatePlaceholderPosition();
        }
        private void PART_TextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!string.Equals(PART_TextBox.Text, Text)) Text = PART_TextBox.Text;
            UpdatePlaceholderPosition();
        }
        private void PART_ShowHide_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!KeepStateOfShowPassword)
            {
                PART_ShowHide.ReleaseMouseCapture();
                ShowPassword = false;
                e.Handled = true;
            }
            else
            {
                ShowPassword = !ShowPassword;
            }
        }

        private void PART_ShowHide_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!KeepStateOfShowPassword)
            {
                PART_ShowHide.CaptureMouse();
                ShowPassword = true;
                e.Handled = true;
            }
        }

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(TPasswordBox), new PropertyMetadata(null));

        public bool ShowPasswordEnabled
        {
            get { return (bool)GetValue(ShowPasswordEnabledProperty); }
            set { SetValue(ShowPasswordEnabledProperty, value); }
        }

        public static readonly DependencyProperty ShowPasswordEnabledProperty =
            DependencyProperty.Register("ShowPasswordEnabled", typeof(bool), typeof(TPasswordBox), new PropertyMetadata(true));

        public bool ShowPassword
        {
            get { return (bool)GetValue(ShowPasswordProperty); }
            set { SetValue(ShowPasswordProperty, value); }
        }

        public static readonly DependencyProperty ShowPasswordProperty =
            DependencyProperty.Register("ShowPassword", typeof(bool), typeof(TPasswordBox), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));




        public bool KeepStateOfShowPassword
        {
            get { return (bool)GetValue(KeepStateOfShowPasswordProperty); }
            set { SetValue(KeepStateOfShowPasswordProperty, value); }
        }


        public static readonly DependencyProperty KeepStateOfShowPasswordProperty =
            DependencyProperty.Register("KeepStateOfShowPassword", typeof(bool), typeof(TPasswordBox), new PropertyMetadata(true));



    }
}
