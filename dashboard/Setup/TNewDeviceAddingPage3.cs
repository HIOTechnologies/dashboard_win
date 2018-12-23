using HIO.Backend;
using HIO.Setup;
using HIO.ViewModels.Settings;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace HIO.Setup
{
    public class TNewDeviceAddingPage3 : TSetupPageBase
    {
        public TNewDeviceAddingPage3(TWizard parent, double progressPercent) : base(parent, progressPercent)
        {
            Commands.AddCommand("ErrorTry", ErrorTry);
         //   colorBackground = "w";

            SyncronizingState = SyncronizingStateEnum.None;
        }



        #region Properties
        public SyncronizingStateEnum SyncronizingState
        {
            get
            {
                return GetValue<SyncronizingStateEnum>();
            }
            set
            {
                if (SetValue(value))
                {
                    Commands.Update();


                  
                }
            }
        }


        public override bool CanMoveNextPage
        {
            get
            {
                if (base.CanMoveNextPage && SyncronizingState == SyncronizingStateEnum.Completed)
                {
                 
                    MoveNextPage();
                    return true;
                }
                else return false;
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
            public string colorBackground
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
        public  double ProgressPercentBar
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
        public string MessageErr
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
        #endregion

        public void ErrorTry()
        {
            MessageErr = null;
            HIOStaticValues.commandQ.Add(() => Qsync());
           
        }


        //private void StartTimer()
        //{
        //    DispatcherTimer DT = new DispatcherTimer();
        //    DT.Interval = TimeSpan.FromSeconds(3);
        //    DT.Tick += (a, b) =>
        //    {

        //        SyncronizingState = SyncronizingStateEnum.None;
        //        ProgressPercent = 0;
        //        base.MoveNextPage();
        //        DT.Stop();
        //    };
        //    DT.Start();
        //}
        void Qsync() {
            Commands ic = new Commands();
           
            int res = ic.Sync();
            if (res != 1)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Message = null;
                    MessageErr = "Something went wrong!";
                }));
                return;
            }

            base.MoveNextPage();
        }
        public override void OnShow()
        {

            MessageErr = null;
            base.OnShow();
            //////Close Wizard
            HIO.Backend.HIOStaticValues.FirstRun = 1; 

            this.ProgressPercentBar = 0;
            if (SyncronizingState == SyncronizingStateEnum.None)
            {

                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    DispatcherTimer dt = new DispatcherTimer();
                    dt.Tick += Dt_Tick;
                    dt.Interval = TimeSpan.FromSeconds(1);
                    dt.Start();
                    SyncronizingState = SyncronizingStateEnum.Syncronizing;
                }));
                Task.Run(() =>
                {

                    HIOStaticValues.commandQ.Add(() =>QresetSync());
                    HIOStaticValues.commandQ.Add( ()=>Qsync());
                });


           
           




            }



        }

        private void QresetSync()
        {
            Commands cmd = new Backend.Commands();
            cmd.ResetSync();
        }

        public override void MoveNextPage()
        {
            ErrorMessage = "";
          

        }

        private void Dt_Tick(object sender, EventArgs e)
        {
            this.ProgressPercentBar = HIOStaticValues.ProgressPercent;
            if (HIOStaticValues.ProgressPercent > 99) (sender as DispatcherTimer).Stop();

        }

        public enum SyncronizingStateEnum
        {
            None = 1,
            Syncronizing = 2,
            Completed = 3,
        }


    }
}
