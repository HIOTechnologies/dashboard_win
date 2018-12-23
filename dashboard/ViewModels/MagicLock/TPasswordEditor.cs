using HIO.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIO.ViewModels.MagicLock
{
    public class TPasswordEditor : TViewModelBase
    {
        public TPasswordEditor()
        {
            Commands.AddCommand("Save", Save, () => CanSave);
        }


        #region Fields
        private TPasswordEditorView _Form;
        #endregion

        #region Properties
        public bool CanSave
        {
            get
            {
                return !Password.IsNullOrEmpty();
            }
        }
        public TMagicLockManager Parent { get; private set; }

        public string Password
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                if (SetValue(value))
                {
                    Commands.Update();
                }
            }
        }
        #endregion

        #region Methods

        private void Save()
        {
            //TODO:Validate User And Password
            Parent.ChangePassword(Password);
            _Form.DialogResult = true;
        }
        public bool Show(TMagicLockManager parent)
        {
            Parent = parent;
            Password = Parent.Password;
            _Form = new TPasswordEditorView();
            _Form.DataContext = this;
            return _Form.ShowDialog() ?? false;
        }
        #endregion

    }
}
