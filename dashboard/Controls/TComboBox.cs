using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System;
using System.Windows.Media.Animation;
using System.Windows.Input;

namespace HIO.Controls
{
    public class TComboBox : ComboBox
    {
        static TComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TComboBox), new FrameworkPropertyMetadata(typeof(TComboBox)));
        }
        public TComboBox()
        {
            MaxDropDownHeight = 200;
            
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _PlaceHolder = (FrameworkElement)GetTemplateChild("PART_PlaceHolder");
            this.Loaded += (a, b) => UpdatePlaceholderPosition();
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            UpdatePlaceholderPosition();
        }

        
        public bool ShowCombo
        {
            get { return (bool)GetValue(ShowComboProperty); }
            set { SetValue(ShowComboProperty, value); }
        }

        public static readonly DependencyProperty ShowComboProperty =
            DependencyProperty.Register("ShowCombo", typeof(bool), typeof(TComboBox), new PropertyMetadata(true));





        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(TComboBox), new PropertyMetadata(null));



        public Brush CaretBrush
        {
            get { return (Brush)GetValue(CaretBrushProperty); }
            set { SetValue(CaretBrushProperty, value); }
        }

        public static readonly DependencyProperty CaretBrushProperty =
            DependencyProperty.Register("CaretBrush", typeof(Brush), typeof(TComboBox), new PropertyMetadata(Brushes.Black));

        private FrameworkElement _PlaceHolder;


        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);
            UpdatePlaceholderPosition();
        }

        private void UpdatePlaceholderPosition()
        {
            if (_PlaceHolder == null || Placeholder.IsNullOrEmpty()) return;
            if (!Text.IsNullOrEmpty() || this.IsKeyboardFocusWithin)
            {
                ThicknessAnimation thicknessAnimation = new ThicknessAnimation();
                thicknessAnimation.To = new Thickness(0, -_PlaceHolder.ActualHeight + 2, 0, 0);
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



    }
}
