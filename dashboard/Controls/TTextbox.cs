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
    public class TTextBox : TextBox
    {
        static TTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TTextBox), new FrameworkPropertyMetadata(typeof(TTextBox)));
        }
        public TTextBox()
        {
            TextChanged += TTextBox_TextChanged;
        }

        private void TTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholderPosition();
        }

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(TTextBox), new PropertyMetadata(null));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _PlaceHolder = (FrameworkElement)GetTemplateChild("PART_PlaceHolder");
            this.Loaded += (a, b) => UpdatePlaceholderPosition();
        }


        private FrameworkElement _PlaceHolder;

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            UpdatePlaceholderPosition();
        }

        private void UpdatePlaceholderPosition()
        {
            if (_PlaceHolder == null || Placeholder.IsNullOrEmpty()) return;

            if (!Text.IsNullOrEmpty() || IsKeyboardFocusWithin)
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

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            UpdatePlaceholderPosition();
        }


    }


}
