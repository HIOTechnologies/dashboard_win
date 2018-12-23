using HIO.Controls;
using HIO.Core;
using HIO.Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;

namespace HIO.ViewModels.Settings
{
    public class TDevice : TViewModelBase
    {
        public TDevice(TSettingManager parent, string title = null, SignalEnum signalValue = SignalEnum.NoConnection, byte[] mac = null, string id = null, bool isConnected = false)
        {
            Parent = parent;
            Id = id;
            Title = title;
            SignalValue = signalValue;
            Mac = mac;
            //IsConnected = isConnected;
            Commands.AddCommand("Forget", new Action(() => { HIOStaticValues.commandQ.Add(Forget); }));
            Commands.AddCommand("Disconnect", Disconnect);
            Commands.AddCommand("Connect", Connect);
        }



        public TSettingManager Parent { get; private set; }
        public byte[] Mac
        {
            get
            {
                return GetValue<byte[]>();
            }
            set
            {
                SetValue(value);
            }
        }

        public string Title
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

        public string Id
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
        public bool IsConnected
        {
            get
            {
                return HIOStaticValues.blea.mac.SequenceEqual(Mac);
            }
            //set
            //{
            //    SetValue(value);
            //}
        }
        public bool IsBLE
        {
            get
            {
                return Id?.StartsWith("BluetoothLE#") ?? false;
            }
        }

        /// <summary>
        /// Value Range : 0-3
        /// </summary>
        public SignalEnum SignalValue
        {
            get
            {
                return GetValue<SignalEnum>();
            }
            set
            {
                SetValue(value);
            }
        }

        private async void Forget()
        {
            if (IsBLE)
            {
                await forgetBLE();
            }
            else
            {
                Commands ic = new Commands();
                ic.UnBond(Mac);
            }
        }

        private async Task forgetBLE()
        {
            var errorHandle = new ErrorHandle();
            try
            {//TODO: use BLEDevice Unpair method
                var isConnected = IsConnected;
                var device = await BluetoothLEDevice.FromIdAsync(Id);
                await device.DeviceInformation.Pairing.UnpairAsync();
                if (isConnected)
                {
                    HIOStaticValues.CONNECTIONBRIDGE = false;
                    HIOStaticValues.BaS.devInfo = null;
                    HIOStaticValues.BaS.dev.Dispose();
                    HIOStaticValues.BaS.dev = null;
                }
                errorHandle.logEvent($"Forget BLE: {Title}");
            }
            catch (Exception ex)
            {
               
            }
        }
        public void Disconnect()
        {
        }
        private void Connect()
        {
        }
    }
}
