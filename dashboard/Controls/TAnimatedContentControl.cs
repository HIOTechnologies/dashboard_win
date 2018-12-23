using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace HIO.Controls
{
    /// <summary>
    /// A ContentControl that animates the transition between content
    /// </summary>
    [TemplatePart(Name = "PART_PaintArea", Type = typeof(Shape)),
     TemplatePart(Name = "PART_MainContent", Type = typeof(ContentPresenter))]
    public class TAnimatedContentControl : ContentControl
    {
        #region Generated static constructor
        static TAnimatedContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TAnimatedContentControl), new FrameworkPropertyMetadata(typeof(TAnimatedContentControl)));
        }
        #endregion

        Shape m_paintArea;
        ContentPresenter m_mainContent;
        static IEasingFunction DefaultSlideEasing = new BackEase { Amplitude = 0.5, EasingMode = EasingMode.EaseOut };

        /// <summary>
        /// This gets called when the template has been applied and we have our visual tree
        /// </summary>
        public override void OnApplyTemplate()
        {
            m_paintArea = Template.FindName("PART_PaintArea", this) as Shape;
            m_mainContent = Template.FindName("PART_MainContent", this) as ContentPresenter;

            base.OnApplyTemplate();
        }

        /// <summary>
        /// This gets called when the content we're displaying has changed
        /// </summary>
        /// <param name="oldContent">The content that was previously displayed</param>
        /// <param name="newContent">The new content that is displayed</param>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (m_paintArea != null && m_mainContent != null)
            {
                m_paintArea.Fill = CreateBrushFromVisual(m_mainContent);
                BeginAnimateContentReplacement();
            }
            base.OnContentChanged(oldContent, newContent);
        }

        /// <summary>
        /// Starts the animation for the new content
        /// </summary>
        private void BeginAnimateContentReplacement()
        {
            var newContentTransform = new TranslateTransform();
            var oldContentTransform = new TranslateTransform();
            m_paintArea.RenderTransform = oldContentTransform;
            m_mainContent.RenderTransform = newContentTransform;
            if (AnimationType == AnimationType.Slide)
            {

                m_paintArea.Visibility = Visibility.Visible;
                if (Direction == AnimationDirection.RightToLeft)
                {
                    newContentTransform.BeginAnimation(TranslateTransform.XProperty, CreateAnimation(ActualWidth, 0, DefaultSlideEasing));
                    oldContentTransform.BeginAnimation(TranslateTransform.XProperty, CreateAnimation(0, -ActualWidth, DefaultSlideEasing, () => m_paintArea.Visibility = Visibility.Hidden));
                }
                else
                {
                    newContentTransform.BeginAnimation(TranslateTransform.XProperty, CreateAnimation(-ActualWidth, 0));
                    oldContentTransform.BeginAnimation(TranslateTransform.XProperty, CreateAnimation(0, ActualWidth, DefaultSlideEasing, () => m_paintArea.Visibility = Visibility.Hidden));
                }
            }
            else if (AnimationType == AnimationType.FadeInOut)
            {
                m_mainContent.Visibility = Visibility.Hidden;
                m_paintArea.BeginAnimation(OpacityProperty, CreateAnimation(1, 0, null, () =>
                {
                    m_paintArea.Opacity = 0;
                    m_mainContent.Opacity = 0;
                    m_mainContent.Visibility = Visibility.Visible;

                    m_mainContent.BeginAnimation(OpacityProperty, CreateAnimation(0, 1));

                }));


            }
        }

        /// <summary>
        /// Creates the animation that moves content in or out of view.
        /// </summary>
        /// <param name="from">The starting value of the animation.</param>
        /// <param name="to">The end value of the animation.</param>
        /// <param name="whenDone">(optional) A callback that will be called when the animation has completed.</param>
        private AnimationTimeline CreateAnimation(double from, double to, IEasingFunction easingFunction = null, Action whenDone = null)
        {

            var duration = new Duration(TimeSpan.FromMilliseconds(AnimationDuration));
            var anim = new DoubleAnimation(from, to, duration);

            if (easingFunction != null) anim.EasingFunction = easingFunction;

            if (whenDone != null)
            {
                anim.Completed += (a, b) => whenDone();
            }

            anim.Freeze();
            return anim;
        }

        /// <summary>
        /// Creates a brush based on the current appearnace of a visual element. The brush is an ImageBrush and once created, won't update its look
        /// </summary>
        /// <param name="v">The visual element to take a snapshot of</param>
        private Brush CreateBrushFromVisual(Visual v)
        {
            if (v == null)
                throw new ArgumentNullException("v");
            var target = new RenderTargetBitmap((int)this.ActualWidth, (int)this.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            target.Render(v);
            var brush = new ImageBrush(target);
            brush.Freeze();
            return brush;
        }

        public double AnimationDuration
        {
            get { return (double)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register("AnimationDuration", typeof(double), typeof(TAnimatedContentControl), new PropertyMetadata(1000.0));

        public AnimationDirection Direction
        {
            get { return (AnimationDirection)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }

        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register("Direction", typeof(AnimationDirection), typeof(TAnimatedContentControl), new PropertyMetadata(AnimationDirection.RightToLeft));



        public AnimationType AnimationType
        {
            get { return (AnimationType)GetValue(AnimationTypeProperty); }
            set { SetValue(AnimationTypeProperty, value); }
        }

        public static readonly DependencyProperty AnimationTypeProperty =
            DependencyProperty.Register("AnimationType", typeof(AnimationType), typeof(TAnimatedContentControl), new PropertyMetadata(AnimationType.FadeInOut));


    }

    public enum AnimationDirection
    {
        RightToLeft = 0,
        LeftToRight,

    }
    public enum AnimationType
    {
        FadeInOut,
        Slide
    }

}
