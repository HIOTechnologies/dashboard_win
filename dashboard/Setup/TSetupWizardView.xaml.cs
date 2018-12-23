using System;
using System.Windows.Interop;
using HIO.Backend;
using HIO.ViewModels.Settings.NewDeviceAdding;

namespace HIO.Setup
{
    /// <summary>
    /// Interaction logic for TSetupWizardView.xaml
    /// </summary>
    public partial class TSetupWizardView
    {
        public TSetupWizardView()
        {
            InitializeComponent();
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
                    
        }
        public override bool canCloseonDC
        {
            get
            {
                return false;
            }

        }


    }
}
