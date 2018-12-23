using System;
using System.Windows.Input;

namespace HIO.Extentions
{
    /// <summary>
    /// Interaction logic for TExtention07View.xaml
    /// </summary>
    public partial class TExtention07View
    {
        public TExtention07View()
        {
            InitializeComponent();
        }

        private void TComboBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                Cbo_Username.IsDropDownOpen = !Cbo_Username.Text.IsNullOrEmpty();
                if (!Cbo_Username.IsKeyboardFocusWithin) Cbo_Username.Focus();
            }
        }


    }
}
