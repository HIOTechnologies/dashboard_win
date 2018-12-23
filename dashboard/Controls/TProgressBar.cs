using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace HIO.Controls
{
    public class TProgressBar : Control
    {
        static TProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TProgressBar), new FrameworkPropertyMetadata(typeof(TProgressBar)));
        }

        private Rectangle _Indicator;
        private ScaleTransform _Transformer;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _Transformer = (ScaleTransform)GetTemplateChild("Transformer");
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(TProgressBar), new PropertyMetadata(0.0, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TProgressBar).StartAnimation();
        }

        private void StartAnimation()
        {
            if (_Transformer != null)
            {
                double V = Math.Min(Math.Max(0, Value), 100);
                if (V > 0)
                {
                    V = V / 100;
                }
                DoubleAnimation DA = new DoubleAnimation(V, new Duration(TimeSpan.FromMilliseconds(AnimationDuration)));
                DA.EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut, Exponent = 2 };
                _Transformer.BeginAnimation(ScaleTransform.ScaleXProperty, DA);
            }
        }



        public double AnimationDuration
        {
            get { return (double)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register("AnimationDuration", typeof(double), typeof(TProgressBar), new PropertyMetadata(1500.0));



    }
}
