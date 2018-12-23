using HIO.Backend;
using System.Windows;
using System.Windows.Input;

namespace HIO.Setup
{
    /// <summary>
    /// Interaction logic for TNewDeviceAddingPage1View.xaml
    /// </summary>
    public partial class TNewDeviceAddingPage1View
    {
        public TNewDeviceAddingPage1View()
        {
            InitializeComponent();
        }

        public TNewDeviceAddingPage1 ViewModel
        {
            get
            {
                return (TNewDeviceAddingPage1)DataContext;
            }
        }
        private void BridgeExpander_Clicked(object sender, MouseButtonEventArgs e)
        {
           // ViewModel.LoadHidDeviceAsync();
            ViewModel.IsInBluetoothTab = false;
            BridgeListContainer.Height = new GridLength(1, GridUnitType.Star);
            BluetoothListContainer.Height = new GridLength(0, GridUnitType.Pixel);
        }
        private void BluetoothExpander_Clicked(object sender, MouseButtonEventArgs e)
        {
           // ViewModel.LoadBLEDeviceAsync();
            ViewModel.IsInBluetoothTab = true;
            BridgeListContainer.Height = new GridLength(6, GridUnitType.Pixel);
            BluetoothListContainer.Height = new GridLength(1, GridUnitType.Star);
            ViewModel.StopScanBridge();
         //   ViewModel.LoadBluetoothItems();
        }

     
    }
}
