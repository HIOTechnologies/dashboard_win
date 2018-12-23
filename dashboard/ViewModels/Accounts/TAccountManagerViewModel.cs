using HIO.Backend;
using HIO.Controls;
using HIO.Core;
using HIO.WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace HIO.ViewModels.Accounts
{
    public class TAccountManagerViewModel : TTabBase
    {
        public TAccountManagerViewModel(TTabManager parent) : base(parent)
        {

            AddNewAccountManager = new TAddNewAccount(this);
            Commands.AddCommand("Delete", Delete, () => SelectedItems.Any());
            Commands.AddCommand("Syncronize", async () => await Syncronize()/*, () => SyncronizingState == SyncronizingStateEnum.NotSynced*/);
            Commands.AddCommand("AccountEdit", AccountEdit);
            Commands.AddCommand("AccountDelete", AccountDelete);
            Commands.AddCommand("AccountCopyUrl", AccountCopyUrl);
            Commands.AddCommand("AccountCopyUsername", AccountCopyUsername);
            Commands.AddCommand("AccountCopyPassword", AccountCopyPassword);
            Commands.AddCommand("SortBy", SortBy);
            Commands.AddCommand("loading", async () => await LoadingAsync());
            Commands.AddCommand("Lunch", Launch);
            ProgressPercent = 0;
            // DispatcherTimer dt = new DispatcherTimer();
            // dt.Interval = TimeSpan.FromSeconds(30);
            // dt.Tick += Dt_CheckConnection;
            // dt.Start();
        }

        private void Dt_CheckConnection(object sender, EventArgs e)
        {
            HIOStaticValues.commandQ.Add(() =>
            {
                Commands rssiCheck = new Backend.Commands();
                rssiCheck.IsConnection();
                App.Current.Dispatcher.Invoke(new Action(() =>

                {
                    SignalValue = rssiCheck.GetSignalStatus(HIOStaticValues.blea.rssi);
                }));
            });
        }



        #region Fields
        List<LoginFieldS> lp;

        #endregion

        #region Properties

        public string SortByField
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                if (SetValue(value))
                {
                    LoadDatabySort();
                }
            }
        }

        public TAddNewAccount AddNewAccountManager { get; private set; }
        public ObservableCollection<TAccountItem> SourceItems { get; private set; } = new ObservableCollection<TAccountItem>();
        public IEnumerable<TAccountItem> Items
        {
            get
            {
                if (SearchText.IsNullOrEmpty())
                {
                    return SourceItems;
                }
                return SourceItems.Where(t => (t.Name?.ContainsIgnoreCase(SearchText) ?? false) || (t.Username?.ContainsIgnoreCase(SearchText) ?? false)).ToList();
            }
        }

        public IEnumerable<TAccountItem> SelectedItems
        {
            get
            {
                return SourceItems.Where(t => t.IsChecked);
            }
        }

        public string SyncronizingImageUrl
        {
            get
            {
                if (SyncronizingStateEnum.NotSynced == SyncronizingState)
                {
                    return "/HIO;component/Resources/NoSynchronize16.png";

                }
                else
                {
                    return "/HIO;component/Resources/Synchronize16.png";
                }
            }

        }

        /// <summary>
        /// Value Range : 0-4
        /// </summary>
        public BatteryStateEnum BatteryValue
        {
            get
            {
                return GetValue<BatteryStateEnum>();
            }
            set
            {
                SetValue(value);
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

        public string SearchText
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                if (SetValue(value))
                {
                    NotifyItemsChanged();
                }
            }
        }

        private void NotifyItemsChanged()
        {
            OnPropertyChanged(() => Items);
            Commands.Update();
        }

        public bool IsAllChecked
        {
            get
            {
                var items = SourceItems?.Where(t => SearchText.IsNullOrEmpty() || (t.Name?.ContainsIgnoreCase(SearchText) ?? false) || (t.Username?.ContainsIgnoreCase(SearchText) ?? false));
                return items != null && items.Any() && items.All(t => t.IsChecked);
            }
            set
            {
                if (IsAllChecked != value)
                {
                    foreach (var item in SourceItems
                        .Where(t => SearchText.IsNullOrEmpty() || (t.Name?.ContainsIgnoreCase(SearchText) ?? false) || (t.Username?.ContainsIgnoreCase(SearchText) ?? false))
                        .Where(t => t.IsChecked != value).ToArray())
                    {
                        item.IsChecked = value;
                    }
                    //MessageBox.Show("IsAllChecked : " + value);
                }
            }
        }

        public override string Title
        {
            get
            {
                return "Accounts";
            }
        }
        public string ItemCounter
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
                Commands.Update();
                OnPropertyChanged();
            }
        }

        public override string HoverImageUrl
        {
            get
            {
                return "pack://application:,,,/HIO;component/Resources/Buttons/profile.png";
            }
        }

        public override string NormalImageUrl
        {
            get
            {
                return "pack://application:,,,/HIO;component/Resources/Buttons/profile2.png";
            }
        }


        public SyncronizingStateEnum SyncronizingState
        {
            get
            {
                return GetValue<SyncronizingStateEnum>();
            }
            set
            {
                if (SetValue(value))
                {
                    Commands.Update();
                    OnPropertyChanged(() => IsSyncronizing);
                    OnPropertyChanged(() => SyncronizingImageUrl);
                    OnPropertyChanged(() => CanShowData);

                    if (value == SyncronizingStateEnum.Completed)
                    {
                        StartTimer();
                    }
                }
            }
        }


        public bool IsSyncronizing
        {
            get
            {
                return SyncronizingState == SyncronizingStateEnum.Syncronizing;
            }
        }
        public bool CanShowData
        {
            get
            {
                return SyncronizingState == SyncronizingStateEnum.Synced;
            }
        }


        public double ProgressPercent
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

        #endregion

        #region Methods
        protected override void OnIsActiveTabChanged(bool value)
        {
            base.OnIsActiveTabChanged(value);
            if (!IsSelected && AddNewAccountManager.IsVisible)
            {
                AddNewAccountManager.IsVisible = false;
            }
        }
        private async Task Syncronize()
        {
            if (HIOStaticValues.CheckSyncingData())
                return;
            if (SyncronizingState != SyncronizingStateEnum.Synced)
            {
                SyncronizingState = SyncronizingStateEnum.Syncronizing;
                try
                {
                    ProgressPercent = 0;
                    DispatcherTimer dt = new DispatcherTimer();
                    dt.Interval = TimeSpan.FromSeconds(1);
                    dt.Tick += Dt_Tick;
                    dt.Start();
                    await Task.Run(() =>

                    {

                        Commands ic = new Commands();
                        SyncronizingState = SyncronizingStateEnum.Syncronizing;
                        ic.Sync();
                        //   dt.Stop();
                    });
                    //   await LoadingAsync();
                }
                finally
                {
                    // SyncronizingState = SyncronizingStateEnum.Completed;

                }
            }
            else
            {

                await LoadData();
            }
            OnPropertyChanged(nameof(IsAllChecked));
        }

        private void Dt_Tick(object sender, EventArgs e)
        {
            ProgressPercent = HIOStaticValues.ProgressPercent;
            if (ProgressPercent > 99)
            {
                (sender as DispatcherTimer).Stop();
                SyncronizingState = SyncronizingStateEnum.Completed;
                LoadData();
            }
            else if (ProgressPercent == 0)
            {
                (sender as DispatcherTimer).Stop();
                SyncronizingState = SyncronizingStateEnum.Completed;
                LoadData();
            }
        }

        private void StartTimer()
        {
            DispatcherTimer DT = new DispatcherTimer();
            DT.Interval = TimeSpan.FromSeconds(3);
            DT.Tick += (a, b) =>
            {
                DT.Stop();
                SyncronizingState = SyncronizingStateEnum.Synced;
                ProgressPercent = 0;
            };
            DT.Start();
        }
        private void Launch(object obj)
        {
            TAccountItem accountItem = (TAccountItem)obj;

            System.Diagnostics.Process.Start("http://" + accountItem.Url);
        }
        public void LoadDatabySort()
        {
            if (HIOStaticValues.CheckSyncingData())
                return;
            #region Sort Data
            List<TAccountItem> items = SourceItems.ToList();
            SourceItems.Clear();
            if (SortByField.IsNullOrEmpty() || "Name".Equals(SortByField, StringComparison.InvariantCultureIgnoreCase))
            {
                items = items.OrderBy(t => t.Name).ToList();
            }
            else if ("Date".Equals(SortByField, StringComparison.InvariantCultureIgnoreCase))
            {
                items = items.OrderByDescending(t => t.Date).ToList();
            }
            else if ("Popularity".Equals(SortByField, StringComparison.InvariantCultureIgnoreCase))
            {
                items = items.OrderByDescending(t => t.Popularity).ToList();
            }
            #endregion
            #region Add Data

            foreach (var item in items)
            {
                SourceItems.Add(item);
            }
            #endregion
            OnPropertyChanged(nameof(Items));
        }
        public async Task LoadData()
        {
            try
            {
                App.Current.Dispatcher.Invoke(new Action(async () =>
                {
                    SourceItems.Clear();
                    DataBase db = new DataBase();
                    Converts conv = new Converts();
                    lp = await Task.Run(() =>
                    {
                        return db.ReadData();
                    });
                    if (lp != null)
                        ItemCounter = (lp.Count > 1) ? lp.Count + " Items" : lp.Count + " Item";
                    //Note : lp may be null
                    if (lp != null && lp.Any())
                    {
                        #region Load Data

                        List<TAccountItem> items = new List<TAccountItem>();
                        foreach (var item in lp)
                        {
                            DrawingImage tmpDraw = new DrawingImage();
                            if (item.imageData == null || item.imageData.Length == 0)
                                tmpDraw = HIOStaticValues.PutTextInImage(item.url.Substring(0, 1));
                            else
                                tmpDraw = conv.BitmapImageToDrawingImage(conv.byteArrayToImage(item.imageData));
                            items.Add(new TAccountItem(this)
                            {
                                UserID = item.rowid,
                                Name = item.title,
                                Username = item.userName,
                                Url = item.url,
                                ImageData = tmpDraw,
                                AppID = item.appID,
                                SubTitle1 = item.userName,
                                SubTitle2 = conv.GetPrettyDate(item.last_used),
                                Popularity = item.popularity,
                                Date = item.last_used

                            });
                            //  items.Add(new User() { Title = listlp[i].title, Username = listlp[i].userName, rowid = listlp[i].rowid });
                        }
                        #endregion
                        #region Sort Data

                        if (SortByField.IsNullOrEmpty() || "Name".Equals(SortByField, StringComparison.InvariantCultureIgnoreCase))
                        {
                            items = items.OrderBy(t => t.Name).ToList();
                        }
                        else if ("Date".Equals(SortByField, StringComparison.InvariantCultureIgnoreCase))
                        {
                            items = items.OrderByDescending(t => t.Date).ToList();
                        }
                        else if ("Popularity".Equals(SortByField, StringComparison.InvariantCultureIgnoreCase))
                        {
                            items = items.OrderByDescending(t => t.Popularity).ToList();
                        }
                        #endregion
                        #region Add Data

                        foreach (var item in items)
                        {
                            SourceItems.Add(item);
                        }
                        #endregion
                    }
                    OnPropertyChanged(nameof(Items));
                }));
            }
            catch {

            }
            finally
            {
                //   NotifyItemsChanged();
                //    SyncronizingState = SyncronizingStateEnum.Completed;
            }
        }
        private void SortBy(object obj)
        {
            SortByField = (string)obj;
            Commands.Update();
        }

        public async Task LoadingAsync()
        {

            try
            {
                if (HIOStaticValues.CheckSyncingData())
                {
                    SyncronizingState = SyncronizingStateEnum.Syncronizing;
                    Trace.WriteLine("SyncronizingState in dashboard:" + SyncronizingState);
                    Trace.WriteLine("percent in dashboard:" + ProgressPercent);
                    DispatcherTimer dt = new DispatcherTimer();
                    dt.Interval = TimeSpan.FromSeconds(1);
                    dt.Tick += Dt_Tick;
                    dt.Start();
                    return;
                }
                App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        SourceItems.Clear();
                    }));
                Converts conv = new Converts();
                DataBase db = new DataBase();
                //  HIOStaticValues.commandQ.Add(()=> QBatteryandSignalCheckAsync());
                Commands ic = new Commands();
                HIOStaticValues.commandQ.Add(() =>
                {

                    BatteryValue = ic.GetBatteryStatus();



                });
              
                HIOStaticValues.commandQ.Add(() =>
                {
                    Commands cmd = new Backend.Commands();
                    cmd.GetRssi();
                    App.Current.Dispatcher.Invoke(new Action(() =>

                    {
                        SignalValue = cmd.GetSignalStatus(HIOStaticValues.blea.rssi);
                    }));
                });
                HIOStaticValues.commandQ.Add(() =>
                {
                    App.Current.Dispatcher.Invoke(new Action(async () =>

                    {
                        SyncronizingState = ic.AmISync();
                        if (SyncronizingState == SyncronizingStateEnum.NotSynced)
                           await Syncronize();
                    }));
                });
               
                App.Current.Dispatcher.Invoke(new Action(async () =>
                {
                    //READ DATA
                    lp = await Task.Run(() =>
                    {
                        return db.ReadData();
                    });
                    if (lp != null)
                        ItemCounter = (lp.Count > 1) ? lp.Count + " Items" : lp.Count + " Item";
                    //Note : lp may be null
                    if (lp != null && lp.Any())
                    {
                        #region Load Data

                        List<TAccountItem> items = new List<TAccountItem>();
                        foreach (var item in lp)
                        {
                            DrawingImage tmpDraw = new DrawingImage();
                            if (item.imageData == null || item.imageData.Length == 0)
                                tmpDraw = HIOStaticValues.PutTextInImage(item.url.Substring(0, 1));
                            else
                                tmpDraw = conv.BitmapImageToDrawingImage(conv.byteArrayToImage(item.imageData));
                            items.Add(new TAccountItem(this)
                            {
                                UserID = item.rowid,
                                Name = item.title,
                                Username = item.userName,
                                Url = item.url,
                                ImageData = tmpDraw,
                                AppID = item.appID,
                                SubTitle1 = item.userName,
                                SubTitle2 = conv.GetPrettyDate(item.last_used),
                                Popularity = item.popularity,
                                Date = item.last_used
                            });
                            //  items.Add(new User() { Title = listlp[i].title, Username = listlp[i].userName, rowid = listlp[i].rowid });
                        }
                        #endregion

                        #region Sort Data

                        if (SortByField.IsNullOrEmpty() || "Name".Equals(SortByField, StringComparison.InvariantCultureIgnoreCase))
                        {
                            items = items.OrderBy(t => t.Name).ToList();
                        }
                        else if ("Date".Equals(SortByField, StringComparison.InvariantCultureIgnoreCase))
                        {
                            items = items.OrderByDescending(t => t.Date).ToList();
                        }
                        else if ("Popularity".Equals(SortByField, StringComparison.InvariantCultureIgnoreCase))
                        {
                            items = items.OrderByDescending(t => t.Popularity).ToList();
                        }
                        #endregion
                        #region Add Data
                        foreach (var item in items)
                        {
                            SourceItems.Add(item);
                        }
                        #endregion

                    }
                    OnPropertyChanged(nameof(Items));
                }));
            }
            catch (Exception ex)
            {

            }
            finally
            {
                NotifyItemsChanged();
            }
        }


        private async void Delete()
        {
            if (HIOStaticValues.CheckSyncingData())
                return;

            if (TMessageBox.Show("Are you sure you want to delete these accounts?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Commands cmd = new Backend.Commands();
                while (true)
                {
                    var res = cmd.AmISync();
                    if (res == SyncronizingStateEnum.Synced)
                    {

                        break;
                    }
                    else

                        HIOStaticValues.SyncOpration();
                }


                await UIService.Execute(async () =>
                {
                    DataBase db = new DataBase();
                    foreach (var item in SourceItems.Where(t => t.IsChecked).ToArray())
                    {
                        Commands ic = new Commands();
                        if (ic.Delete(item.UserID))
                        {
                            App.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                SourceItems.Remove(SourceItems.FirstOrDefault(i => i.UserID == item.UserID));
                                OnPropertyChanged(nameof(Items));
              
                            }));
                        }
                      
                    }
                    await LoadData();
                });
                OnPropertyChanged(nameof(IsAllChecked));
            }
        }

        public void OnIsCheckedChanged(TAccountItem accountItem)
        {
            this.OnPropertyChanged(t => t.IsAllChecked);
            this.OnPropertyChanged(t => t.SelectedItems);
            foreach (var item in Items.Where(t => t.IsChecked))
            {
                item.OnPropertyChanged(t => t.CornerRadius);
            }

            Commands.Update();
        }

        private void AccountCopyPassword(object obj)
        {
            if (HIOStaticValues.CheckSyncingData())
                return;
            (obj as TAccountItem).CopyPassword();
        }

        private void AccountCopyUsername(object obj)
        {

            (obj as TAccountItem).CopyUsername();

        }

        private void AccountCopyUrl(object obj)
        {
            (obj as TAccountItem).CopyUrl();

        }

        private void AccountDelete(object obj)
        {
            if (HIOStaticValues.CheckSyncingData())
                return;
            (obj as TAccountItem).Delete();

        }

        private void AccountEdit(object obj)
        {
            if (HIOStaticValues.CheckSyncingData())
                return;
            (obj as TAccountItem).StartEdit();
        }
        #endregion

    }

    public enum SyncronizingStateEnum
    {
        NotSynced = 0,
        Synced = 1,
        Syncronizing = 2,
        Completed = 3,
    }


}
