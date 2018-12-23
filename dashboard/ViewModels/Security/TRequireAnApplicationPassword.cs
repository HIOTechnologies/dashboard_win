using HIO.Controls;
using HIO.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIO.ViewModels.Security
{
    public class TRequireAnApplicationPassword : TViewModelBase
    {
        public TRequireAnApplicationPassword()
        {
            Commands.AddCommand("Apply", Apply);
        }

        private TRequireAnApplicationPasswordView _Form;

        public TSecurityManager Parent { get; set; }

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

        private void Apply()
        {
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
            
            TMessageBox.Show("Password saved successfully.");
            _Form.Close();
        }

        public bool Show(TSecurityManager securityManager)
        {
            Parent = securityManager;
            _Form = new TRequireAnApplicationPasswordView();
            _Form.DataContext = this;
            return _Form.ShowDialog() ?? false;
        }
    }
}
