using HIO.Backend;
using HIO.Setup;
using HIO.ViewModels.Accounts;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace HIO.Setup
{
    public class TNewDeviceAddingPage4 : TSetupPageBase
    {
        TAccountManagerViewModel TabAccount =null; 

        public TNewDeviceAddingPage4(TWizard parent, double progressPercent, TAccountManagerViewModel TabAccount) : base(parent, progressPercent)
        {
            this.TabAccount = TabAccount;
        }
        //public override bool CanMoveNextPage
        //{
        //    get
        //    {
        //        return false;
        //    }
        //}

        public override void OnShow()
        {

            base.OnShow();
            ////////
            TabAccount.LoadData();
            Task.Run(() =>
            {
                if (HIOStaticValues.DirectBluetooth)
                {
                    var cmd = new Commands();
                    cmd.IsConnection();
                }
                Thread.Sleep(3000);
              //  if (CanMoveNextPage) MoveNextPage();
              

                MoveNextPage();

            });
           
        }

        public override bool CanMovePreviousPage => false;
     
    }
}
