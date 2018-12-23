using HIO.Backend;
using HIO.ViewModels.Settings;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIO.Setup
{
    public class TNewDeviceAddingPage2 : TSetupPageBase
    {
        private readonly object Locker = new object();
        public TNewDeviceAddingPage2(TWizard parent, double progressPercent) : base(parent, progressPercent)
        {
            Commands.AddCommand("ErrorOK", ErrorOK);
            IsConnecting = false;
        }



        #region Properties



        public override bool CanMoveNextPage
        {
            get
            {
                if (HIOStaticValues.DirectBluetooth || HIOStaticValues.CONNECTIONBHIO)
                {
                    MoveNextPage();
                    return true;
                }
                else return false;
            }
        }

        public string PairingCode
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

                    if (!value.IsNullOrEmpty() && value.Length == 6)
                        MoveNextPage();
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

        public bool IsConnecting
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




        public new double ProgressPercent
        {
            get
            {
                return GetValue<double>();
            }
            set
            {
                SetValue(value);
            }
        }

        public TNewDeviceAddingPage1 NewDeviceAddingPage1
        {
            get
            {
                return ParentWizard.Pages.OfType<TNewDeviceAddingPage1>().FirstOrDefault();
            }
        }
        public TDevice SelectedDevice
        {
            get
            {
                return NewDeviceAddingPage1?.SelectedItem;
            }
        }

        #endregion



        public override void OnShow()
        {


            base.OnShow();
            ErrorMessage = null;
            if (!HIOStaticValues.DirectBluetooth)
            {
                HIOStaticValues.commandQ.Add(() =>
                {
                    var cmd = new Commands();
                    cmd.IsConnection();
                });
            }
        }

        void ErrorOK()
        {


            PairingCode = null;
            ErrorMessage = null;

        }
        private void QBond()
        {
            lock (Locker)
            {
                Trace.WriteLine($"Page2 QBond CanRead/Write: {HIOStaticValues.BaS.dev.CanRead} {HIOStaticValues.BaS.dev.CanWrite}");
                Commands ic = new Commands();
                Converts conv = new Converts();
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    IsConnecting = true;
                }));
                string res = ic.Bond(SelectedDevice.Mac, Encoding.UTF8.GetBytes(PairingCode));
                IsConnecting = false;
                switch (res)
                {
                    case StatusWord.SW_PIN_NOT_VERIFIED:
                        App.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ErrorMessage = "Incorrect pairing code";
                            PairingCode = null;
                        }));
                        break;
                    case StatusWord.SW_NO_ERROR:

                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            base.MoveNextPage();
                        }));
                        break;

                    case StatusWord.SW_RECEIVE_TIMEOUT:
                        App.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            Message = "Please keep the HIO as close as it's possible and click/double tap it.";
                            PairingCode = null;
                        }));
                        break;

                    default:
                        App.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {


                            Message = $"Something went wrong!({StatusWord.sw[res]})\nPlease try again.";
                            PairingCode = null;
                        }));

                        break;

                }
            }

        }
        public override void MoveNextPage()
        {

            ErrorMessage = "";
            if (HIOStaticValues.DirectBluetooth || HIOStaticValues.CONNECTIONBHIO)
            {
                base.MoveNextPage();
                return;
            }

            Task.Run(() =>
            {
                HIOStaticValues.commandQ.Add(() => QBond());

            });


        }



        public enum SyncronizingStateEnum
        {
            None = 1,
            Syncronizing = 2,
            Completed = 3,
        }


    }
}
