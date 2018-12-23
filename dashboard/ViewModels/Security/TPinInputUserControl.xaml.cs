using HIO.Backend;
using HIO.Setup;
using System.Windows;
using System.Windows.Controls;

namespace HIO.ViewModels.Security
{
    /// <summary>
    /// Interaction logic for TPinInputUserControl.xaml
    /// </summary>
    public partial class TPinInputUserControl : UserControl
    {
        public TPinInputUserControl()
        {
            DataContext = HIOStaticValues.PinInputDashboardVM;
            InitializeComponent();
        }


        public TSetupWizard SetupWizard
        {
            get { return (TSetupWizard)GetValue(SetupWizardProperty); }
            set { SetValue(SetupWizardProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SetupWizard.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SetupWizardProperty =
            DependencyProperty.Register("SetupWizard", typeof(TSetupWizard), typeof(TPinInputUserControl), new PropertyMetadata(null, OnSetupWizardChanged));

        private static void OnSetupWizardChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (TPinInputUserControl)sender;
            ((TPinInputViewModel)control.DataContext).SetupWizard = (TSetupWizard)e.NewValue;
        }


    }
}
