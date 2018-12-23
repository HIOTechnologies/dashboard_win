using HIO.Backend;
using HIO.Core;
using HIO.Setup;
using System;
using System.Linq;
using System.Windows.Threading;

namespace HIO.ViewModels.Security
{
    public class TPinInputViewModel : TViewModelBase
    {
        private static DateTime? _PinLockEnd;
        private static readonly object _Lock = new object();
        public event EventHandler OnSubmit;

        public TPinInputViewModel()
        {
            lock (_Lock)
            {
                if (_PinLockEnd != null)
                    StartTimer();
            }
            Commands.AddCommand("SubmitPersonalPin", SubmitPersonalPin, CanSubmitPersonalPin);
            Commands.AddCommand("ResetPersonalPin", ResetPersonalPin, CanResetPersonalPin);
            Commands.AddCommand("ResetPersonalPinOK", ResetPersonalPinOK, CanResetPersonalPin);
            Commands.AddCommand("ResetPersonalPinCancel", ResetPersonalPinCancel);
            Commands.AddCommand("AddNewDevice", AddNewDevice, CanAddNewDevice);
        }

        public TSetupWizard SetupWizard { get; set; }

        public bool IsPinRequired
        {
            get
            {
                return HIOStaticValues.tmain.IsPinRequired;
            }
        }
        public bool ShowResetPersonalPin
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
        public string PersonalPin
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
                Commands.Update();
            }
        }
        public string PersonalPinErrorMessage
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
        public string PersonalPinFooterErrorMessage
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
        private DispatcherTimer _Timer = null;
        private void StartTimer()
        {
            StopTimer();
            if (_PinLockEnd == null)
                return;
            if (DateTime.Now < _PinLockEnd)
            {
                SetPersonalPinFooterErrorMessage();
            }
            _Timer = new DispatcherTimer();
            _Timer.Interval = TimeSpan.FromSeconds(1);
            _Timer.Tick += _Timer_Tick;
            _Timer.Start();
        }

        private void _Timer_Tick(object sender, EventArgs e)
        {
            if (_PinLockEnd == null || DateTime.Now >= _PinLockEnd.Value.AddSeconds(-1))
            {
                PinLockEnd = null;
                HIOStaticValues.PinInputDashboardVM?.Commands.Update();
                HIOStaticValues.PinInputExtensionVM?.Commands.Update();
                return;
            }

            SetPersonalPinFooterErrorMessage();
        }

        private void SetPersonalPinFooterErrorMessage(string message = null)
        {
            if (message == null)
            {
                PersonalPinFooterErrorMessage = $"HIO is disabled\nTry again in {(PinLockEnd.Value - DateTime.Now).ToReadableString()}";
            }
            else
            {
                PersonalPinFooterErrorMessage = message;
            }
            if (HIOStaticValues.PinInputDashboardVM != null)
                HIOStaticValues.PinInputDashboardVM.PersonalPinFooterErrorMessage = PersonalPinFooterErrorMessage;
            if (HIOStaticValues.PinInputExtensionVM != null)
                HIOStaticValues.PinInputExtensionVM.PersonalPinFooterErrorMessage = PersonalPinFooterErrorMessage;
        }

        private void StopTimer()
        {
            if (_Timer != null)
            {
                _Timer.Stop();
                _Timer = null;
            }
        }

        public DateTime? PinLockEnd
        {
            get
            {
                return _PinLockEnd;
            }
            set
            {
                lock (_Lock)
                {
                    if (_PinLockEnd == value)
                        return;
                    _PinLockEnd = value;
                    if (value == null)
                    {
                        SetPersonalPinFooterErrorMessage("");
                        PersonalPinErrorMessage = null;
                        StopTimer();
                    }
                    else
                    {
                        StartTimer();
                    }
                }
            }
        }

        private void SubmitPersonalPin()
        {
            Commands cmd = new Backend.Commands();
            var pinVerificationResult = cmd.SetPin(PersonalPin);
            PersonalPin = null;
            PersonalPinErrorMessage = null;
            if (pinVerificationResult == 1)
            {
                HIOStaticValues.tmain.IsPinRequired = false;
                ShowResetPersonalPin = false;
                if (SetupWizard != null)
                    SetupWizard.MoveNextPage();
                OnSubmit?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                HIOStaticValues.tmain.IsPinRequired = true;
                PersonalPinErrorMessage = "Wrong pincode";
                switch (pinVerificationResult)
                {
                    case 0:
                        PinLockEnd = null;
                        break;
                    case -1:
                        PinLockEnd = DateTime.Now.AddMinutes(1);
                        break;
                    case -2:
                        PinLockEnd = DateTime.Now.AddMinutes(5);
                        break;
                    case -3:
                        PinLockEnd = DateTime.Now.AddMinutes(10);
                        break;
                    case -4:
                        PinLockEnd = DateTime.Now.AddHours(1);
                        break;
                    default:
                        PersonalPinErrorMessage = null;
                        SetPersonalPinFooterErrorMessage("Something went wrong!");
                        HIOStaticValues.PinInputDashboardVM?.OnPropertyChanged(nameof(PersonalPinFooterErrorMessage));
                        HIOStaticValues.PinInputExtensionVM?.OnPropertyChanged(nameof(PersonalPinFooterErrorMessage));
                        PinLockEnd = null;
                        break;
                }
            }
        }

        private bool CanSubmitPersonalPin()
        {
            return _PinLockEnd == null && PersonalPin?.Length == 6;
        }

        private bool CanAddNewDevice()
        {
            return _PinLockEnd == null;
        }

        private bool CanResetPersonalPin()
        {
            return _PinLockEnd == null;
        }

        private void ResetPersonalPin()
        {
            ShowResetPersonalPin = true;
        }
        private void ResetPersonalPinOK()
        {

            ShowResetPersonalPin = false;
            Commands cmd = new Backend.Commands();
            if (!cmd.EraseAll())
            {
                HIOStaticValues.popUp("Something went wrong!");
                return;
            }
            if (SetupWizard != null)
                SetupWizard.ActivePage = SetupWizard.Pages.First(t => t is TNewDeviceAddingPage1);
        }
        private void ResetPersonalPinCancel()
        {
            ShowResetPersonalPin = false;
        }
        private void AddNewDevice()
        {
            PersonalPin = null;
            PersonalPinErrorMessage = null;

            if (SetupWizard != null)
            {
                SetupWizard.ActivePage = SetupWizard.Pages.First(t => t is TNewDeviceAddingPage1);
            }
            else
            {
                HIOStaticValues.tmain.SettingManager.AddNewDevice();
            }
        }
    }
}
