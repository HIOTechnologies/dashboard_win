using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace HIO.Controls
{
    /// <summary>
    /// Interaction logic for TInnerMessageBox.xaml
    /// </summary>
    public partial class TInnerMessageBox : UserControl
    {
        public TInnerMessageBox()
        {
            InitializeComponent();
        }


        private DispatcherTimer timer;
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TInnerMessageBox), new PropertyMetadata(null, OnTextChanged));
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as TInnerMessageBox;
            if (((string)e.NewValue).IsNullOrEmpty())
            {
                control.InternalText = null;
                control.DisposeTimer();
                return;
            }
            if (control.Expiry != null)
            {
                control.CreateTimer();
                control.InternalText = e.NewValue?.ToString().Replace("{TimeRemaining}", (control.Expiry.Value - (((TimeSpan?)control.timer.Tag) ?? TimeSpan.Zero)).ToReadableString());
                control.timer.Start();
            }
            else
            {
                control.InternalText = e.NewValue.ToString();
            }

        }
        private string InternalText
        {
            get { return (string)GetValue(InternalTextProperty); }
            set
            {
                SetValue(InternalTextProperty, value);
            }
        }

        public static readonly DependencyProperty InternalTextProperty =
            DependencyProperty.Register("InternalText", typeof(string), typeof(TInnerMessageBox), new PropertyMetadata(null));


        public TimeSpan? Expiry
        {
            get { return (TimeSpan?)GetValue(ExpiryProperty); }
            set
            {
                SetValue(ExpiryProperty, value);
            }
        }
        public static readonly DependencyProperty ExpiryProperty =
           DependencyProperty.Register("Expiry", typeof(TimeSpan?), typeof(TInnerMessageBox), new PropertyMetadata(null, OnExpiryChanged));
        private static void OnExpiryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as TInnerMessageBox;
            if (e.NewValue != e.OldValue)
            {
                control.DisposeTimer();
                if (e.NewValue != null)
                {
                    control.CreateTimer();
                }
                else
                {
                    control.DisposeTimer();
                }
            }
        }

        private void CreateTimer()
        {
            if (timer != null) return;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }
        private void DisposeTimer()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.IsEnabled = false;
                timer.Tick -= Timer_Tick;
                timer = null;
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (timer.Tag == null)
                timer.Tag = TimeSpan.FromSeconds(1);
            else
                timer.Tag = ((TimeSpan)timer.Tag).Add(TimeSpan.FromSeconds(1));
            if (((TimeSpan)timer.Tag) >= Expiry)
            {
                DisposeTimer();
                InternalText = null;
                if (OKCommand != null)
                {
                    if (OKCommand.CanExecute(null))
                    {
                        OKCommand.Execute(null);
                    }
                }
                else
                {
                    Visibility = Visibility.Collapsed;
                }
            }
            if (!Text.IsNullOrEmpty() && Expiry.HasValue && timer?.Tag != null)
                InternalText = Text.Replace("{TimeRemaining}", (Expiry.Value - (((TimeSpan?)timer.Tag) ?? TimeSpan.Zero)).ToReadableString());
        }

        public ICommand OKCommand
        {
            get { return (ICommand)GetValue(OKCommandProperty); }
            set { SetValue(OKCommandProperty, value); }
        }

        public static readonly DependencyProperty OKCommandProperty =
            DependencyProperty.Register("OKCommand", typeof(ICommand), typeof(TInnerMessageBox), new PropertyMetadata(null));
        public ICommand TryCommand
        {
            get { return (ICommand)GetValue(TryCommandProperty); }
            set { SetValue(TryCommandProperty, value); }
        }

        public static readonly DependencyProperty TryCommandProperty =
            DependencyProperty.Register("TryCommand", typeof(ICommand), typeof(TInnerMessageBox), new PropertyMetadata(null));


    }
}
