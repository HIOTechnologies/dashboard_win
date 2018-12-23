using HIO.Backend;
using HIO.Backend.Firefox;
using HIO.Backend.Edge;
using HIO.Controls;
using HIO.Core;
using HIO.Setup;
using HIO.ViewModels.Accounts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WpfAnimatedGif;

namespace HIO.ViewModels.Settings
{
    public class TImport : TSetupPageBase
    {
        public double percent = 0;
        public TImport(TWizard parent, double progressPercent, TSettingManager settingManager) : base(parent, progressPercent)
        {
            SettingManager = settingManager;
            Commands.AddCommand("StartImporting", StartImportingAsync, CanImport);
            Commands.AddCommand("ClosePage", ClosePage);//For Setup Importing
            Commands.AddCommand("ErrorOK", ErrorOK);
            ImportVisible = true;
            LoadItems();
        }





        #region Fields
        private TImportView _Form;
        #endregion

        #region Properties
        public TSettingManager SettingManager { get; private set; }
        public ObservableCollection<TPasswordSource> Items { get; private set; } = new ObservableCollection<TPasswordSource>();

        public bool IsImporting
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

        public bool IsComplete
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
                if (value)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (_Form != null && _Form.TickGif != null)
                        {
                            ImageBehavior.GetAnimationController(_Form.TickGif).Pause();
                            ImageBehavior.GetAnimationController(_Form.TickGif).GotoFrame(0);
                            ImageBehavior.GetAnimationController(_Form.TickGif).Play();
                        }
                    });
                }
            }
        }

        public string MessageErr
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
                ImportVisible = string.IsNullOrEmpty(value);
            }
        }
        public double ImportProgressPercent
        {
            get
            {
                return GetValue<double>();
            }
            set
            {
                SetValue(value);
            }
        }


        public IEnumerable<TPasswordSource> SelectedItems
        {
            get
            {
                return Items.Where(t => t.IsChecked);
            }
        }

        public bool ImportVisible
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
        private void ClosePage(object obj)
        {
            MoveNextPage();
        }
        private void LoadItems()
        {
            Items.Clear();
            var hio = new TPasswordSource("Load Backup", "LogoCollapsedNormal.png");
            hio.PropertyChanged += (s, e) => Commands.Update();
            var chrome = new TPasswordSource("Google Chrome", "Chrome.png");
            chrome.PropertyChanged += (s, e) => Commands.Update(); 
            var firefox = new TPasswordSource("Mozilla Firefox", "Firefox.png");
            firefox.PropertyChanged += (s, e) => Commands.Update();
            var edge = new TPasswordSource("Edge", "edge.png") { IsDisabled = !HIOStaticValues.IsBLESupported }; 
            edge.PropertyChanged += (s, e) => Commands.Update();
            Items.Add(hio);
            Items.Add(chrome);
            Items.Add(firefox);
            Items.Add(edge);

        }
        public override void OnShow()
        {
            base.OnShow();

            MessageErr = null;
        }
        void ErrorOK()
        {
            IsImporting = false;
            ImportVisible = true;
            MessageErr = null;
        }
        private bool CanImport()
        {
            return SelectedItems.Any();
        }
        private async void StartImportingAsync()
        {

            if (HIOStaticValues.CheckSyncingData())

                return;

            List<LoginFieldS> listUser = new List<LoginFieldS>();

            try
            {
                if (HIOStaticValues.CONNECTIONBHIO == true)
                {
                    Commands cmd = new Backend.Commands();
                    int userImported = 0;
                    DataBase dbLocal = new DataBase();
                    LabelImport:
                    if (cmd.AmISync() == SyncronizingStateEnum.Synced)
                    {
                        IsImporting = true;

                        await Task.Run(() =>
                        {
                            Backup bk = new Backup();
                            Chrome chrome = new Chrome();
                            Firefox firefox = new Firefox();
                            Edge edge = new Edge();
                            double progress = ImportProgressPercent;
                            //  var itemList = Items.Where(t => t.IsChecked);

                            foreach (var item in Items.Where(t => t.IsChecked))
                            {
                                if (item.Title == "Load Backup")
                                    listUser.AddRange(bk.LoadBackup());
                                if (item.Title == "Google Chrome")
                                    listUser.AddRange(chrome.getDataFromChrome());
                                if (item.Title == "Mozilla Firefox")
                                    listUser.AddRange(firefox.FetchPassword());
                                if (item.Title == "Edge")
                                    listUser.AddRange(edge.ReadPasswords());

                            }
                            listUser=listUser.Distinct(new loginFieldsCompare()).ToList();
                            int rows = listUser.Count;
                            // ImportVisible = true;

                            try
                            {
                                HIOStaticValues.IMPORT_ON = true;
                                if (rows == 0)
                                {

                                    MessageErr = "No account exist in your browsers";
                                    return;
                                }
                                for (int i = 0; i < rows; i++)
                                {
                                    ImportProgressPercent = (progress == 0) ? (i * 100) / (rows) : progress + ((i * 100) / (rows)) / 2;
                                    int res = HIOStaticValues.BaS.InsertToSwitch(new LoginFieldS { password = listUser[i].password, userName = listUser[i].userName, title = listUser[i].title, url = listUser[i].url });

                                    if (res <= 0)
                                    {

                                        if (res == -2) throw new Exception($"Time out.\n {i}  accounts are imported successfully from {rows} accounts");
                                        if (res == -3) throw new Exception($"No space is available.\n {userImported} accounts are imported successfully");

                                        break;
                                    }
                                    userImported++;
                                    dbLocal.insertToDatabase(res.ToString(), listUser[i].url, (string)listUser[i].userName, "", listUser[i].title);

                                }
                            }
                            catch (SQLiteException ex) when (ex.ErrorCode == 5)
                            {

                                MessageErr = $"HIO needs to close all browsers!{Environment.NewLine}Please save or close your Tabs if your are using any browsers.";
                                return;
                            }
                            catch (Exception ex)
                            {
                                MessageErr = ex.Message;
                                return;
                            }

                            HIOStaticValues.IMPORT_ON = false;
                            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                IsImporting = false;
                                ImportVisible = false;
                                ImportProgressPercent = 0;
                                IsComplete = true;
                                //  if (db_way2 != "")
                                //    System.IO.File.Delete(db_way2);
                            }));
                        });




                        if (Items.Any(t => t.IsChecked))
                            MoveNextPage();
                    }
                    else
                    {
                        HIOStaticValues.popUp("You are not sync!\nPlease wait,We are syncing data...");
                        IsImporting = true;
                        DispatcherTimer dt = new DispatcherTimer();
                        dt.Interval = TimeSpan.FromSeconds(1);
                        dt.Tick += Dt_Tick;
                        dt.Start();
                        int res = await Task.Run(() =>
                        {
                            DataBase db = new DataBase();
                            Commands resetCmd = new Backend.Commands();
                            resetCmd.ResetSync();
                            return resetCmd.Sync();
                        });
                        dt.Stop();

                        if (res == 1) goto LabelImport;

                    }

                }
                else
                {
                    TMessageBox.Show("HIO is not connect!");
                }
            }
            finally
            {
                IsImporting = false;

                HIOStaticValues.IMPORT_ON = false;
            }
        }

        private void Dt_Tick(object sender, EventArgs e)
        {
            ImportProgressPercent = HIOStaticValues.ProgressPercent / 2;
        }

        public void Show()
        {
            _Form = new TImportView();
            _Form.DataContext = this;
            _Form.Closing += _Form_Closing;
            _Form.ShowDialog();
        }

        private void _Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsImporting == true)
            {
                e.Cancel = true;
                HIOStaticValues.popUp("You can not close window while importing data");
            }
        }
        #endregion

    }
    public class TPasswordSource : TViewModelBase
    {
        public TPasswordSource()
        {

        }
        public TPasswordSource(string title, string url) : this()
        {
            Title = title;
            ImageData = TEmbeddedResource.OpenFileByteArray(url);
        }
        public bool IsDisabled
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
        public bool IsChecked
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

        public byte[] ImageData
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

    }
}
