using HIO;
using HIO.Backend;
using HIO.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Radios;

namespace Mighty.HID
{
    public class BLEBrowser
    {
        private static readonly string[] requestedProperties = { BLEDevice.DeviceAddressProperty, BLEDevice.IsConnectedProperty, BLEDevice.IsConnectableProperty, BLEDevice.SignalStrengthProperty };
        private readonly ErrorHandle _errorHandler = new ErrorHandle();
        public BLEBrowser()
        {
        }

        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public async Task<IReadOnlyList<TDevice>> Browse()
        {
            var devices = await DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelectorFromConnectionStatus(BluetoothConnectionStatus.Connected), requestedProperties);
            return devices.Select(d => new TDevice(HIOStaticValues.tmain?.SettingManager,
                                d.Name,
                                id: d.Id,
                                mac: GetMac(d),
                                signalValue: BLEDevice.GetSignal(d.Properties),
                                isConnected: d.Properties.ContainsKey(BLEDevice.IsConnectableProperty) ? (bool)d.Properties[BLEDevice.IsConnectedProperty] : false)).ToList();

        }

        private DeviceWatcher _Watcher;
        public void StartScan(ICollection<TDevice> devices, Action onChangeCallback = null)
        {
            devices.Clear();
            if (_Watcher == null)
            {
                Radio radio = null;
                Task.Run(async () =>
                {
                    radio = (await Radio.GetRadiosAsync()).FirstOrDefault(r => r.Kind == RadioKind.Bluetooth);
                    if (radio != null)
                        radio.StateChanged += (r, args) =>
                        {
                            if (radio.State == RadioState.Off) App.Current.Dispatcher.Invoke(() => devices.Clear());
                        };
                });

                _Watcher = DeviceInformation.CreateWatcher(BluetoothLEDevice.GetDeviceSelectorFromConnectionStatus(BluetoothConnectionStatus.Connected), requestedProperties);

                _Watcher.Added += async (s, e) =>
                {
                    await semaphoreSlim.WaitAsync();
                    try
                    {
                        var di = await BluetoothLEDevice.FromIdAsync(e.Id);
                        if (di != null && await IsConnected(di) && await IsHIO(di) && !devices.Any(i => i.Id == e.Id))
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            devices.Add(new TDevice(HIOStaticValues.tmain?.SettingManager,
                                e.Name,
                                id: e.Id,
                                mac: GetMac(e),
                                signalValue: BLEDevice.GetSignal(e.Properties),
                                isConnected: e.Properties.ContainsKey(BLEDevice.IsConnectableProperty) ? (bool)e.Properties[BLEDevice.IsConnectedProperty] : false)));
                            onChangeCallback?.Invoke();
                        }
                    }
                    catch (Exception ex)
                    {
                        _errorHandler.ErrorFunc(ex);
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                };

                _Watcher.Updated += async (s, e) =>
                {
                    await semaphoreSlim.WaitAsync();
                    try
                    {
                        if (!devices.Any(i => i.Id == e.Id))
                        {
                            var di = await BluetoothLEDevice.FromIdAsync(e.Id);
                            if (await IsConnected(di) && await IsHIO(di))
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                devices.Add(new TDevice(HIOStaticValues.tmain?.SettingManager,
                                    di.Name,
                                    id: di.DeviceId,
                                    signalValue: BLEDevice.GetSignal(e.Properties),
                                    mac: GetMac(di))));
                                onChangeCallback?.Invoke();

                            }
                            else
                            {

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _errorHandler.ErrorFunc(ex);
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                };

                _Watcher.Removed += (s, e) =>
                {
                    semaphoreSlim.WaitAsync();
                    try
                    {
                        App.Current.Dispatcher.Invoke(() => devices.Remove(devices.FirstOrDefault(i => i.Id == e.Id)));
                        onChangeCallback?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _errorHandler.ErrorFunc(ex);
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                };

                _Watcher.Start();
            }
            else if (_Watcher.Status == DeviceWatcherStatus.Created ||
                _Watcher.Status == DeviceWatcherStatus.Stopped ||
                _Watcher.Status == DeviceWatcherStatus.EnumerationCompleted ||
                _Watcher.Status == DeviceWatcherStatus.Aborted)
            {
                if (_Watcher.Status == DeviceWatcherStatus.EnumerationCompleted)
                {
                    _Watcher.Stop();
                    _Watcher = null;
                    StartScan(devices, onChangeCallback);
                    return;
                }
                _Watcher.Start();
            }
        }

        public void StopScan()
        {
            if (_Watcher == null)
                return;
            if (_Watcher.Status != DeviceWatcherStatus.Stopped && _Watcher.Status != DeviceWatcherStatus.Stopping)
                _Watcher.Stop();
        }

        public static BLEDeviceInfo FindConnectedDevice()
        {
            if (!HIOStaticValues.IsBLESupported)
                return null;
            BLEDeviceInfo result = null;
            Task.Run(async () =>
            {
                try
                {
                    var devices = await DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelectorFromConnectionStatus(BluetoothConnectionStatus.Connected), requestedProperties);
                    if (Array.TrueForAll(HIOStaticValues.blea.mac, v => v == 0))
                    {
                        var db = new DataBase();
                        var lastMac = db.GetLastDeviceMac();
                        if (lastMac != null)
                            Array.Copy(lastMac, HIOStaticValues.blea.mac, 6);
                    }

                    foreach (var di in devices.Where(d => GetMac(d).SequenceEqual(HIOStaticValues.blea.mac)))
                    {
                        try
                        {
                            var device = await BluetoothLEDevice.FromIdAsync(di.Id);
                            if (await IsConnected(device) && await IsHIO(device))
                            {
                                return new BLEDeviceInfo(device.Name, device.DeviceId);
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {

                }
                return null;
            })
            .ContinueWith(r =>
            {
                result = r.Result;
            })
            .Wait();

            return result;
        }

        private static byte[] GetMac(BluetoothLEDevice device)
        {
            return device.DeviceId.Substring(device.DeviceId.IndexOf("-") + 1).Split(':').Select(x => Converts.HexStringToByteArray(x)[0]).ToArray();
        }


        private static byte[] GetMac(DeviceInformation e)
        {
            return e.Id.Substring(e.Id.IndexOf("-") + 1).Split(':').Select(x => Converts.HexStringToByteArray(x)[0]).ToArray();
        }

        private static async Task<bool> IsHIO(BluetoothLEDevice device)
        {
            var result = device != null && (await device.GetGattServicesAsync()).Services.Any(s => s.Uuid == BLEDevice.HIOServiceUuid);
            return result;
        }

        public static async Task<bool> IsConnected(BluetoothLEDevice device)
        {
            if (device?.ConnectionStatus != BluetoothConnectionStatus.Connected)
                return false;
            var info = await DeviceInformation.CreateFromIdAsync(device.DeviceId);
            return info?.Pairing.IsPaired ?? false;
        }
    }
}
