using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace HIO.Controls
{
    public class TLinkButton : Button
    {
        static TLinkButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TLinkButton), new FrameworkPropertyMetadata(typeof(TLinkButton)));
        }
        //private ContentPresenter _Border;
        //public override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();
        //    _Border = (ContentPresenter)GetTemplateChild("Part_Content");
        //}
        //protected override void OnMouseEnter(MouseEventArgs e)
        //{
        //    base.OnMouseEnter(e);
        //    TextElement.SetForeground(_Border, MouseHoverBrush);
        //}
        //protected override void OnMouseLeave(MouseEventArgs e)
        //{
        //    base.OnMouseLeave(e);
        //    TextElement.SetForeground(_Border, Foreground);
        //}

        public Brush MouseHoverBrush
        {
            get { return (Brush)GetValue(MouseHoverBrushProperty); }
            set { SetValue(MouseHoverBrushProperty, value); }
        }

        public static readonly DependencyProperty MouseHoverBrushProperty =
            DependencyProperty.Register("MouseHoverBrush", typeof(Brush), typeof(TLinkButton), new PropertyMetadata(Brushes.Black));


    }
}
