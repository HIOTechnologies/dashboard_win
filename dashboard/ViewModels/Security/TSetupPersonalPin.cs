using HIO.Core;

namespace HIO.ViewModels.Security
{
    public class TSetupPersonalPin : TViewModelBase
    {
        public TSetupPersonalPin()
        {
            Commands.AddCommand("Apply", Apply);
            Commands.AddCommand("ErrorOK", ErrorOK);
        }

        #region Fields
        private TSetupPersonalPinView _Form;

        #endregion

        #region Properties

        public TSecurityManager Parent { get; private set; }
        public bool AllSet
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
        #endregion

        #region Methods
        private void ErrorOK()
        {
            ErrorMessage = null;
        }
        private void Apply()
        {
            if (NewPin?.Length != 6)
            {
                ErrorMessage = "Your pin code must be 6 characters.";
                return;
            }
            if (NewPin != ReEnterNewPin)
            {
                ErrorMessage = "Pins do not match, please retype.";
                return;
            }
            Backend.Commands cmd = new Backend.Commands();
            int res = cmd.SetPin(NewPin);
            NewPin = null;
            ReEnterNewPin = null;
            ErrorOK();
            switch (res)
            {
                case 1:
                    //  SecurityManager.DisablePersonalPin();
                    ErrorMessage = null;
                    Parent.IsPinEnabled = true;
                    AllSet = true;
                    break;
                default:
                    ErrorMessage = "Something went wrong!";
                    break;
            }
        }

        public bool Show(TSecurityManager securityManager)
        {
            Parent = securityManager;
            _Form = new TSetupPersonalPinView();
            _Form.DataContext = this;
            return _Form.ShowDialog() ?? false;
        }

        #endregion

    }
}
