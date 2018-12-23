using System.Windows;
using System.Windows.Controls;

namespace HIO.ViewModels.Accounts
{
    /// <summary>
    /// Interaction logic for TAddNewAccountContent.xaml
    /// </summary>
    public partial class TAddNewAccountContent : UserControl
    {
        public TAddNewAccountContent()
        {
            InitializeComponent();
        }



        private void TTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as HIO.Controls.TTextBox).Text == " ")
                (sender as HIO.Controls.TTextBox).Text = "";
            titleRequiredImage.Visibility = System.Windows.Visibility.Hidden;
        }


        private void TPasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as HIO.Controls.TPasswordBox).Text == " ")
                (sender as HIO.Controls.TPasswordBox).Text = "";
            passRequiredImage.Visibility = System.Windows.Visibility.Hidden;
        }

        private void TTextBox_GotFocus_1(object sender, RoutedEventArgs e)
        {
            if ((sender as HIO.Controls.TTextBox).Text == " ")
                (sender as HIO.Controls.TTextBox).Text = "";
            urlRequiredImage.Visibility = System.Windows.Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (TXTtitle.Text == null || TXTtitle.Text.TrimStart() == "")
            {
                titleRequiredImage.Visibility = Visibility.Visible;

                TXTtitle.Text = " ";

            }
            if (TXTurl.Text == null || TXTurl.Text.TrimStart() == "")
            {
                urlRequiredImage.Visibility = Visibility.Visible;

                TXTurl.Text = " ";

            }
            if (TXTpass.Text == null || TXTpass.Text.TrimStart() == "")
            {
                passRequiredImage.Visibility = Visibility.Visible;

                TXTpass.Text = " ";

            }
        }
    }
}
