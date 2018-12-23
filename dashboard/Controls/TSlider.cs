using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace HIO.Controls
{
    public class TSlider : Slider
    {

        private Track _track;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _track = GetTemplateChild("PART_Track") as Track;
        }
        protected override void OnThumbDragDelta(DragDeltaEventArgs e)
        {
            // Use distance to modify value change
            Point pt = Mouse.GetPosition(_track);

            double newValue = _track.ValueFromPoint(pt);
            if (!double.IsInfinity(newValue))
                UpdateValue(newValue);
            e.Handled = true;
        }
        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (IsMoveToPointEnabled && _track != null && _track.Thumb != null && !_track.Thumb.IsMouseOver)
            {
                // Move Thumb to the Mouse location
                Point pt = e.MouseDevice.GetPosition(_track);

                double newValue = _track.ValueFromPoint(pt);
                if (!double.IsInfinity(newValue))
                    UpdateValue(newValue);
                e.Handled = true;
            }
        }

        private void UpdateValue(double value)
        {
            Double snappedValue = SnapToTick(value);

            if (snappedValue != Value)
            {
                DoubleAnimation animation = new DoubleAnimation
                {
                    //animation.From = value;
                    To = Math.Max(this.Minimum, Math.Min(this.Maximum, snappedValue)),
                    Duration = TimeSpan.FromSeconds(0.2),
                    FillBehavior = FillBehavior.HoldEnd,
                    EasingFunction = new ExponentialEase()
                };
                animation.Completed += (a, b) =>
                {
                    BeginAnimation(ValueProperty, null);
                    Value = snappedValue;
                };
                BeginAnimation(ValueProperty, animation);
            }
        }

        private double SnapToTick(double value)
        {
            if (IsSnapToTickEnabled)
            {
                double previous = Minimum;
                double next = Maximum;

                if (TickFrequency > 0.0)
                {
                    previous = Minimum + (Math.Round(((value - Minimum) / TickFrequency)) * TickFrequency);
                    next = Math.Min(Maximum, previous + TickFrequency);
                }

                value = (value > ((previous + next) * 0.5)) ? next : previous;
            }

            return value;
        }
    }
}
