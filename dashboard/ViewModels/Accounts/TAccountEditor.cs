using HIO.Backend;
using HIO.Core;
using HIO.Extentions;
using HIO.WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace HIO.ViewModels.Accounts
{
    public class TAccountEditor : TViewModelBase
    {
        public TAccountEditor()
        {

            Commands.AddCommand("Apply", Apply);


            DataBase db = new DataBase();
            List<LoginFieldS> lp = db.ReadData();
            ObservableCollection<TAccountItem> items = new ObservableCollection<TAccountItem>();
            if (lp != null && lp.Any())
            {
                foreach (var item in lp)
                {
                    items.Add(new TAccountItem()
                    {
                        UserID = item.rowid,
                        Name = item.title,
                        Username = item.userName,
                        Url = item.url,
                        AppID = item.appID,
                        SubTitle1 = item.userName,

                    });
                    //  items.Add(new User() { Title = listlp[i].title, Username = listlp[i].userName, rowid = listlp[i].rowid });
                }
            }
            Items = items;
        }



        #region Fields
        private TAccountEditorView _Form;
        private byte flagPass = 0x00;
        #endregion

        #region Properties
        public ObservableCollection<TAccountItem> Items { get; private set; } = new ObservableCollection<TAccountItem>();

        public bool ShowPassword
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                if (SetValue(value))
                {
                    if (value == true) LoadPasswordAsync();
                }
            }
        }

        private void LoadPasswordAsync()
        {
            Task.Run(() =>
            {
                try
                {
                    if (HIOStaticValues.TPinStatus())
                    {

                        Commands ic = new Commands();


                        Converts conv = new Converts();
                        int userIdInt = Int32.Parse(EditingObject.UserID);
                        byte[] rowidByteArray = BitConverter.GetBytes(userIdInt);
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
                                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                                {

                                    EditingObject.Password = sp.pass;
                                }));
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {


                }


            });










            


        }

        public TAccountItem EditingObject
        {
            get
            {
                return GetValue<TAccountItem>();
            }
            set
            {
                SetValue(value);
            }
        }
        public Visibility EmptyTitle
        {
            get
            {
                return GetValue<Visibility>();
            }
            set
            {
                SetValue(value);
            }
        }
        public bool IsSaving
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
        async Task QUpdateUserAsync()
        {
            try
            {
                await UIService.Execute(() =>
                {
                    Commands ic = new Commands();
                    HIOStaticValues.CheckingData(EditingObject);
                    bool res = ic.UpdateUser(EditingObject.UserID, EditingObject.Url, EditingObject.AppID, EditingObject.Name, EditingObject.Username, EditingObject.Password, flagPass);
                    if (res)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            IsSaving = true;
                            EditingObject.EndEdit();
                            if (_Form?.Owner == null) /*from extension*/
                            {
                                try
                                {
                                    HIOStaticValues.tmain?.AccountManager?.LoadData();
                                    HIOStaticValues.tmain?.AccountManager?.OnPropertyChanged(nameof(TAccountManagerViewModel.IsAllChecked));
                                }
                                catch { /*TODO: remove try catch*/}
                            }
                            if (_Form.IsVisible)
                                _Form.Close();

                        }));
                    }
                });
            }
            finally
            {
                IsSaving = false;
            }
        }
        private void Apply()
        {
            //TODO:Validate data
            //TODO:Save Changes
            var isInvalid = false;

            if (HIOStaticValues.TPinStatus())
            {

                if (EditingObject.Name.TrimStart() == "")
                {
                    _Form.titleRequiredImage.Visibility = Visibility.Visible;

                    EditingObject.Name = " ";
                    isInvalid = true;
                }
                if (EditingObject.Url != null && EditingObject.Url.TrimStart() == "")
                {
                    _Form.urlRequiredImage.Visibility = Visibility.Visible;

                    EditingObject.Url = " ";
                    isInvalid = true;
                }
                if (EditingObject.Password == null)
                {
                    EditingObject.Password = "";
                }
                if (isInvalid)
                    return;

                HIOStaticValues.commandQ.Add(() => QUpdateUserAsync());
            }
        }

        public void Show(TAccountItem editingItem)
        {
            EditingObject = editingItem;

            _Form = new TAccountEditorView();
            _Form.DataContext = this;
            _Form.MouseDown += _Form_MouseDown;
            _Form.Closed += _Form_Closed;
            EditingObject.PropertyChanged += EditingObject_PropertyChanged;
            _Form.Title = $"Edit Account : {editingItem.Name}";
            _Form.Show();
            _Form.Activate();
        }

        private void _Form_MouseDown(object sender, MouseButtonEventArgs e)
        {

            //try
            //{
            //    if (e.ChangedButton == MouseButton.Left)
            //    _Form.DragMove();
            //}
            //catch (Exception) { }
            //finally { }

        }

        private void _Form_Closed(object sender, EventArgs e)
        {
            EditingObject.CancelEdit();
            IsSaving = false;
            EditingObject.Password = "";
        }

        private void EditingObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Password")
                if ((sender as TAccountItem).Password != "")
                    flagPass = 0x1;
                else flagPass = 0x00;
        }
        #endregion

    }
}
