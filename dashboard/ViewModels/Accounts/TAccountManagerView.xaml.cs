using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace HIO.ViewModels.Accounts
{
    /// <summary>
    /// Interaction logic for TAccountManagerView.xaml
    /// </summary>
    public partial class TAccountManagerView : UserControl
    {
        public TAccountManagerView()
        {
            InitializeComponent();
        }


        private void Btn_SortBy_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Cmnu_SortBy.PlacementTarget = Btn_SortBy;
            Cmnu_SortBy.Placement = PlacementMode.Relative;
            Cmnu_SortBy.MinWidth = 130;
            Cmnu_SortBy.HorizontalOffset = -30;
            Cmnu_SortBy.VerticalOffset = 5;
            //Cmnu_Main.off = PlacementMode.Custom;
            //Cmnu_Main.CustomPopupPlacementCallback = Test;
            Cmnu_SortBy.IsOpen = true;
 
        }

        private void TSearchbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F8)
                HIO.Backend.HIOStaticValues.BaS.SwitchSetDeviceName((sender as Controls.TSearchbox).txt.Text??"");
        }
    }
}
