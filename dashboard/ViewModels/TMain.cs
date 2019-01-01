using HIO.Backend;
using HIO.Backend.Bridge;
using HIO.Core;
using HIO.Setup;
using HIO.ViewModels.Accounts;
using HIO.ViewModels.MagicLock;
using HIO.ViewModels.Security;
using HIO.ViewModels.Settings;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace HIO.ViewModels
{
    public class TMain : TViewModelBase
    {
        public TMain()
        {

            TabManager = new TTabManager(this);
            AccountManager = new TAccountManagerViewModel(TabManager);
            MagicLockManager = new TMagicLockManager(TabManager);
            SecurityManager = new TSecurityManager(TabManager);
            SettingManager = new TSettingManager(TabManager);

            TabManager.AddItem(AccountManager);
            TabManager.AddItem(MagicLockManager);
            TabManager.AddItem(SecurityManager);
            TabManager.AddItem(SettingManager);

            //AccountManager.IsEnabled = false;
            //MagicLockManager.IsEnabled = false;
            //SecurityManager.IsEnabled = false;

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;



        }
        public void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                HIOStaticValues.ISLOCK = true;
            }
            else
            {
                HIOStaticValues.ISLOCK = false;
            }
        }


        public static TMain Instance { get; set; }

        #region Fields
        private TMainView _Form;
        #endregion

        #region Properties

        public bool CanShowSetup
        {
            get
            {
                //TODO: Return Valid Value.

                if (HIOStaticValues.FirstRun == 0)
                    return true;
                else return false;
            }
        }
        public TTabManager TabManager { get; private set; }
        public TAccountManagerViewModel AccountManager { get; private set; }
        public TMagicLockManager MagicLockManager { get; private set; }
        public TSecurityManager SecurityManager { get; private set; }
        public TSettingManager SettingManager { get; private set; }

        public bool IsConnected
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                if (SetValue(value))
                {
#if Test_Debug
                    return;
#endif
                    AccountManager.IsEnabled = IsConnected;
                    MagicLockManager.IsEnabled = IsConnected;
                    SecurityManager.IsEnabled = IsConnected;
                    SettingManager.IsEnabled = true;
                    if (!IsConnected)
                    {
                        TabManager.ActiveTab = SettingManager;
                    }
                    else
                    {

                        HIOStaticValues.commandQ.Add(() =>
                        {
                            var commands = new Commands();
                            var checkPin = commands.CheckPin();
                            IsPinRequired = checkPin == CheckPinStatus.Enabled;
                            SecurityManager.IsPinEnabled = checkPin != CheckPinStatus.Disabled;
                            TabManager.ActiveTab = AccountManager;
                        });
                    }
                }
            }
        }

        public bool IsPinRequired
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                if (SetValue<bool>(value))
                {
                    HIOStaticValues.PinInputDashboardVM?.OnPropertyChanged(nameof(TPinInputViewModel.IsPinRequired));
                    HIOStaticValues.PinInputExtensionVM?.OnPropertyChanged(nameof(TPinInputViewModel.IsPinRequired));
                }
            }
        }
        #endregion

        #region Methods
        public void Start(bool silentMode)
        {
            //new TestExtentions().Show();
            //return;


            Task.Run(() =>
            {
                InitializeAsync(this);
            });
            if (CanShowSetup)
            {
                Show(); //trick for get tmain
                new TSetupWizard(this).Show();
                Show();
            }
            else
            {
                if(!silentMode)
                Show();
            }

        }


        private async void InitializeAsync(TMain tMain)
        {

            Task.Run(() =>
            {
                ExtenstionConnection ec = new ExtenstionConnection();
                ec.ChromeConnectionSend(tMain);
            });

            Task.Run(() =>
            {
                deviceListAndOpen devList = new deviceListAndOpen();
                devList.ConnectToBridge(tMain);
            });

            HIOStaticValues._signalCheckDevice.WaitOne(); //FREE
            Commands ic = new Commands();
            bool res = await Task.Run(() => { return ic.IsConnection(); });
            if (res == true)
            {

                IsConnected = true;
            }
            else
            {
                IsConnected = false;
            }

        }
        public void Show()
        {

            if (_Form == null)
            {
                _Form = new TMainView();
                _Form.DataContext = this;
                _Form.Closed += (a, b) =>
                {
                    _Form = null;
                };
            }
            else
            {
                Application app = App.Current;
                if (!CanShowSetup)
                {
                    _Form.WindowState = WindowState.Normal;
                    _Form.Activate();
                }
            }


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
            if (!CanShowSetup)
            {
                _Form.Show();
                _Form.Activate();
                try
                {
                    HIOStaticValues.scale = PresentationSource.FromVisual(App.Current.MainWindow).CompositionTarget.TransformToDevice.M11;
                }
                catch (Exception ex) { HIOStaticValues.scale = 1; }
            }
        }
        #endregion
    }
}
