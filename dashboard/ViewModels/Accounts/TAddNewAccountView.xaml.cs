using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace HIO.ViewModels.Accounts
{
    /// <summary>
    /// Interaction logic for TAddNewAccountView.xaml
    /// </summary>
    public partial class TAddNewAccountView
    {
        public TAddNewAccountView()
        {
            InitializeComponent();
            MaskGrid.Opacity = 0;
            Grd_Main.Visibility = Visibility.Collapsed;
            _OpenAnimation = ((Storyboard)FindResource("Open"));
            _CloseAnimation = ((Storyboard)FindResource("Close"));
            _CloseAnimation.Completed += (a, b) => Grd_Main.Visibility = Visibility.Collapsed;

        }


        public bool AllowShow
        {
            get { return (bool)GetValue(AllowShowProperty); }
            set { SetValue(AllowShowProperty, value); }
        }

        public static readonly DependencyProperty AllowShowProperty =
            DependencyProperty.Register("AllowShow", typeof(bool), typeof(TAddNewAccountView), new FrameworkPropertyMetadata(false, OnAllowShowChanged));


        private static void OnAllowShowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TAddNewAccountView).UpdateVisibility();
        }
        private Storyboard _OpenAnimation;
        private Storyboard _CloseAnimation;
        public void UpdateVisibility()
        {
            if (DesignerProperties.GetIsInDesignMode(this) || _OpenAnimation == null || _CloseAnimation == null) return;
            if (AllowShow)
            {
                Grd_Main.Visibility = Visibility.Visible;
                _OpenAnimation.Begin();
                MoveObjectTo(new Thickness(0, 0, 0, Grd_Road.ActualHeight - button.ActualHeight), 200, () =>
                {
                    button.VerticalAlignment = VerticalAlignment.Top;
                    button.Margin =  new Thickness(0, 0, 0, 0);
                });

                //         < DoubleAnimationUsingKeyFrames Storyboard.TargetProperty = "(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.Y)" 
                //Storyboard.TargetName = "button" >

                //< EasingDoubleKeyFrame x: Name = "a1" KeyTime = "0:0:0.2" Value = "-495" />

                //      < EasingDoubleKeyFrame KeyTime = "0:0:0.5" Value = "-514" />

                //     </ DoubleAnimationUsingKeyFrames >

            }
            else
            {
                _CloseAnimation.Begin(this, true);
                MoveObjectTo(new Thickness(0, Grd_Road.ActualHeight - button.ActualHeight, 0, 0), 200, () =>
                {
                    button.VerticalAlignment = VerticalAlignment.Bottom;
                    button.Margin = new Thickness(0, 0, 0, 0);
                });

            }
        }

        private void MoveObjectTo(Thickness margin, int duration = 200, Action completed = null)
        {
            ThicknessAnimation DA = new ThicknessAnimation();
            DA.To = margin;
            DA.Duration = new Duration(TimeSpan.FromMilliseconds(duration));
            DA.RepeatBehavior = new RepeatBehavior(1);
            DA.FillBehavior = FillBehavior.HoldEnd;
            DA.Completed += (a, b) =>
            {
                button.BeginAnimation(MarginProperty, null);
                completed?.Invoke();
            };
            button.BeginAnimation(MarginProperty, DA);
        }
    }

}
