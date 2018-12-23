using HIO.ViewModels;
using HIO.ViewModels.MagicLock;
using HIO.ViewModels.Settings;

namespace HIO.Setup
{
    public class TSetupWizard : TWizard
    {
        public TSetupWizard(TMain parent)
        {
            Parent = parent;
            Pages.Add(new TSetupPage1(this, 0));
            Pages.Add(new TSetupPage2(this, 14));
            Pages.Add(new TSetupPage3(this, 28));
            Pages.Add(new TSetupPage3_2(this, 42));
            Pages.Add(new TSetupPage4(this, 56));
            Pages.Add(new TSetupPage5(this, 70));
            Pages.Add(new TSetupPage5_2(this, 85));
            Pages.Add(new TSetupPage6(this, 100));



            Pages.Add(new TNewDeviceAddingPage1(this, 0, parent.SettingManager) { ProgressAnimationDuration = 0 });
            Pages.Add(new TNewDeviceAddingPage2(this, 17));
            Pages.Add(new TNewDeviceAddingPage3(this, 34));
            Pages.Add(new TNewDeviceAddingPage3_2(this, 43));
            Pages.Add(new TNewDeviceAddingPage4(this, 62, parent.AccountManager));
            //Pages.Add(new TPcLockerEditor(this, 69, Parent.MagicLockManager));
            Pages.Add(new TImport(this, 86, Parent.SettingManager));
            Pages.Add(new TSetupImportComplete(this, 100));
        }

        #region Fields
        private TSetupWizardView _Form;
        #endregion

        #region Properties
        public TMain Parent { get; private set; }

        #endregion

        #region Merthods


        public override void Complete()
        {
            try
            {
                _Form?.Close();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }


        public void Show()
        {
            //ActivePage = Pages[6];
            MoveNextPage();
            _Form = new TSetupWizardView();
            _Form.DataContext = this;
            if (System.Windows.SystemParameters.PrimaryScreenWidth < 1400)
            {
                _Form.Height = 640;
                _Form.Width = 1024;
            }
            else
            {
                _Form.Height = 768;
                _Form.Width = 1366;
            }
            _Form.ShowDialog();
        }
        #endregion

    }
}
