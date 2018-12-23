using System;
using System.Windows.Interop;
using HIO.Backend;
namespace HIO
{
    /// <summary>
    /// Interaction logic for TMainView.xaml
    /// </summary>
    public partial class TMainView
    {
        public TMainView()
        {
  
                
            InitializeComponent();
        }
        public override bool canCloseonDC
        {
            get
            {
                return false;
            }
        
        }



        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

        
        }

        
    }
}
