using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIO.Setup
{
    public class TSetupPage2 : TSetupPageBase
    {
        public TSetupPage2(TSetupWizard parent, double progressPercent) : base(parent, progressPercent)
        {

        }

        public override bool CanMovePreviousPage
        {
            get
            {
                return false;
            }
        }

    }
}
