using HIO.Controls;
using HIO.Extentions;
using HIO.ViewModels.Accounts;
using HIO.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace HIO.Backend
{

    public class Commands
    {
        public bool IsConnection()
        {
            if (HIOStaticValues.BaS.CheckConnection() == 1)
                return true;
            else
                return false;
        }
        public void SetRssiTH(byte value)
        {
            if (HIOStaticValues.DirectBluetooth)
            {
                HIOStaticValues.BaS.SetRSSITHBLE(value);
            }
            else
            {
                HIOStaticValues.BaS.SetRSSITH(value);
                HIOStaticValues.BaS.SetRSSITHBLE(value);

            }
            
        }
        public bool GetRssi()
        {
            if (HIOStaticValues.BaS.GetRssi() == 1)
                return true;
            else
                return false;
        }
        public int Insert(string website, string username, string title, string password)
        {
            DataBase db = new DataBase();
            int res = -1;
            if (db.getInfoFromDB(website, username, title).Count == 0)
            {

                while (true)
                {
                    res = HIOStaticValues.BaS.AmISyncedSwitch();
                    if (res == 1)
                    {

                        break;
                    }
                    else if (res == -2)
                    {
                        HIOStaticValues.popUp("HIO is not connected");
                        return 0;
                    }
                    else

                        HIOStaticValues.SyncOpration();



                }
                res = HIOStaticValues.BaS.InsertToSwitch(new LoginFieldS { url = website, userName = username, title = title, password = password, appID = "" });
                if (res > 0)
                {
                    db.insertToDatabase(res.ToString(), website, username, "", title);
                    HIOStaticValues.username = username;
                    return 1;
                }

                else return 0;

            }
            else
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    HIOStaticValues.popUp("This user already exists,You can modify it in the Edit Form.");

                }));
                return -4;
            }
        }
        public bool Delete(string rowid)
        {


            DataBase db = new DataBase();

            int rowidInt = Int32.Parse(rowid);
            byte[] rowidByteArray = BitConverter.GetBytes(rowidInt).ToArray();
            string result = HIOStaticValues.BaS.DeleteFromSwitch(rowidByteArray);
            if (result == StatusWord.SW_NO_ERROR)
            {
                db.deleteFromDatabase(rowid);
                return true;
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    HIOStaticValues.popUp("Something was wrong!\nPlease check log file.");

                }));

            }
            return false;
        }

        public List<TDevice> GetListBLE()
        {
            List<TDevice> lstBLEBytes = new List<TDevice>();
            if (HIOStaticValues.CONNECTIONBRIDGE == true)
            {
                lstBLEBytes = HIOStaticValues.BaS.ScanBluetooth();
            }
            return lstBLEBytes;
        }

        public string Bond(byte[] mac, byte[] pin)
        {
            Trace.WriteLine("Bond");
            var res = HIOStaticValues.BaS.BondBridge(mac, pin);
            return res;
        }

        public bool ResetSync()
        {
            Trace.WriteLine("ResetSync");
            return HIOStaticValues.BaS.SwitchResetSync();
        }

        public bool UnBond(byte[] mac)
        {
            Trace.WriteLine("UnBond");
            return HIOStaticValues.BaS.UnBondSwitch(mac);
        }

        public SyncronizingStateEnum AmISync()
        {
            int res = HIOStaticValues.BaS.AmISyncedSwitch();
            if (res == 1)
                return SyncronizingStateEnum.Synced;
            else
            {

                return SyncronizingStateEnum.NotSynced;
            }
        }
        public bool EraseAll()
        {
            try
            {
                if (HIOStaticValues.BaS.EraseAllDataSwitch())
                {
                    HIOStaticValues.DirectBluetooth = false;
                    HIOStaticValues.Disconnect_Click(null, null);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
            
                return false;
            }
        }
        public SignalEnum GetSignalStatus(byte rssi)
        {
            int rssiInt = rssi;
            rssiInt = 255 - rssiInt;
            if (rssiInt > 80)
                return SignalEnum.Low;
            else if (rssiInt <= 80 && rssiInt > 60)
                return SignalEnum.Medium;
            else if (rssiInt <= 60 && rssiInt > 1)
                return SignalEnum.Full;

            else return SignalEnum.NoConnection;
        }
        public BatteryStateEnum GetBatteryStatus()
        {
            int res = HIOStaticValues.BaS.BatteryStatus();
            if (res > 60)
                return BatteryStateEnum.Full;
            else if (res <= 60 && res > 30)
                return BatteryStateEnum.Medium;
            else if (res <= 30 && res > 1)
                return BatteryStateEnum.Low;

            else return BatteryStateEnum.Empty;
        }

        public int Sync()
        {
            try
            {
                Trace.WriteLine("Sync");
                SyncronizingStateEnum result = 0;
                int res = -1;
                do
                {
                    HIOStaticValues.SYNC_ON = true;
                    Trace.WriteLine("SYNC_ON == true");


                    HIOStaticValues.tmain.TabManager.ActiveTab = HIOStaticValues.tmain.AccountManager;

                    Trace.WriteLine("change tab");
                    res = HIOStaticValues.BaS.SyncSwitch();
                    Trace.WriteLine("result " + res);
                    Trace.WriteLine("SYNC_ON == false");
                    HIOStaticValues.SYNC_ON = false;
                    if (res < 0) return res;
                    result = AmISync();


                } while (result != SyncronizingStateEnum.Synced);

                return res;
            }
            catch (Exception e)
            {
              
                return -4;
            }
            finally
            {
                HIOStaticValues.ProgressPercent = 0;
            }
        }

        public int GetVersion()
        {
            string ver = HIOStaticValues.BaS.SwitchGetVersion();

            HIOStaticValues.blea.ver = ver;
            return 1;
        }

        public List<BluetoothList> PairedList()
        {
            List<BluetoothList> listBLE = HIOStaticValues.BaS.GetBondedListBridge();
            return listBLE;


        }

        public bool UnBondAll()
        {
            throw new NotImplementedException();
        }

        public StatusPassword GetPassword(byte[] rowid)
        {
            return HIOStaticValues.BaS.GetPassFromSwitch(rowid);
        }

        public bool UpdateUser(string ID, string url, string appID, string title, string username, string password, byte flagPass)
        {
            DataBase db = new DataBase();
            string urlFilter = HIOStaticValues.getDomainName(url);
            if (HIOStaticValues.BaS.UpdateSwitch(ID, urlFilter, appID, title, username, password, flagPass) == 0)
            {
                db.UpdateDBFromRowID(ID, urlFilter, appID, title, username);
                return true;
            }
            return false;
        }
        public int ChangePin(string oldPin, string newPin)
        {

            try
            {
                string res = HIOStaticValues.BaS.SwitchChangeDevicePin(Encoding.UTF8.GetBytes(oldPin), Encoding.UTF8.GetBytes(newPin));
                switch (res)
                {
                    case StatusWord.SW_NO_ERROR:
                        return 1;
                    case StatusWord.SW_LOCKER_MOD_1:
                        return -1;

                    case StatusWord.SW_LOCKER_MOD_2:
                        return -2;

                    case StatusWord.SW_LOCKER_MOD_3:
                        return -3;

                    case StatusWord.SW_LOCKER_MOD_4:
                        return -4;
                    case StatusWord.SW_PIN_NOT_VERIFIED:
                        return 0;
                    default:
                        return -5;

                }



            }
            catch (Exception ex)
            {
             
                return 0;
            }

        }
        public CheckPinStatus CheckPin()
        {
            try
            {
                return HIOStaticValues.BaS.SwitchCheckDevicePin();
            }
            catch (Exception ex)
            {
                return CheckPinStatus.Disabled;
            }
        }
        public int DisablePin(string pin)
        {
            try
            {
                string res = HIOStaticValues.BaS.SwitchDisableDevicePin(Encoding.UTF8.GetBytes(pin));
                switch (res)
                {
                    case StatusWord.SW_NO_ERROR:
                        return 1;
                    case StatusWord.SW_LOCKER_MOD_1:
                        return -1;

                    case StatusWord.SW_LOCKER_MOD_2:
                        return -2;

                    case StatusWord.SW_LOCKER_MOD_3:
                        return -3;

                    case StatusWord.SW_LOCKER_MOD_4:
                        return -4;
                    case StatusWord.SW_PIN_NOT_VERIFIED:
                        return 0;
                    default:
                        return -5;

                }



            }
            catch (Exception ex)
            {
              
                return 0;
            }
        }
        public int SetPin(string pin)
        {
            try
            {
                string res = HIOStaticValues.BaS.SwitchEnableDevicePin(pin.GetUTF8Bytes(6));
                switch (res)
                {
                    case StatusWord.SW_PIN_NOT_VERIFIED:
                        return 0;
                    case StatusWord.SW_NO_ERROR:
                    case StatusWord.SW_ALREADY_AUTHENTICATED:
                        return 1;
                    case StatusWord.SW_LOCKER_MOD_1:
                        return -1;
                    case StatusWord.SW_LOCKER_MOD_2:
                        return -2;
                    case StatusWord.SW_LOCKER_MOD_3:
                        return -3;
                    case StatusWord.SW_LOCKER_MOD_4:
                        return -4;
                    default:
                        return -5;

                }


            }
            catch (Exception ex)
            {
              
                return 0;
            }
        }
       
    }



}
