using HIO.Backend;
using HIO.ViewModels.Settings;
using Mighty.HID;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HIO.Setup
{
    public class TNewDeviceAddingPage1 : TSetupPageBase
    {
        private const string NoBluetoothDeviceMessage = "Add your HIO from your PC Bluetooth setting!";
        public TNewDeviceAddingPage1(TWizard parent, double progressPercent, TSettingManager settingManager) : base(parent, progressPercent)
        {
            ColorSearch = "#FF93B9C2";
            SettingManager = settingManager;
            Commands.AddCommand("LoadItems", LoadHidDeviceAsync);
            Commands.AddCommand("LoadBluetoothItems", LoadBLEDeviceAsync);
            Commands.AddCommand("ErrorOK", ErrorOK);
        }



        #region Fields
        private const string ConnectionUnsuccesfullMessage = @"Connection unsuccesfull!
Make sure HIO is turn on and in range";
        int counter = 0;
        DispatcherTimer timer = new DispatcherTimer();
        #endregion

        #region Properties
        public TSettingManager SettingManager { get; private set; }
        public ObservableCollection<TDevice> Items { get; private set; } = new ObservableCollection<TDevice>();
        public ObservableCollection<TDevice> BluetoothItems { get; private set; } = new ObservableCollection<TDevice>();

        public TDevice SelectedItem
        {
            get
            {
                return GetValue<TDevice>();
            }
            set
            {
                if (SetValue(value))
                {

                    Commands.Update();
                }
            }
        }
        public override bool CanMoveNextPage
        {
            get
            {
                if (base.CanMoveNextPage && SelectedItem != null)
                {
                    return true;
                }
                else
                    return false;

            }
        }

        public bool IsConnecting
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }
        public string ColorSearch
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }
        public bool IsInBluetoothTab
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                if (value)
                {
                    LoadBLEDeviceAsync();
                    SetValue(value);
                }
                else
                {
                    LoadHidDeviceAsync();
                    SetValue(value);
                }
            }

        }
        public bool DisSearch
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }
        #endregion

        #region Methods


        void ErrorOK()
        {


            Message = null;

        }
        public override void OnShow()
        {
            base.OnShow();
            LoadHidDeviceAsync();

        }
        public override void OnHide()
        {
            timer.Stop();
            timer.Tick -= timer_TickAsync;
            timer.IsEnabled = false;
            if (bleBrowser != null)
                bleBrowser.StopScan();
            base.OnHide();

        }
        public override void MoveNextPage()
        {
            timer.Stop();
            Trace.WriteLine("AddNewDevice Timer is stopped");
            timer.Tick -= timer_TickAsync;
            timer.IsEnabled = false;
            bleBrowser?.StopScan();
            Array.Copy(SelectedItem.Mac, 0, HIOStaticValues.blea.mac, 0, 6);
            //Array.Resize(ref HIOStaticValues.blea.name, Encoding.ASCII.GetBytes(SelectedItem.Title).Length);
            Array.Copy(Encoding.ASCII.GetBytes(SelectedItem.Title), 0, HIOStaticValues.blea.name, 0, Encoding.ASCII.GetBytes(SelectedItem.Title).Length);
            if (SelectedItem?.IsBLE ?? false)
            {
                HIOStaticValues.DirectBluetooth = true;
                //HIOStaticValues.EventCheckDevice.Set();
                //HIOStaticValues._signalCheckDevice.WaitOne(); //FREE

                var deviceInfo = BLEBrowser.FindConnectedDevice();
                if (deviceInfo != null)
                {
                    HIOStaticValues.BaS.devInfo = deviceInfo;
                    HIOStaticValues.BaS.dev = new BLEDevice();
                    HIOStaticValues.BaS.dev.Open(deviceInfo);
                }
                HIOStaticValues.EventCheckDevice.Set();
            }
            //else
            //{
            //    HIOStaticValues.DirectBluetooth = false;
            //}
            base.MoveNextPage();
        }


        public void StopScanBridge()
        {
            timer.Interval = TimeSpan.Zero;
            timer.Tick -= timer_TickAsync;
            timer.Stop();
            timer.IsEnabled = false;
            IsConnecting = false;
            DisSearch = true;
        }
        private BLEBrowser bleBrowser;

        //private void LoadItems()
        //{
        //    LoadHidDeviceAsync();
        //    //ErrorOK();
        //    //if (bleBrowser != null)
        //    //    bleBrowser.StopScan();
        //    //if (IsConnecting != true)
        //    //{
        //    //    IsConnecting = true;
        //    //    Items.Clear();
        //    //    ColorSearch = "#FFA9A9A9";
        //    //    DisSearch = false;

        //    //    //TODO:Load Items


        //    //    timer.Interval = TimeSpan.FromSeconds(1);
        //    //    timer.Tick += timer_TickAsync;
        //    //    timer.IsEnabled = true;
        //    //    timer.Start();
        //    // }

        //}
        private async void timer_TickAsync(object sender, EventArgs e)
        {

            var o = (sender as DispatcherTimer);
            try
            {
                IsConnecting = true;
                o.Stop();
                BoxMsgError = true;
                counter += 1;

                Commands ic = new Commands();
                Converts conv = new Converts();
                Trace.WriteLine("Start GetListBLE");
                List<TDevice> lstBLE = await Task.Run(() =>
                {
                    return ic.GetListBLE();

                });
                Trace.WriteLine("Finish GetListBLE");


                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    lstBLE.Where(ble => Items.All(item => !Enumerable.SequenceEqual(item.Mac, ble.Mac)))
                                               .Select(ble => ble).ToList().ForEach(Items.Add);

                    Items.Where(item => lstBLE.All(ble => !Enumerable.SequenceEqual(item.Mac, ble.Mac)))
                   .Select(ble => ble).ToList().ForEach(item => Items.Remove(item));
                }));
                o.Start();
                #endregion

            }
            catch (Exception ex)
            {

                o.Start();
            }
            finally
            {
                if (counter > 10 || HIOStaticValues.CONNECTIONBHIO || HIOStaticValues.BaS.dev is BLEDevice)
                {
                    IsConnecting = false;
                    if (Items.Count == 0)
                        Message = "Please keep the HIO as close as it's possible and click/double tap it.";
                    counter = 0;
                    ColorSearch = Application.Current.Resources["HIOGigari"].ToString();
                    DisSearch = true;

                    timer.Stop();
                    timer.Tick -= timer_TickAsync;
                    timer.IsEnabled = false;
                }
            }
        }

        public async void LoadHidDeviceAsync()
        {
            try
            {

                timer.Stop();
                timer.Tick -= timer_TickAsync;
                //if (HIOStaticValues.BaS.dev is HIDDev)
                //{
                HIOStaticValues.BaS.dev.Dispose();
                if (HIOStaticValues.BaS.dev is BLEDevice)
                {
                    HIOStaticValues.BaS.dev = null;
                    HIOStaticValues.BaS.dev = new HIDDev();
                }
                //Thread.Sleep(500);
                HIOStaticValues.EventCheckDevice.Set();
                Trace.WriteLine("Page1: EventCheckDevice.Set");
                await Task.Run(new Action(() =>
                {
                    Trace.WriteLine("Page1: Wait for connection");
                    HIOStaticValues._signalCheckDevice.Reset();
                    HIOStaticValues._signalCheckDevice.WaitOne();
                    Trace.WriteLine("Page1: Open Connection");

                })); //FREE
                     //}

                if (IsConnecting != true && !HIOStaticValues.DirectBluetooth)
                {
                    IsConnecting = true;
                    Items.Clear();
                    ColorSearch = "#FFA9A9A9";
                    DisSearch = false;

                    //TODO:Load Items


                    timer.Interval = TimeSpan.FromSeconds(1);
                    timer.Tick += timer_TickAsync;
                    timer.IsEnabled = true;
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                Debug.Write($"Exception HIDDevice: {ex.Message}");
                //throw;
            }

        }
        public async void LoadBLEDeviceAsync()
        {
            try
            {
                timer.IsEnabled = false;
                timer.Stop();
                timer.Tick -= timer_TickAsync;
                HIOStaticValues.BaS.dev.Dispose();
                if (HIOStaticValues.IsBLESupported && HIOStaticValues.BaS.dev is HIDDev)
                {
                    HIOStaticValues.BaS.dev = null;
                    HIOStaticValues.BaS.dev = new BLEDevice();
                }
                //HIOStaticValues.EventCheckDevice.Set();
                //}
                ErrorOK();
                IsConnecting = true;
                DisSearch = false;
                BluetoothItems.Clear();
                if (bleBrowser == null)
                {
                    bleBrowser = new BLEBrowser();
                }
                bleBrowser.StopScan();
                bleBrowser.StartScan(BluetoothItems, () =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (IsInBluetoothTab && BluetoothItems?.Count == 0)
                            Message = NoBluetoothDeviceMessage;
                        else if (Message == NoBluetoothDeviceMessage)
                            Message = null;
                    });
                });

                await Task.Delay(2000).ContinueWith(r =>
                {
                    if (IsInBluetoothTab && BluetoothItems?.Count == 0)
                        App.Current.Dispatcher.Invoke(() => Message = NoBluetoothDeviceMessage);
                    IsConnecting = false;
                    DisSearch = true;
                });

                //await Task.Run(new Action(() => HIOStaticValues._signalCheckDevice.WaitOne())); //FREE
            }
            catch (Exception ex)
            {
             
                Debug.Write($"Exception BLEDevice: {ex.Message}");

            }

        }
    }
}