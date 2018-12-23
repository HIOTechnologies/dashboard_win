using HIO.Backend;
using HIO.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Devices.Radios;

namespace Mighty.HID
{
    public class BLEDevice : IDevice
    {
        [DllImport("user32")]
        public static extern void LockWorkStation();
        public const string IsConnectedProperty = "System.Devices.Aep.IsConnected";
        public const string DeviceAddressProperty = "System.Devices.Aep.DeviceAddress";
        public const string IsConnectableProperty = "System.Devices.Aep.Bluetooth.Le.IsConnectable";
        public const string SignalStrengthProperty = "System.Devices.Aep.SignalStrength";

        public static readonly Guid HIOServiceUuid = new Guid("b36e1e004b5638864c48f903401b31f8");
        private static readonly Guid ReadCharUUID = new Guid("b36e1e024b5638864c48f903401b31f8");
        private static readonly Guid NotifyCharUUID = new Guid("b36e1e044b5638864c48f903401b31f8");
        private static readonly Guid WriteCharUUID = new Guid("b36e1e014b5638864c48f903401b31f8");

        private GattCharacteristic _ReadChar;
        private GattCharacteristic _NotifyChar;
        private GattCharacteristic _WriteChar;

        private static Radio BluetoothRadio;

        private object _lock = new object();
        BluetoothLEDevice _Device;
        GattDeviceService _Service;
        public IReadOnlyList<GattCharacteristic> _Characteristics = new List<GattCharacteristic>();
        public BLEDevice()
        {
            Task.Run(async () =>
            {
                BluetoothRadio = (await Radio.GetRadiosAsync()).FirstOrDefault(radio => radio.Kind == RadioKind.Bluetooth);
            });
        }

        public bool CanRead => _Device?.ConnectionStatus == BluetoothConnectionStatus.Connected && _Characteristics?.Count > 0;

        public bool CanWrite => _Device?.ConnectionStatus == BluetoothConnectionStatus.Connected && _Characteristics?.Count > 0;

        public void Close()
        {
            //HIOStaticValues.DirectBluetooth = false;
            if (Monitor.IsEntered(_ReadLock))
            {
                Monitor.Enter(_ReadLock);
                Monitor.Pulse(_ReadLock);
                Monitor.Exit(_ReadLock);
            }
        }
        public void Dispose()
        {
            //lock (_lock)
            //{

            Close();

            if (_Device == null)
                return;

            if (BluetoothRadio != null)
            {
                BluetoothRadio.StateChanged -= _BluetoothRadio_StateChanged;
                BluetoothRadio = null;
            }

            if (_Characteristics != null)
            {
                if (_ReadChar != null && _NotifyChar != null)
                {
                    _ReadChar.ValueChanged -= _Read;
                    _NotifyChar.ValueChanged -= _Read;
                }
                _Characteristics = null;
            }

            if (_Service != null)
            {
                _Service.Dispose();
                _Service = null;
            }

            if (_Device != null)
            {
                _Device.ConnectionStatusChanged -= _Device_ConnectionStatusChanged;
                _Device.GattServicesChanged -= _Device_GattServicesChanged;
                _Device.Dispose();
                _Device = null;
            }

            //}
            GC.Collect();

            Monitor.Enter(_ReadLock);

            Monitor.Pulse(_ReadLock);

            Monitor.Exit(_ReadLock);
        }
        public bool Open(IDeviceInfo dev)
        {
            lock (_lock)
            {
                if (_Device != null && _Characteristics != null && _ReadChar != null && _NotifyChar != null && _WriteChar != null)
                    return false; //TODO: check connection
                BluetoothLEDevice.FromIdAsync(dev.Path)
                    .AsTask()
                    .ContinueWith(r1 =>
                    {
                        _Device = r1.Result;
                        if (_Device != null)
                        {
                            BluetoothRadio.StateChanged -= _BluetoothRadio_StateChanged;
                            BluetoothRadio.StateChanged += _BluetoothRadio_StateChanged;
                            _Device.ConnectionStatusChanged += _Device_ConnectionStatusChanged;
                            _Device.GattServicesChanged += _Device_GattServicesChanged;
                            _Device.GetGattServicesForUuidAsync(HIOServiceUuid)
                            .AsTask()
                            .ContinueWith(r2 =>
                            {
                                _Service = r2.Result.Services.First();
                                _Service.OpenAsync(GattSharingMode.SharedReadAndWrite).AsTask().Wait(); 
                                if (_Characteristics == null)
                                    _Characteristics = new List<GattCharacteristic>();
                                lock (_Characteristics)
                                {
                                    GetCharacteristics();
                                    Trace.WriteLine($"Number of chars: {_Characteristics?.Count ?? -1}");
                                }
                            })
                            .Wait();
                        }
                    })
                    .Wait();
                return true;
            }
        }

        private void Reopen()
        {
            if (_Device == null)
                return;
            var id = _Device.DeviceId;

            if (BluetoothRadio != null)
            {
                BluetoothRadio.StateChanged -= _BluetoothRadio_StateChanged;
                BluetoothRadio = null;
            }

            if (_Characteristics != null)
            {
                if (_ReadChar != null && _NotifyChar != null)
                {
                    _ReadChar.ValueChanged -= _Read;
                    _NotifyChar.ValueChanged -= _Read;
                }
                _Characteristics = null;
            }

            if (_Service != null)
            {
                _Service.Dispose();
                _Service = null;
            }

            if (_Device != null)
            {
                _Device.ConnectionStatusChanged -= _Device_ConnectionStatusChanged;
                _Device.GattServicesChanged -= _Device_GattServicesChanged;
                _Device.Dispose();
                _Device = null;
            }

            //}
            GC.Collect();

            BluetoothLEDevice.FromIdAsync(id)
                .AsTask()
                .ContinueWith(r1 =>
                {
                    _Device = r1.Result;
                    BluetoothRadio.StateChanged += _BluetoothRadio_StateChanged;
                    _Device.ConnectionStatusChanged += _Device_ConnectionStatusChanged;
                    _Device.GattServicesChanged += _Device_GattServicesChanged;
                    _Device.GetGattServicesForUuidAsync(HIOServiceUuid)
                    .AsTask()
                    .ContinueWith(r2 =>
                    {
                        _Service = r2.Result.Services.First();
                        if (_Characteristics == null)
                            _Characteristics = new List<GattCharacteristic>();
                        lock (_Characteristics)
                        {
                            GetCharacteristics();
                            Trace.WriteLine($"Number of chars: {_Characteristics?.Count ?? -1}");
                        }
                    })
                    .Wait();
                })
                .Wait();

            Monitor.Enter(_ReadLock);

            Monitor.Pulse(_ReadLock);

            Monitor.Exit(_ReadLock);
        }

        private void _Device_GattServicesChanged(BluetoothLEDevice sender, object args)
        {
            SendConnectedEvent();
        }

        public byte[] mac
        {
            get
            {
                if (_Device.DeviceInformation == null)
                    return Enumerable.Empty<byte>().ToArray();
                return _Device.DeviceId.Substring(_Device.DeviceId.IndexOf("-") + 1).Split(':').Select(x => Converts.HexStringToByteArray(x)[0]).ToArray();
            }
        }
        public byte[] name
        {
            get
            {
                if (_Device == null)
                    return Enumerable.Empty<byte>().ToArray();
                var result = Encoding.UTF8.GetBytes(_Device.Name);
                if (result.Length < 32)
                    Array.Resize(ref result, 32);
                return result;
            }
        }
        private bool _BluetoothRadioChanged = false;
        private void _BluetoothRadio_StateChanged(Radio radio, object args)
        {
            Trace.WriteLine($"BluetoothRadio_StateChanged: Radio State: {radio.State} Device Connection State: {_Device.ConnectionStatus} {DateTime.Now}");
            if (radio.State == RadioState.Off)
            {
                //SendDisconnectedEvent();
                HIOStaticValues.CONNECTIONBRIDGE = false;
                _BluetoothRadioChanged = true;
            }
        }
        private void SendDisconnectedEvent()
        {
            HIOStaticValues.CONNECTIONBRIDGE = false;
            //Dispose();
            Task.Run(() =>
            {
                Thread.Sleep(5000);
                if (_Device?.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
                    LockWorkStation();
            });

            //Monitor.Enter(_ReadLock);
            //_Queue.Enqueue(Enumerable.Range(1, 20).Select((item, index) => (byte)(index == 0 ? (byte)HIO.Backend.Type.EVENTS : index == 5 ? 0x70 : 0x0)).ToArray());
            //Monitor.Pulse(_ReadLock);
            //Monitor.Exit(_ReadLock);
        }

        public async void SendConnectedEvent()
        {
            if (!await BLEBrowser.IsConnected(_Device))
                return;
            HIOStaticValues.DirectBluetooth = true;
            HIOStaticValues.EventCheckDevice.Set();
            HIOStaticValues.commandQ.Add(() =>
            {
                Commands ic = new Commands();
                ic.IsConnection();

            });
            //HIOStaticValues.EventCheckDevice.Reset();
        }

        private void _Device_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            Trace.WriteLine($"Device_ConnectionStatusChanged: {_Device.ConnectionStatus} {DateTime.Now}");
            if (_Device.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
            {
                if (!_BluetoothRadioChanged)
                    SendDisconnectedEvent();
                else
                    _BluetoothRadioChanged = false;
            }
            else
            {
                SendConnectedEvent();
            }
        }

        private void GetCharacteristics()
        {
            _Service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached)
                            .AsTask()
                            .ContinueWith(async r3 =>
                            {
                                _Characteristics = r3.Result.Characteristics.ToList();
                                if (_Characteristics?.Count != 4)
                                {
                                    Thread.Sleep(300);
                                    GetCharacteristics();
                                }
                                else
                                {
                                    _WriteChar = _Characteristics.First(c => c.Uuid == WriteCharUUID);
                                    _ReadChar = _Characteristics.First(c => c.Uuid == ReadCharUUID);
                                    _NotifyChar = _Characteristics.First(c => c.Uuid == NotifyCharUUID);
                                    _ReadChar.ValueChanged += _Read;
                                    _NotifyChar.ValueChanged += _Read;
                                    await _ReadChar.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                                    await _NotifyChar.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                                }
                            })
                            .Wait();
        }
        private readonly ConcurrentQueue<Byte[]> _Queue = new ConcurrentQueue<byte[]>();
        private void _Read(GattCharacteristic s, GattValueChangedEventArgs e)
        {
            Monitor.Enter(_ReadLock);
            _Queue.Enqueue(e.CharacteristicValue.ToArray());
            Monitor.PulseAll(_ReadLock);
            Monitor.Exit(_ReadLock);
        }


        private readonly byte[] cid = new byte[] { 0, 0, 0, 0, 1 };
        private readonly object _ReadLock = new object();
        public void Read(byte[] data)
        {
            byte[] result = null;
            //while (_Queue.Count == 0) { }
            Monitor.Enter(_ReadLock);
            while (!_Queue.TryDequeue(out result))
            {
                Monitor.Wait(_ReadLock);
                if (_Device == null)
                    throw new DeviceIsDisposedException();
            }
            Monitor.Exit(_ReadLock);
            cid.Concat(result).ToArray().CopyTo(data, 0);
        }
        public bool Write(byte[] data)
        {
            try
            {
                GattCommunicationStatus result = GattCommunicationStatus.Success;
                lock (_lock)
                {
                    _WriteChar.WriteValueAsync(data.Skip(5).Take(20).ToArray().AsBuffer())
                        .AsTask()
                        .ContinueWith(r =>
                        {
                            if (r.Status == TaskStatus.Faulted)
                            {
                                var name = _Device.Name;
                                var id = _Device.DeviceId;
                                result = GattCommunicationStatus.Unreachable;
                                if (r.Exception is AggregateException aex && aex.InnerExceptions.Any(e => e is ObjectDisposedException))
                                    Reopen();
                            }
                            else
                                result = r.Result;
                        })
                        .Wait();
                }
                Trace.WriteLine("Send:\n" + Converts.ByteArrayToString(data.Skip(5).Take(20).ToArray()));

                return result == GattCommunicationStatus.Success;
            }
            catch (Exception ex)
            {
                
                return false;
            }

        }


        private static readonly Commands commands = new Commands();
        public static SignalEnum GetSignal(IReadOnlyDictionary<string, object> properties)
        {
            return commands.GetSignalStatus((byte)GetSignalValue(properties));
        }

        public static byte GetSignalValue(IReadOnlyDictionary<string, object> properties)
        {
            if (properties == null || !properties.ContainsKey(SignalStrengthProperty) || properties[BLEDevice.SignalStrengthProperty] == null)
                return 0;
            int.TryParse(properties[SignalStrengthProperty].ToString(), out int sv);
            return (byte)Math.Abs(sv);
        }

        public byte GetSignalValue()
        {
            DeviceInformation info = null;
            DeviceInformation
               .CreateFromIdAsync(_Device.DeviceId, new string[] { SignalStrengthProperty })
               .AsTask()
               .ContinueWith(r => info = r.Result)
               .Wait();
            if (info == null)
                return 0;
            return GetSignalValue(info.Properties);
        }

        public async Task Unpair()
        {
            var errorHandle = new ErrorHandle();
            try
            {
                if (_Device == null)
                    return;
                _Device.ConnectionStatusChanged -= _Device_ConnectionStatusChanged;
                await _Device.DeviceInformation.Pairing.UnpairAsync();
                errorHandle.logEvent($"Unpair BLE: {_Device.Name}");
                Dispose();
            }
            catch (Exception ex)
            {
               
            }
        }
    }

    public class DeviceIsDisposedException : Exception
    {
        public override string Message => "Device is disposed.";
    }
}
