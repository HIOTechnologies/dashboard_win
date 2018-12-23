using HIO.Backend;
using HIO.Core;
using HIO.ViewModels.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HIO.Extentions
{
    public class TExtention07 : TViewModelBase,IExtention
    {

        public TExtention07()
        {
           // HIOStaticValues.AdminExtention.ShowOnly(this);
            Commands.AddCommand("Apply", Apply);
        }


        #region Fields
        private TExtention07View _Form;
        #endregion

        #region Properties
        public List<TAccountItem> Items { get; private set; }
        public bool IsClosed { get; private set; }

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

        #endregion

        #region Methods

        private void Apply()
        {
            //TODO:Validate data
            //TODO:Save Changes
            _Form.DialogResult = true;
        }

        public void Show(TAccountItem editingItem)
        {
            EditingObject = editingItem;
            IsClosed = false;
            _Form = new TExtention07View();
            _Form.DataContext = this;
            _Form.Closing += _Form_Closing;
            _Form.Deactivated += _Form_Deactivated;
            _Form.Show() ;

        }

        private void _Form_Deactivated(object sender, EventArgs e)
        {
            if (!IsClosed)
                Close();
        }

        private void _Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClosed = true;
          
        }

        public void Close()
        {
            _Form?.Close();
            _Form = null;
        }

        #endregion

    }
}
