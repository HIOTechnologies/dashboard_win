using System.Windows.Input;
using System;
using HIO.WPF;
using System.ComponentModel;
using System.Windows;

namespace HIO.ViewModels.Accounts
{
    /// <summary>
    /// Interaction logic for TAccountEditorView.xaml
    /// </summary>
    public partial class TAccountEditorView
    {
        public TAccountEditorView()
        {
            InitializeComponent();
            this.SetDialogStandardSize();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        private void TTextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if ((sender as HIO.Controls.TTextBox).Text == " ")
                (sender as HIO.Controls.TTextBox).Text = "";
            titleRequiredImage.Visibility = System.Windows.Visibility.Hidden;
        }

        private void TTextBox_GotFocus_1(object sender, System.Windows.RoutedEventArgs e)
        {
            if ((sender as HIO.Controls.TTextBox).Text == " ")
                (sender as HIO.Controls.TTextBox).Text = "";
            urlRequiredImage.Visibility = System.Windows.Visibility.Hidden;
        }

     
    }
}
