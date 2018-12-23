using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HIO.Controls
{
    public class TImportButton : Control
    {
        static TImportButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TImportButton), new FrameworkPropertyMetadata(typeof(TImportButton)));
        }





        public ICommand ImportCommand
        {
            get { return (ICommand)GetValue(ImportCommandProperty); }
            set { SetValue(ImportCommandProperty, value); }
        }

        public static readonly DependencyProperty ImportCommandProperty =
            DependencyProperty.Register("ImportCommand", typeof(ICommand), typeof(TImportButton), new PropertyMetadata(null));




        public bool IsImporting
        {
            get { return (bool)GetValue(IsImportingProperty); }
            set { SetValue(IsImportingProperty, value); }
        }

        public static readonly DependencyProperty IsImportingProperty =
            DependencyProperty.Register("IsImporting", typeof(bool), typeof(TImportButton), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsImportingChanged));

        private static void OnIsImportingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TImportButton).UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            if (IsImporting)
            {
                VisualStateManager.GoToState(this, "Importing", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "Default", true);
            }

        }

        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(TImportButton), new PropertyMetadata(null));



        public double ProgressPercent
        {
            get { return (double)GetValue(ProgressPercentProperty); }
            set { SetValue(ProgressPercentProperty, value); }
        }

        public static readonly DependencyProperty ProgressPercentProperty =
            DependencyProperty.Register("ProgressPercent", typeof(double), typeof(TImportButton), new PropertyMetadata(0.0));



    }
}
