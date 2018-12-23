namespace HIO.ViewModels.Security
{
    public class TSecurityManager : TTabBase
    {
        public TSecurityManager(TTabManager parent) : base(parent)
        {
            Commands.AddCommand("DisableYourPin", DisableYourPin);
            Commands.AddCommand("ChangeYourPersonalPin", ChangeYourPersonalPin);
            Commands.AddCommand("SetupPersonalPin", SetupPersonalPin);

            Commands.AddCommand("RequireAnApplicationPassword", RequireAnApplicationPassword);
            Commands.AddCommand("ChangeApplicationPassword", ChangeApplicationPassword);



        }

        #region Properties
        public bool IsPinEnabled
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
        public override string Title
        {
            get
            {
                return "Security";
            }
        }

        public override string HoverImageUrl
        {
            get
                {
                return "pack://application:,,,/HIO;component/Resources/Buttons/security.png";
            }
        }

        public override string NormalImageUrl
        {
            get
            {
                return "pack://application:,,,/HIO;component/Resources/Buttons/security2.png";
            }
        }

        #endregion

        #region Methods
        public void SaveApplicationPassword(string password)
        {
            //TODO: Save Application Password
        }

        private void ChangeYourPersonalPin()
        {
            TPersonalPinEditor editor = new TPersonalPinEditor();
            editor.Show(this);
        }

        private void SetupPersonalPin()
        {
            var editor = new TSetupPersonalPin();
            editor.Show(this);
        }

        private void DisableYourPin()
        {
            TDisablePersonalPin editor = new TDisablePersonalPin();
            editor.Show(this);
        }
        private void ChangeApplicationPassword()
        {
            TApplicationPasswordEditor editor = new TApplicationPasswordEditor();
            editor.Show(this);
        }

        private void RequireAnApplicationPassword()
        {
            TRequireAnApplicationPassword editor = new TRequireAnApplicationPassword();
            editor.Show(this);
        }
        #endregion
    }
}
