namespace HIO.ViewModels.Notify
{
    /// <summary>
    /// Interaction logic for TNotifyView.xaml
    /// </summary>
    public partial class TAboutView
    {
        public TAboutView()
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
    }
}
