using HIO.Core;
using System;

namespace HIO.ViewModels.Security
{
    public class TDisablePersonalPin : TViewModelBase
    {
        public TDisablePersonalPin()
        {
            Commands.AddCommand("Apply", Apply, () => CanSave);
            Commands.AddCommand("ErrorOK", ErrorOK);
        }

        #region Fields
        private TDisablePersonalPinView _Form;
        #endregion

        #region Properties
        public bool CanSave
        {
            get
            {
                return !PersonalPin.IsNullOrEmpty() && PersonalPin.Length == 6;
            }
        }
        public TSecurityManager SecurityManager { get; set; }

        public string PersonalPin
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


        public string ErrorMessage
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
        public TimeSpan? ErrorMessageExpiry
        {
            get
            {
                return GetValue<TimeSpan?>();
            }
            set
            {
                SetValue(value);
            }
        }
        #endregion

        #region Methods
        private void ErrorOK()
        {
            ErrorMessage = null;
        }
        private void Apply()
        {
            Backend.Commands cmd = new Backend.Commands();
            int res = cmd.DisablePin(PersonalPin);
            var hioDisabledMessage = "HIO is disabled\nTry again in {TimeRemaining}";
            ErrorMessageExpiry = null;
            PersonalPin = null;
            ErrorOK();
            switch (res)
            {
                case -1:
                    ErrorMessageExpiry = TimeSpan.FromMinutes(1);
                    ErrorMessage = hioDisabledMessage;
                    return;
                case -2:
                    ErrorMessageExpiry = TimeSpan.FromMinutes(5);
                    ErrorMessage = hioDisabledMessage;
                    return;
                case -3:
                    ErrorMessageExpiry = TimeSpan.FromMinutes(10);
                    ErrorMessage = hioDisabledMessage;
                    return;
                case -4:
                    ErrorMessageExpiry = TimeSpan.FromHours(1);
                    ErrorMessage = hioDisabledMessage;
                    return;
                case 0:
                    ErrorMessage = "Incorrect personal pin";
                    return;
                case -5:
                    ErrorMessage = "Something went wrong!";
                    return;
            }
            ErrorOK();
            SecurityManager.IsPinEnabled = false;
            _Form.Close();
        }

        public bool Show(TSecurityManager securityManager)
        {
            SecurityManager = securityManager;
            _Form = new TDisablePersonalPinView();
            _Form.DataContext = this;
            return _Form.ShowDialog() ?? false;
        }
        #endregion

    }
}
