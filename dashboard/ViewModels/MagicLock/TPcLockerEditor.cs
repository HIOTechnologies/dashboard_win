using HIO.Setup;
using System;
using System.Collections.ObjectModel;

namespace HIO.ViewModels.MagicLock
{
    public class TPcLockerEditor : TSetupPageBase
    {
        public TPcLockerEditor(TWizard parent, double progressPercent, TMagicLockManager magicLockManager) : base(parent, progressPercent)
        {
            Commands.AddCommand("Apply", Apply, () => CanSave);
            MagicLockManager = magicLockManager;
            SelectedUser = MagicLockManager.SelectedUser;
            Password = MagicLockManager.Password;
        }

        #region Fields
        private TPcLockerEditorView _Form;
        #endregion

        #region Properties
        public bool CanSave
        {
            get
            {
                return SelectedUser != null && !Password.IsNullOrEmpty();
            }
        }
        public TMagicLockManager MagicLockManager { get; private set; }

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

        public ObservableCollection<TUser> Users
        {
            get
            {
                return MagicLockManager?.Users;
            }
        }

        public TUser SelectedUser
        {
            get
            {
                return GetValue<TUser>();
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
        private void Skip()
        {
            MoveNextPage();
        }
        private void Apply()
        {
            //TODO:Validate User And Password
            MagicLockManager.ChangePassword(SelectedUser, Password);
            if (_Form != null)
            {
                _Form.DialogResult = true;
            }
            else
            {
                MoveNextPage();
            }
        }
        public bool Show()
        {
            _Form = new TPcLockerEditorView();
            _Form.DataContext = this;
            return _Form.ShowDialog() ?? false;
        }
        #endregion

    }
}
