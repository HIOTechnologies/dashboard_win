namespace HIO.ViewModels.Security
{
    /// <summary>
    /// Interaction logic for TSecurityManagerView.xaml
    /// </summary>
    public partial class TSecurityManagerView
    {
        public TSecurityManagerView()
        {
            InitializeComponent();
            Expander_ApplicaionPassword.Expanded += Expander_ApplicaionPassword_Expanded;
            Expander_PersonalPin.Expanded += Expander_PersonalPin_Expanded;
        }

        private void Expander_PersonalPin_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            Expander_ApplicaionPassword.IsExpanded = false;
        }

        private void Expander_ApplicaionPassword_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            Expander_PersonalPin.IsExpanded = false;
        }
    }
}
