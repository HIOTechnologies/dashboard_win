using HIO.Core;
using HIO.ViewModels.Accounts;
using HIO.Backend;

using System.Collections.ObjectModel;
using System;
using System.Windows;
using HIO.Controls;
using HIO.WPF.Services;

namespace HIO.Extentions
{
    public class TExtention02 : TViewModelBase, IExtention
    {
        public bool IsClosed { get; private set; }
        public TExtention02()
        {
            try
            {

                //HIOStaticValues.AdminExtention.ShowOnly(this);
                Commands.AddCommand("Apply", Apply);
            }
            catch (Exception e)
            {


            }
        }


        #region Fields
        private TExtention02View _Form;
        #endregion

        #region Properties

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

        public ObservableCollection<TAccountItem> Items { get; private set; } = new ObservableCollection<TAccountItem>();

        #endregion

        #region Methods
        void insertData() {
            Commands ic = new Commands();
            UIService.Execute(async () =>
            {
                if (HIOStaticValues.TPinStatus())
                {

                    int res = ic.Insert(EditingObject.Url, EditingObject.Username, EditingObject.Name, EditingObject.Password);
                    try
                    {
                        HIOStaticValues.tmain?.AccountManager?.LoadData();
                        HIOStaticValues.tmain?.AccountManager?.OnPropertyChanged(nameof(TAccountManagerViewModel.IsAllChecked));
                    }
                    catch { /*TODO: remove try catch*/}
                    if (res == 1)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (!IsClosed)
                                Close();
                        }));
                    }
                }
            }).Wait();

        }
        private void Apply()
        {
            //TODO:Validate data
            //TODO:Save Changes
            if (EditingObject.Name != "" && EditingObject.Password != "")
            {
              
                  HIOStaticValues.commandQ.Add(()=>insertData());
 
            }
 
        }

        public void Show(TAccountItem editingItem)
        {
            EditingObject = editingItem;
            IsClosed = false;
            _Form = new TExtention02View();
            _Form.DataContext = this;
          //  _Form.Deactivated += _Form_Deactivated;
            _Form.Closing += _Form_Closing;
            _Form.Show();
        }

        //private void _Form_Deactivated(object sender, System.EventArgs e)
        //{
        //    if (!IsClosed)
        //        Close();
        //}

        private void _Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClosed = true;

        }
        public bool IsFormOpen
        {
            get
            {
                return _Form != null;
            }
        }

        public Window Form
        {
            get
            {
                return _Form;
            }

        }

        public void Close()
        {
         
            _Form?.Close();
                _Form = null;
            
        }


        #endregion

    }
}
