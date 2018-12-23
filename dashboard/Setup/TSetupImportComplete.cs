using HIO.Setup;
using HIO.ViewModels.Accounts;
using HIO.ViewModels.Settings;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HIO.Setup
{
    public class TSetupImportComplete : TSetupPageBase
    {
        private DateTimeOffset showTime;
        public TSetupImportComplete(TWizard parent, double progressPercent) : base(parent, progressPercent)
        {
        }

        public override void OnShow()
        {
            showTime = DateTime.Now;
            if (PreviousPage is TImport PrevStep && !PrevStep.IsComplete/*Skip button is Clicked*/)
            {
                MoveNextPage();
            }
            base.OnShow();

            Task.Run(() =>
            {
                Thread.Sleep(4000);
                Application.Current.Dispatcher.Invoke(() => MoveNextPage());
            });

        }

        public override bool CanMovePreviousPage => false;
    }
}
