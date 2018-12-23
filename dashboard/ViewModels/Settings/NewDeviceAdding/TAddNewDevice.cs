using HIO.Core;
using HIO.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HIO.Backend;
using System.Windows.Threading;
using System.Windows;
using HIO.ViewModels.Accounts;

namespace HIO.ViewModels.Settings.NewDeviceAdding
{
    public class TAddNewDevice : TWizard
    {
        public TAddNewDevice(TAccountManagerViewModel tabAcc)
        {
            Step1 = new TNewDeviceAddingPage1(this, 0, Parent);
            Step2 = new TNewDeviceAddingPage2(this, 35);
            Step3 = new TNewDeviceAddingPage3(this, 65);
            Step4 = new TNewDeviceAddingPage4(this, 100, tabAcc);
            Pages.Add(Step1);
            Pages.Add(Step2);
            Pages.Add(Step3);
            Pages.Add(Step4);
        }

        #region Fields
        private TAddNewDeviceView _Form;
        #endregion

        #region Properties
        public TSettingManager Parent { get; set; }

        public TNewDeviceAddingPage1 Step1 { get; private set; }
        public TNewDeviceAddingPage2 Step2 { get; private set; }
        public TNewDeviceAddingPage3 Step3 { get; private set; }
        public TNewDeviceAddingPage4 Step4 { get; private set; }

        #endregion

        public override void Complete()
        {
            base.Complete();
            Application.Current.Dispatcher.Invoke(() => _Form?.Close());
        }

        public void Show(TSettingManager settingManager)
        {

            Parent = settingManager;
            MoveNextPage();
            _Form = new TAddNewDeviceView();
            _Form.DataContext = this;
            _Form.Owner = Application.Current.MainWindow as TMainView;
            _Form.ShowDialog();
        }

  
    }
}
