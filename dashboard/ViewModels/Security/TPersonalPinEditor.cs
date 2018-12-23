using HIO.Core;
using System;

namespace HIO.ViewModels.Security
{
    public class TPersonalPinEditor : TViewModelBase
    {
        public TPersonalPinEditor()
        {
            Commands.AddCommand("Apply", Apply);
            Commands.AddCommand("ErrorOK", ErrorOK);
        }

        #region Fields
        private TPersonalPinEditorView _Form;

        #endregion

        #region Properties

        public TSecurityManager Parent { get; private set; }

        public string CurrentPin
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
        public string NewPin
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
        public string ReEnterNewPin
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
                SetValue(value);
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

        private void Apply()
        {
            Backend.Commands cmd = new Backend.Commands();
            if (CurrentPin?.Length != 6 || NewPin?.Length != 6)
            {
                ErrorMessage = "Your pin code must be 6 characters.";
                return;
            }
            if (NewPin != ReEnterNewPin)
            {
                ErrorMessage = "Pins do not match, please retype.";
                return;
            }
            //Validate Pin
            // Parent.ChangePersonalPin(NewPin);

            var changePinResult = cmd.ChangePin(CurrentPin, NewPin);
            CurrentPin = null;
            NewPin = null;
            ReEnterNewPin = null;
            ErrorOK();
            var hioDisabledMessage = "HIO is disabled\nTry again in {TimeRemaining}";
            ErrorMessageExpiry = null;
            switch (changePinResult)
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
                    ErrorMessage = "Wrong pincode";
                    return;
                case -5:
                    ErrorMessage = "Something went wrong!";
                    return;
            }
            ErrorOK();
            _Form.Close();
        }

        public bool Show(TSecurityManager securityManager)
        {
            Parent = securityManager;
            _Form = new TPersonalPinEditorView();
            _Form.DataContext = this;
            return _Form.ShowDialog() ?? false;
        }

        private void ErrorOK()
        {
            ErrorMessage = "";
        }
        #endregion

    }
}
