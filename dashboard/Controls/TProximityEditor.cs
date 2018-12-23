using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HIO.Controls
{
    public class TProximityEditor : Control
    {
        static TProximityEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TProximityEditor), new FrameworkPropertyMetadata(typeof(TProximityEditor)));
        }
        private Thumb _Thumb;
        private ItemsControl PART_Ticks;
        private RepeatButton Part_RightButton;
        private RepeatButton Part_LeftButton;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _Thumb = (Thumb)GetTemplateChild("PART_THUMB");
            PART_Ticks = (ItemsControl)GetTemplateChild("PART_Ticks");
            Part_LeftButton = (RepeatButton)GetTemplateChild("Part_LeftButton");
            if (Part_LeftButton != null) Part_LeftButton.Click += Part_LeftButton_Click;

            Part_RightButton = (RepeatButton)GetTemplateChild("Part_RightButton");
            if (Part_RightButton != null) Part_RightButton.Click += Part_LeftButton_Click;

            if (_Thumb != null)
            {
                _Thumb.DragDelta += _Thumb_DragDelta;
                _Thumb.DragCompleted += _Thumb_DragCompleted;
            }

            if (PART_Ticks != null) UpdateTickPoints();
            SnapToNearestFromValue();
        }

        private void Part_LeftButton_Click(object sender, RoutedEventArgs e)
        {
            TTickPoint Current = _TickPoints.FirstOrDefault(t => t.Value == Value);
            if (Current == null) return;
            if (sender == Part_LeftButton && Current.Previous != null)
            {
                Value = Current.Previous.Value;
            }
            else if (sender == Part_RightButton && Current.Next != null)
            {
                Value = Current.Next.Value;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateTickPoints();
        }
        private void UpdateTickPoints()
        {
            if (_TickPoints == null) return;
            if (Ticks == null) return;

            _TickPoints.Clear();
            var TotalWidth = (PART_Ticks.ActualWidth - PART_Ticks.Margin.Right - PART_Ticks.Margin.Left);
            var ItemWidth = TotalWidth / (Ticks.Count - 1);
            int index = 0;
            foreach (var Value in Ticks)
            {
                var TickPoint = new TTickPoint(this, Value);
                TickPoint.Left = (int)(index * ItemWidth);
                _TickPoints.Add(TickPoint);
                index++;
            }

            if (PART_Ticks != null) PART_Ticks.ItemsSource = _TickPoints;
            SnapToNearestFromValue();
        }

        private void _Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            SnapToNearestFromLocation();
        }

        private void SnapToNearestFromLocation()
        {
            var location = ThumbLeft;

            TTickPoint previous = null;
            foreach (var item in _TickPoints)
            {
                if (item.Previous == null) continue;
                previous = item.Previous;

                if (location >= previous.Left && location <= item.Left)
                {
                    double LeftDelta = location - previous.Left;
                    double RightDelta = item.Left - location;
                    if (LeftDelta < RightDelta)
                    {
                        SnapTo(previous);
                    }
                    else
                    {
                        SnapTo(item);
                    }
                    break;
                }
                else if (item.Next == null)
                {
                    SnapTo(item);
                    break;
                }
            }
        }

        private void AnimateToLocation(double location)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                //animation.From = value;
                To = location,
                Duration = TimeSpan.FromSeconds(0.2),
                FillBehavior = FillBehavior.HoldEnd,
                EasingFunction = new ExponentialEase()
            };
            animation.Completed += (a, b) =>
            {
                BeginAnimation(ThumbLeftProperty, null);
                ThumbLeft = location;
            };
            BeginAnimation(ThumbLeftProperty, animation);
        }

        private void SnapToNearestFromValue()
        {
            double value = Value;
            TTickPoint previous = null;
            if (!_TickPoints.Any()) return;

            var FirstTick = _TickPoints.FirstOrDefault();
            var LastTick = _TickPoints.LastOrDefault();
            if (value < FirstTick.Value)
            {
                SnapTo(FirstTick);

                return;
            }
            if (value > LastTick.Value)
            {
                SnapTo(LastTick);
                return;
            }
            foreach (var item in _TickPoints)
            {
                if (item.Previous == null) continue;
                previous = item.Previous;

                if (value >= previous.Value && value <= item.Value)
                {
                    double LeftDelta = value - previous.Value;
                    double RightDelta = item.Value - value;
                    if (LeftDelta < RightDelta)
                    {
                        SnapTo(previous);
                    }
                    else
                    {
                        SnapTo(item);
                    }
                    return;
                }
            }

        }

        private void SnapTo(TTickPoint tick)
        {
            if (Value != tick.Value)
            {
                Value = tick.Value;
            }
            if (ThumbLeft != tick.Left) AnimateToLocation(tick.Left);
        }

        private void _Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Canvas canvas = (Canvas)_Thumb.Parent;
            double Left = IsNan(Canvas.GetLeft(_Thumb), 0);
            Canvas.SetLeft(_Thumb, SetInRange(0, Left + e.HorizontalChange, canvas.ActualWidth - _Thumb.ActualWidth));
            //_Thumb.RenderTransform e.HorizontalChange
        }
        private double SetInRange(double min, double value, double max)
        {
            return Math.Min(Math.Max(min, value), max);
        }
        private double IsNan(double v1, double v2)
        {
            if (double.IsNaN(v1)) return v2;
            return v1;
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(TProximityEditor), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TProximityEditor).SnapToNearestFromValue();
        }

        public DoubleCollection Ticks
        {
            get { return (DoubleCollection)GetValue(TicksProperty); }
            set { SetValue(TicksProperty, value); }
        }

        public static readonly DependencyProperty TicksProperty =
            DependencyProperty.Register("Ticks", typeof(DoubleCollection), typeof(TProximityEditor), new PropertyMetadata(null));



        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(TProximityEditor), new PropertyMetadata(2.0));



        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(TProximityEditor), new PropertyMetadata(10.0));



        public double ThumbLeft
        {
            get { return (double)GetValue(ThumbLeftProperty); }
            set { SetValue(ThumbLeftProperty, value); }
        }

        public static readonly DependencyProperty ThumbLeftProperty =
            DependencyProperty.Register("ThumbLeft", typeof(double), typeof(TProximityEditor), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnLeftChanged));

        private static void OnLeftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private ObservableCollection<TTickPoint> _TickPoints = new ObservableCollection<TTickPoint>();


        public class TTickPoint : INotifyPropertyChanged
        {

            public TTickPoint(TProximityEditor editor, double value, double left = 0)
            {
                _Editor = editor;
                _Value = value;
                _Left = left;
            }
            private TProximityEditor _Editor;
            private double _Value;
            private double _Left;

            public TTickPoint Previous
            {
                get
                {
                    int index = _Editor._TickPoints.IndexOf(this);
                    if (index > 0) return _Editor._TickPoints[index - 1];
                    return null;
                }
            }
            public TTickPoint Next
            {
                get
                {
                    int index = _Editor._TickPoints.IndexOf(this);
                    if ((index + 1) < _Editor._TickPoints.Count) return _Editor._TickPoints[index + 1];
                    return null;
                }
            }



            public double Value
            {
                get { return _Value; }
                set
                {
                    _Value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
                }
            }



            public event PropertyChangedEventHandler PropertyChanged;

            public double Left
            {
                get { return _Left; }
                set
                {
                    _Left = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Left"));
                }
            }

            public override string ToString()
            {
                return $"{Value} = {Left}";
            }
        }
    }
    public class TValueToCanvasLocation : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double Value = (double)values[0];
            double ActualWidth = (double)values[1];
            DoubleCollection AllValues = (DoubleCollection)values[2];
            var ItemWidth = ActualWidth / (AllValues.Count - 1);
            return AllValues.IndexOf(Value) * ItemWidth;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
    public class TThumbValueToCanvasLocation : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double Value = (double)values[0];
            double ActualWidth = (double)values[1];
            DoubleCollection AllValues = (DoubleCollection)values[2];
            var ItemWidth = ActualWidth / (AllValues.Count - 1);
            return AllValues.IndexOf(Value) * ItemWidth;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
