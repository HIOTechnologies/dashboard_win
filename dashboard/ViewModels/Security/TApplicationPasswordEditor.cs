using HIO.Controls;
using HIO.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIO.ViewModels.Security
{
    public class TApplicationPasswordEditor : TViewModelBase
    {
        public TApplicationPasswordEditor()
        {
            Commands.AddCommand("Apply", Apply);
        }

        #region Fields
        private TApplicationPasswordEditorView _Form;

        #endregion

        #region Properties

        public TSecurityManager Parent { get; private set; }


        public string OldPassword
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

        public string Password
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

        public string ReEnterPassword
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
        #endregion

        #region Methods

        private void Apply()
        {
            if (OldPassword.IsNullOrEmpty())
            {
                TMessageBox.Show("Old password is required !");
                return;
            }

            if (Password.IsNullOrEmpty())
            {
                TMessageBox.Show("Password is required !");
                return;
            }
            if (ReEnterPassword.IsNullOrEmpty())
            {
                TMessageBox.Show("Re-Enter Password is required !");
                return;
            }
            if (Password != ReEnterPassword)
            {
                TMessageBox.Show("Password re-entered incorrectly!");
                return;
            }

            Parent.SaveApplicationPassword(Password);
            //TODO: Save Application Password
            //TMessageBox.Show("Password saved successfully.");


            _Form.Close();
        }

        public bool Show(TSecurityManager securityManager)
        {
            Parent = securityManager;
            _Form = new TApplicationPasswordEditorView();
            _Form.DataContext = this;
            return _Form.ShowDialog() ?? false;
        }
        #endregion

    }
}
