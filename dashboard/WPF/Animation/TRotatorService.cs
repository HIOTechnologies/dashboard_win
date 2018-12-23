using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HIO.Controls
{
    public static class TRotatorService
    {

        public static bool GetRotate(FrameworkElement obj)
        {
            return (bool)obj.GetValue(RotateProperty);
        }

        public static void SetRotate(FrameworkElement obj, bool value)
        {
            obj.SetValue(RotateProperty, value);
        }

        public static readonly DependencyProperty RotateProperty =
            DependencyProperty.RegisterAttached("Rotate", typeof(bool), typeof(TRotatorService), new PropertyMetadata(false, OnRotateChanged));

        private static void OnRotateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement FR = (FrameworkElement)d;
            if (GetRotate(FR))
            {
                DoubleAnimation DA = new DoubleAnimation();
                DA.From = 0;
                DA.To = 360;
                DA.Duration = new Duration(TimeSpan.FromSeconds(1));
                DA.RepeatBehavior = RepeatBehavior.Forever;

                RotateTransform RT = new RotateTransform();
                FR.RenderTransform = RT;
                FR.RenderTransformOrigin = new Point(0.5, 0.5);
                RT.BeginAnimation(RotateTransform.AngleProperty, DA);

            }
            else
            {
                FR.RenderTransform = null;
            }
        }
    }
}
