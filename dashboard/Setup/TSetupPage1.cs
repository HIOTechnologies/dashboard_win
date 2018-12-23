using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace HIO.Setup
{
    public class TSetupPage1 : TSetupPageBase
    {
        public TSetupPage1(TWizard parent, double progressPercent) : base(parent, progressPercent)
        {

        }
        private DispatcherTimer dispatcherTimer;


        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            dispatcherTimer?.Stop();
            ParentWizard.MoveNextPage();
        }

        
        public override bool AllowShowToolbox => false;

        public override void OnShow()
        {
            if (dispatcherTimer == null)
            {
                dispatcherTimer = new DispatcherTimer(DispatcherPriority.Loaded);
                dispatcherTimer.Interval = TimeSpan.FromSeconds(5);
                dispatcherTimer.Tick += DispatcherTimer_Tick;
            }
            dispatcherTimer.Start();
        }

    }
}
