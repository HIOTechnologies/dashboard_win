using HIO.Controls;
using HIO.Extentions;
using HIO.ViewModels;
using HIO.ViewModels.Accounts;
using HIO.ViewModels.Notify;
using HIO.ViewModels.Security;
using Microsoft.Win32;
using Mighty.HID;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WindowsInput;

namespace HIO.Backend
{
    public class StatusWord
    {
        public static Dictionary<string, string> sw =
             new Dictionary<string, string>() {

                { "9000" , "SW_NO_ERROR"},
        { "6985","SW_CONDITIONS_NOT_SATISFIED"},
        {"6996","SW_SYNC_IS_REQUIRED"},
        { "6AF0","SW_WRONG_DATA"},
        {"6D00","SW_INS_NOT_SUPPORTED"},
        { "9210","SW_BRIDGE_INSUFFICIENT_MEMORY"},
        {"6387","SW_MEM_IS_FULL"},
        {"6901","SW_BUSY_STATE"},
        { "9E04","SW_PIN_NOT_VERIFIED"},
        {"6600","SW_RECEIVE_TIMEOUT"},
        {"9580","SW_BAD_SEQUENCE"},
        {"9862","SW_APP_NOT_INRODUCED"},
        {"9240","SW_BRIDGE_BOND_FULL"},
        { "9681","SW_BLE_CONNECTION_IS_NOT_AVAILABLE"},
        { "6501","SW_FS_WRITE_READ_ERROR"},
        {"6A89","SW_DURING_INITIATING_BLE_CONNECTION"},
        {"6A82","SW_COULD_NOT_START_RSSI_MEASURMENT_ENGINE"},
        {"6282","SW_DURING_STABLISHING_BLE_LINK"},
        {"6283","SW_ALREADY_AUTHENTICATED"},
        {"63C1","SW_LOCKER_MOD_1"},
        {"63C2","SW_LOCKER_MOD_2"},
        {"63C3","SW_LOCKER_MOD_3"},
        {"63C4","SW_LOCKER_MOD_4"},

     };


        public const string SW_NO_ERROR = "9000";
        public const string SW_CONDITIONS_NOT_SATISFIED = "6985";
        public const string SW_SYNC_IS_REQUIRED = "6996";
        public const string SW_WRONG_DATA = "6AF0";
        public const string SW_INS_NOT_SUPPORTED = "6D00";
        public const string SW_BRIDGE_INSUFFICIENT_MEMORY = "9210";
        public const string SW_MEM_IS_FULL = "6387";
        public const string SW_BUSY_STATE = "6901";
        public const string SW_PIN_NOT_VERIFIED = "9E04";
        public const string SW_RECEIVE_TIMEOUT = "6600";
        public const string SW_BAD_SEQUENCE = "9580";
        public const string SW_APP_NOT_INRODUCED = "9862";
        public const string SW_BRIDGE_BOND_FULL = "9240";
        public const string SW_BLE_CONNECTION_IS_NOT_AVAILABLE = "9681";
        public const string SW_FS_WRITE_READ_ERROR = "6501";
        public const string SW_DURING_INITIATING_BLE_CONNECTION = "6A89";
        public const string SW_COULD_NOT_START_RSSI_MEASURMENT_ENGINE = "6A82";
        public const string SW_DURING_STABLISHING_BLE_LINK = "6282";
        public const string SW_ALREADY_AUTHENTICATED = "6283";
        public const string SW_LOCKER_MOD_1 = "63C1";
        public const string SW_LOCKER_MOD_2 = "63C2";
        public const string SW_LOCKER_MOD_3 = "63C3";
        public const string SW_LOCKER_MOD_4 = "63C4";
    }
   
    public class StatusPassword
    {
        public string pass;
        public byte[] statusWord = new byte[2];
    }

    public class BLEActive
    {
        public byte[] mac = new byte[6];
        public byte[] name = new byte[32];
        public byte rssi;
        public string ver;
    }
    public class BluetoothList
    {
        public byte[] name = new byte[32];
        public byte[] mac = new byte[6];
        public byte rssi;
    }
    public class ListDevices
    {
        public string name { get; set; }
        public string rssi { get; set; }
        public string mac { get; set; }
        public string rowid { get; set; }
        public string serial { get; set; }
        public string datetime { get; set; }


    }
    public class LoginFieldS
    {
        public byte[] recordKey { get; set; }
        public string url { get; set; }
        public string action { get; set; }
        public string userName { get; set; }
        public byte isPassFlag { get; set; }
        public string password { get; set; }
        public string title { get; set; }
        public string appID { get; set; }
        public string rowid { get; set; }
        public string last_used { get; set; }
        public byte[] imageData { get; set; }
        public int popularity { get; set; }
      
    }
    class loginFieldsCompare : IEqualityComparer<LoginFieldS>
    {
        public bool Equals(LoginFieldS x, LoginFieldS y)
        {
            return x.title == y.title && x.userName == y.userName && x.url == y.url;
        }

        public int GetHashCode(LoginFieldS obj)
        {

            return (obj.url+obj.userName+obj.title).GetHashCode();
        }
    }
    public static class HIOStaticValues
    {
        private static System.Windows.Forms.MenuItem menuItem1 = new System.Windows.Forms.MenuItem();
        private static System.Windows.Forms.MenuItem menuItem2 = new System.Windows.Forms.MenuItem();
        private static System.Windows.Forms.MenuItem menuItem3 = new System.Windows.Forms.MenuItem();
        public static int CounterTimerIcon=0; 
        public static TAdminExtention AdminExtention = new TAdminExtention();
        public static double ProgressPercent = 0;
        public static AutoResetEvent _signalRec = new AutoResetEvent(false);
        public static AutoResetEvent _signalSendCMD = new AutoResetEvent(false);
        public static AutoResetEvent _signalCheckDevice = new AutoResetEvent(false);
        public static AutoResetEvent EventCheckDevice = new AutoResetEvent(false);
        public static System.Windows.Forms.NotifyIcon CTicon { get; set; }
        public static string PATHMSGEDGE = @".\Private$\MSGHIO1Edge" + System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\').Last();
        public static string PATHMSGFF = @".\Private$\MSGHIO1FF" + System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\').Last();
        public static string PATHMSGCHROME = @".\Private$\MSGHIO1Chrome" + System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\').Last();
        public static string PATHMSGREC = @".\Private$\MSGHIO2" + System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\').Last();
        public static bool SYNC_ON = false;
        public static bool IMPORT_ON = false;
        public static bool ISLOCK = false;
        public static readonly object Cmdlocker = new object();
        public static bool checkRec = false; //checking recieve packet from extention(flag)
        public static Double scale = 1; //scale monitor
        public static bool FormHide; //check Dashboard is open or not
        public static string username = "";
        public static BLEActive blea = new BLEActive();
        public static string password = "";
        public static InputSimulator sim = new InputSimulator();
        public static BridgeandSwitch BaS = new BridgeandSwitch();
        public static TMain tmain = null;
        public static QueueManager commandQ = new QueueManager();
        public static Queue eventQ = new Queue();
        public static bool AbortThread = false;
        public static int SOURCE; 
        public static TPinInputViewModel PinInputDashboardVM = new TPinInputViewModel();
        public static TPinInputViewModel PinInputExtensionVM = new TPinInputViewModel();
        const string userRoot = "HKEY_CURRENT_USER\\Software";
        const string subkey = "HIO";
        const string keyName = userRoot + "\\" + subkey;
        private static bool? _isBLESupported;
        public static bool IsBLESupported
        {
            get
            {
                try
                {
                    if (_isBLESupported == null)
                    {
                        var dummy = System.Type.GetType("Windows.Devices.Radios.RadioKind, Windows.Devices, Version=255.255.255.255, Culture=neutral, PublicKeyToken=null, ContentType=WindowsRuntime");
                        _isBLESupported = dummy != null;
                    }
                    return _isBLESupported.Value;
                }
                catch
                {
                    return false;
                }
            }
        }
        public static bool DirectBluetooth
        {
            get
            {
                if (!IsBLESupported)
                    return false;
                int res = (int)Registry.GetValue(keyName, "DirectBluetooth", 0);
                return (res == 1) ? true : false;
            }
            set
            {
                if (!IsBLESupported)
                    return;

                Registry.SetValue(keyName, "DirectBluetooth", (value == true) ? 1 : 0, RegistryValueKind.DWord);
                if (value)
                {
                    CONNECTIONBHIO = true;
                    tmain.IsConnected = true;
                }
                else
                {
                    CONNECTIONBRIDGE = false;
                    _signalCheckDevice.Set();
                }
            }

        }
        private static bool _CONNECTIONBRIDGE = false;
        public static bool CONNECTIONBRIDGE
        {
            get
            {
                return _CONNECTIONBRIDGE;
            }
            set
            {
                _CONNECTIONBRIDGE = value;
                if (value)
                {
                    menuItem1.Visible = true;
                    if (CONNECTIONBHIO)
                        menuItem1.Text = "&Disconnect";
                    else
                        menuItem1.Text = "Add &new device";
                }
                else
                {
                    menuItem1.Visible = false;
                    CTicon.Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/HIO;component/Resources/HIO2.ico")).Stream);
                    tmain.IsConnected = false;
                    Trace.WriteLine("******* ConnectionBridge");
                    CONNECTIONBHIO = false;
                    Array.Clear(blea.mac, 0, blea.mac.Length);
                    Array.Clear(blea.name, 0, blea.name.Length);
                }


            }
        }
        private static bool _CONNECTIONBHIO = false;
        public static bool CONNECTIONBHIO
        {
            get
            {
                return _CONNECTIONBHIO;

            }
            set
            {
                _CONNECTIONBHIO = value;

                if (value)
                {

                    if (tmain.CanShowSetup == false && tmain.GetType().GetField("_Form", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tmain) == null)//access to _form properties private
                    {

                        popUp("HIO is Connected");
                        Registry.SetValue(keyName, "StatusConnectionPopUp", 0, RegistryValueKind.DWord); //check if apear popup for status connection or not

                    }

                    CONNECTIONBRIDGE = true;
                    tmain.IsConnected = true;
                    menuItem1.Visible = true;
                    menuItem1.Text = "&Disconnect";
                    CTicon.Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/HIO;component/Resources/icon-16.ico")).Stream);
                    commandQ.Add(() =>
                    {
                        DataBase db = new DataBase();
                        db.CheckData();
                    });
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        foreach (Window window in System.Windows.Application.Current.Windows)
                        {

                            if ((window as TWindow)?.canCloseonDC == true)
                                window.Close();



                        }
                    }));
                    tmain.IsPinRequired = false;
                    if (tmain.CanShowSetup == false && (int)Registry.GetValue(keyName, "StatusConnectionPopUp", 0) == 0 && tmain.GetType().GetField("_Form", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tmain) == null) //access to _form properties private
                    {
                        popUp("HIO is Disconnected!");
                        Registry.SetValue(keyName, "StatusConnectionPopUp", 1, RegistryValueKind.DWord); //check if apear popup for status connection or not

                    }

                    menuItem1.Visible = true;
                    menuItem1.Text = "Add &new device";
                    CTicon.Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/HIO;component/Resources/HIO2.ico")).Stream);

                    tmain.IsConnected = false;
                    tmain.SettingManager.MyHIOTitle = "HIO is disconnected";
                    tmain.SettingManager.MyHIOVersion = "HIO is disconnected";
                    tmain.SettingManager.MyHIOMac = "HIO is disconnected";
                    Array.Clear(HIOStaticValues.blea.name, 0, HIOStaticValues.blea.name.Length);
                    Array.Clear(HIOStaticValues.blea.mac, 0, HIOStaticValues.blea.mac.Length);
                }


            }

        }
        public static void popUp(string text)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
               {
                   AdminExtention.CloseAll();
                   AdminExtention.Extention13.Show(text);
               }));
            }
            catch (Exception ex)
            {
               
            }
        }

        private static bool IsValidIP(string Address)
        {
            try
            {
                IPAddress ip;
                if (IPAddress.TryParse(Address, out ip))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex) { return false; }
        }
        public static string getDomainNameURI(string strURL)
        {

            try
            {
                List<string> protocolList = new List<string>();
                protocolList.Add("http://");
                protocolList.Add("ftp://");
                protocolList.Add("https://");
                int index = -1;
                foreach (string protocol in protocolList)
                {

                    index = strURL.ToLower().IndexOf(protocol);
                    if (index > -1) break;
                }

                if (index == -1) return getDomainName(strURL);
                Uri myUri = new Uri(strURL);
                string host = myUri.Host;
                if (IsValidIP(host)) return host;
                return getDomainName(host);
            }
            catch (Exception ex)
            {
                return strURL;
            }

        }
        public static string getTitleNameURI(string strURL)
        {
            try
            {
                Uri myUri = new Uri(strURL);
                string host = myUri.Host;
                if (IsValidIP(host)) return host;
                return getTitleName(host);

            }
            catch
            {


                return strURL;
            }
        }
        public static void CheckingData(TAccountItem account)
        {
            Converts conv = new Converts();
            ////////////////////////////////////////////
            account.Url = (account.Url == null || account.Url == "") ? "" : HIOStaticValues.getDomainName(account.Url.ToLower());
            var urlByteArray = account.Url.GetUTF8Bytes(256);
            account.Url = UnicodeEncoding.UTF8.GetString(urlByteArray);
            //proccess unicode character and get len of string
            account.Name = (account.Name == null || account.Name == "") ? "" : (account.Name.Length < 65) ? account.Name : account.Name.Substring(0, 64);
            var titleByteArray = account.Name.GetUTF8Bytes(256);
            account.Name = UnicodeEncoding.UTF8.GetString(titleByteArray);
            //////////////////////
            account.Password = (account.Password == null || account.Password == "") ? "" : (account.Password.Length < 65) ? account.Password : account.Password.Substring(0, 64);
            var passByteArray = account.Password.GetUTF8Bytes(256);
            account.Password = UnicodeEncoding.UTF8.GetString(passByteArray);
            ////////////////////////////////////////
            account.Username = (account.Username == null || account.Username == "") ? "" : (account.Username.Length < 65) ? account.Username : account.Username.Substring(0, 64);
            if (account.Username != "")
                HIOStaticValues.username = account.Username;//check username(if user want just fill password element and username element filled already by self)
            var userByteArray = account.Username.GetUTF8Bytes(256);
            account.Username = UnicodeEncoding.UTF8.GetString(userByteArray);
            ////////////////////////////////////////////

        }
        public static bool CheckSyncingData()
        {
            //SYNC_ON = true; for test
            if (SYNC_ON == true)
            {
                if (tmain == null)
                    popUp("Please wait...\nHIO is syncing data.");
                return true;
            }
            else if (IMPORT_ON == true)
            {
                if (tmain == null)
                    popUp("Please wait...\nHIO is importing data.");
                return true;

            }
            return false;
        }
        public static string getDomainName(string strURL)
        {
            try
            {
                try
                {
                    Uri uri = new Uri(strURL);
                    strURL = uri.Host;
                }
                catch
                {
                }
                string host = "";
                if (IsValidIP(strURL))
                {
                    return strURL;
                }
                string[] nodes = strURL.Split('.');
                host = nodes[nodes.Length - 2] + "." + nodes[nodes.Length - 1];
                return host.Replace("/", "");

            }
            catch (Exception ex)
            {
                 return strURL;
            }
        }
        public static string getTitleName(string strURL)
        {
            try
            {
                string host = "";

                string[] nodes = strURL.Split('.');
                if (nodes.Length > 2)
                {
                    int startNode = 1;
                    if (nodes[0] == "www" && nodes.Length == 3) startNode = 1;
                    else if (nodes[0] == "www") startNode = 2;
                    for (int i = startNode; i < nodes.Length - 1; i++)
                        host += string.Format("{0}.", nodes[i]);
                    return host.Substring(0, host.Length - 1);

                }
                else
                    return nodes[0];



            }
            catch (Exception ex)
            {
               
                return strURL;
            }
        }



        public static DrawingImage PutTextInImage(string text)
        {
            Uri oUri = new Uri("pack://application:,,,/HIO;component/Resources/circle.png");
            // Uri oUri = new Uri("/HIO;component/Resources/ico.ico");

            BitmapSource bitmapSource = BitmapFrame.Create(oUri);
            var visual = new DrawingVisual();
            using (DrawingContext drawingContext = visual.RenderOpen())
            {
                drawingContext.DrawImage(bitmapSource, new Rect(0, 0, 24, 24));
                var ft = new FormattedText(text.ToUpper(), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 11, Brushes.White);
                drawingContext.DrawText(ft, new Point((24 - ft.Width) / 2, 4));
            }
            var image = new DrawingImage(visual.Drawing);
            return image;
            //  Image1.Source = image;
        }
        public static bool TPinStatus()
        {

            try
            {


                if (tmain.IsPinRequired)
                    return Application.Current.Dispatcher.Invoke(new Func<bool>(() =>
                         {
                             AdminExtention.CloseAll();
                             return AdminExtention.PinInputExtension.Show() ?? false;

                         }));
                return true;


            }
            catch (Exception ex)
            {
               
                return false;
            }

        }
        internal static void InitializeNotifyIcon(TMain tm)
        {

            tmain = tm;



            System.ComponentModel.Container components = new System.ComponentModel.Container();
            System.Windows.Forms.ContextMenu contextMenu1 = new System.Windows.Forms.ContextMenu();

            contextMenu1.MenuItems.AddRange(
                new System.Windows.Forms.MenuItem[] { menuItem1, menuItem2, menuItem3 });



            // Initialize Disconnect
            menuItem1.Index = 0;
            menuItem1.Text = "&Disconnect";
            menuItem1.Click += new System.EventHandler(Disconnect_Click);
            // Initialize About
            menuItem2.Index = 1;
            menuItem2.Text = "&About us";
            menuItem2.Click += new System.EventHandler(About_Click);
            menuItem3.Index = 2;
            menuItem3.Text = "Dash&board";
            menuItem3.Click += new System.EventHandler(Manage_Click);
            CTicon = new System.Windows.Forms.NotifyIcon(components);
            CTicon.ContextMenu = contextMenu1;
            CTicon.Visible = true;
            CTicon.Text = "HIO";
            CTicon.Visible = true;
            CTicon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            CTicon.MouseClick += CTicon_MouseClick;
        }


        private static void About_Click(object sender, EventArgs e)
        {
            try
            {
                CloseDuplicateWindows();


                string ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                TAbout aboutPage = new TAbout();
                aboutPage.Show(ver);
            }
            catch
            {
                TAbout aboutPage = new TAbout();
                aboutPage.Show("1.0.68");
            }
        }

        private static void CTicon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                TMain.Instance.Show();
        }
        private static void Manage_Click(object sender, EventArgs e)
        {
            TMain.Instance.Show();
        }
        public async static void Disconnect_Click(object sender, EventArgs e)
        {
            if (HIOStaticValues.CheckSyncingData())
                return;
            CloseDuplicateWindows();
            if (BaS.dev is BLEDevice bleDevice)
            {
                await bleDevice.Unpair();
            }
            App.Current.Dispatcher.BeginInvoke((Action)(() => tmain.SettingManager.AddNewDevice()));

            // BaS.UnBondSwitch(HIOStaticValues.blea.mac);
        }
        public static int FirstRun
        {
            get
            {
                try
                {
                    return (int)Registry.GetValue(keyName,
                         "SetupWizard",
                         0);

                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
            set
            {
                Registry.SetValue(keyName, "SetupWizard", value, RegistryValueKind.DWord);
            }
        }
        public static void SyncOpration()
        {

            SYNC_ON = true;
            Commands cmd = new Commands();
            tmain.TabManager.ActiveTab = tmain.AccountManager;
            tmain.AccountManager.SyncronizingState = SyncronizingStateEnum.Syncronizing;
            cmd.Sync();
            SYNC_ON = false;

        }
        public static void CloseDuplicateWindows()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                foreach (Window window in Application.Current.Windows)
                    if (window.DataContext != null && window.DataContext != tmain)
                        window.Close();

            }));

        }
    }
    public class SendData
    {
        public string label;
        public string data;
    };

    public enum CheckPinStatus
    {
        Disabled = 0,
        Enabled_Authenticated = 1,
        Enabled = 2
    }
}
