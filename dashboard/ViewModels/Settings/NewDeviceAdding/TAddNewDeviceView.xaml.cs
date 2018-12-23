using HIO.Setup;
using HIO.WPF;
using System;
using System.Windows;

namespace HIO.ViewModels.Settings.NewDeviceAdding
{
    /// <summary>
    /// Interaction logic for TAddNewDeviceView.xaml
    /// </summary>
    public partial class TAddNewDeviceView
    {
        public TAddNewDeviceView()
        {
            InitializeComponent();
            this.SetDialogStandardSize();
        }

        protected override void OnClosing( System.ComponentModel.CancelEventArgs e)
        {
          

            base.OnClosing(e);
            var form = DataContext;
            (form as TAddNewDevice).ActivePage.OnHide();
            //(form as TNewDeviceAddingPage1).OnHide();
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
