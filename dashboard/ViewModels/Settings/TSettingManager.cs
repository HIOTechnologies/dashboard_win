using HIO.Backend;
using HIO.Controls;
using HIO.ViewModels.Notify;
using HIO.ViewModels.Settings.NewDeviceAdding;
using HIO.WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HIO.ViewModels.Settings
{
    public class TSettingManager : TTabBase
    {

        public TSettingManager(TTabManager parent) : base(parent)
        {
            Items = new ObservableCollection<TDevice>();
            Commands.AddCommand("AddNewDevice", AddNewDevice);
            Commands.AddCommand("Import", Import);
            Commands.AddCommand("Backup", Backup);
            Commands.AddCommand("Reset", async () => await ResetAsync());
            Commands.AddCommand("HelpAndSupport", HelpAndSupport);
            Commands.AddCommand("AboutUs", AboutUs);
            Commands.AddCommand("loading", async () => await LoadPage());
            Commands.AddCommand("UpdateDeviceName", UpdateDeviceName, (p) => (HIOStaticValues.tmain?.IsConnected ?? false));
         
        }


        #region Properties
        public ObservableCollection<TDevice> Items { get; private set; }
        public override string Title
        {
            get
            {
                return "Settings";
            }
        }
        public  string ProcessExport {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }

        }
       
        public bool Exapnder
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

        public override string HoverImageUrl
        {
            get
            {
                return "pack://application:,,,/HIO;component/Resources/Buttons/setting.png";
            }
        }

        public override string NormalImageUrl
        {
            get
            {
                return "pack://application:,,,/HIO;component/Resources/Buttons/setting2.png";
            }
        }


        public string MyHIOTitle
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

        public string MyHIOVersion
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

        public string MyHIOMac
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

        #endregion

        #region Methods

        private void Backup()
        {
            if (HIOStaticValues.CheckSyncingData())
                return;
           
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "HIO_Records_"+DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss"); 
            dlg.DefaultExt = ".text"; 
            dlg.Filter = "Text documents (.txt)|*.txt"; 

            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                DataBase db = new DataBase();
             
                   var users = db.ReadData();
                   Task.Run(() =>
                   {

                       ExportUsersAsync(users, filename);
                     
                   });
     
            }
     


        }

        private async Task ResetAsync()
        {

            if (HIOStaticValues.CheckSyncingData())
                return;
            HIOStaticValues.tmain.AccountManager.SourceItems.Clear();
            await UIService.Execute(async () =>
            {
                if (HIOStaticValues.CheckSyncingData())
                    return;
                if (HIOStaticValues.CONNECTIONBHIO == true)
                {

                    Commands ic = new Commands();
                    if (!ic.EraseAll())
                    {
                        HIOStaticValues.popUp("Something went wrong!");
                    }
                }
                else
                {

                    HIOStaticValues.popUp("HIO is not connect!");
                }
            });
            //Application.Current.Dispatcher.Invoke(new Action(() =>
            //{
            //    AddNewDevice();
            //}));

        }

        private void HelpAndSupport()
        {

        }

        private void AboutUs()
        {
            TAbout aboutPage = new TAbout();
            HIOStaticValues.CloseDuplicateWindows();
            string ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            aboutPage.Show(ver);
        }
        public void Import()
        {
            if (!HIOStaticValues.CONNECTIONBHIO)
            {
                TMessageBox.Show("HIO is not connect!");
                return;
            }
            if (HIOStaticValues.CheckSyncingData())
                return;
            TImport importPage = new TImport(null, 0, this);
            importPage.Show();
        }


        public async Task LoadPage()
        {

            try
            {
                if (HIOStaticValues.CheckSyncingData())
                    return;
                await UIService.Execute(async () =>
                {
                    if (HIOStaticValues.CONNECTIONBHIO == false) Exapnder = true;
                    else Exapnder = false;
                    if (HIOStaticValues.CheckSyncingData())
                        return;
                    Commands ic = new Commands();
                    Converts conv = new Converts();
                    if (HIOStaticValues.CONNECTIONBHIO == true)
                    {
                        HIOStaticValues.commandQ.Add(() =>
                        {
                            //Get VersioN
                            ic.GetVersion();
                            //  await Task.Run(() => { ic.GetVersion(); });
                            MyHIOTitle = Encoding.UTF8.GetString(HIOStaticValues.blea.name).Replace("\0", "");
                            MyHIOVersion = "Version: " + HIOStaticValues.blea.ver;
                            MyHIOMac = "MAC: " + conv.ByteToChar(HIOStaticValues.blea.mac[0]) + ":" + conv.ByteToChar(HIOStaticValues.blea.mac[1]) + ":" + conv.ByteToChar(HIOStaticValues.blea.mac[2]) + ":" + conv.ByteToChar(HIOStaticValues.blea.mac[3]) + ":" + conv.ByteToChar(HIOStaticValues.blea.mac[4]) + ":" + conv.ByteToChar(HIOStaticValues.blea.mac[5]);
                        });
                        //IsConnecting = true;
                    }
     
                });
            }
            catch (Exception ex)
            {
               
            }
            finally { }

        }


        public async Task ExportUsersAsync(List<LoginFieldS> lf, string fileName)
        {
            string jsonData = "";
            int counter = 0;
            await UIService.Execute(async () =>
            {
                foreach (LoginFieldS user in lf)
                {

                    try
                    {
                        counter++;
                        if (user.rowid == "")
                            continue;
                        if (HIOStaticValues.TPinStatus())
                        {
                            Commands ic = new Commands();
                            int rowidInt = Int32.Parse(user.rowid);
                            byte[] rowidByteArray = BitConverter.GetBytes(rowidInt).ToArray();
                            while (true)
                            {
                                StatusPassword sp = ic.GetPassword(rowidByteArray);
                                if (sp.statusWord != null && sp.statusWord.SequenceEqual(new byte[] { 0x69, 0x85 }))
                                {

                                    var res = System.Windows.Application.Current.Dispatcher.Invoke(new Func<bool>(() =>
                                    {
                                        return HIOStaticValues.AdminExtention.Extention06a.Show();
                                    }));
                                    if (!res) return;
                                }
                                else if (sp.statusWord.SequenceEqual(new byte[] { 0x90, 0x00 }))
                                {
                                    System.Windows.Application.Current.Dispatcher.Invoke(new Func<bool>(() =>
                                    {
                                        ProcessExport = counter * 100 / lf.Count + "%";
                                        return true;
                                    }));
                                    jsonData += $@"{{""username"": ""{user.userName}"",""password"": ""{sp.pass}"",""url"": ""{user.url}"",""title"": ""{user.title}"",""appid"": ""{user.appID}"",""counter"": ""{user.popularity}"",""last_used"":""{user.last_used}""}}
";


                                    break;
                                }
                            }
                        }

            
                    }
                    catch (Exception ex)
                    {

                        continue;
                    }

                }
               
                ProcessExport = "";
                using (var file = new StreamWriter(fileName, true))
                {
                    file.WriteLine(jsonData);
                    file.Close();
                }
            });
        }
        public void AddNewDevice()
        {

            if (HIOStaticValues.CheckSyncingData())
                return;
            if (HIOStaticValues.PinInputDashboardVM != null)
                HIOStaticValues.PinInputDashboardVM.PinLockEnd = null;
            if (HIOStaticValues.PinInputExtensionVM != null)
                HIOStaticValues.PinInputExtensionVM.PinLockEnd = null;
            Commands cmd = new Commands();

            if (HIOStaticValues.DirectBluetooth == false && HIOStaticValues.CONNECTIONBHIO == true)
            {

                cmd.ResetSync();
                cmd.UnBond(HIOStaticValues.blea.mac);

            }


            HIOStaticValues.DirectBluetooth = false;



            TAddNewDevice deviceAdd = new TAddNewDevice(Parent.Parent.AccountManager);

            deviceAdd.Show(this);
        }

        public void UpdateDeviceName(object name)
        {
            if (name == null || name.ToString().IsNullOrWhiteSpace())
                return;
            HIOStaticValues.BaS.SwitchSetDeviceName(name.ToString());
        }
        #endregion
    }
}
