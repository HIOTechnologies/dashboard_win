using HIO.Backend;
using HIO.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HIO.ViewModels.Notify
{
    public class TAbout : TViewModelBase
    {
        public TAbout()
        {

             
        }



        public string VersionApp{

            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }

        }

        public bool IsClosed { get; private set; }

        private TAboutView _Form;

        public void Show(string ver)
        {
            IsClosed = false;
            _Form = new TAboutView();
            _Form.DataContext = this;
            VersionApp = "Version: "+ver;
            _Form.WindowStartupLocation = WindowStartupLocation.Manual;
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            _Form.Left = desktopWorkingArea.Right - _Form.Width;
            _Form.Top = desktopWorkingArea.Bottom - _Form.Height;
            _Form.Show();
        }
        public bool IsFormOpen
        {
            get
            {
                return _Form != null;
            }
        }

        public Window Form
        {
            get
            {
                return _Form;
            }

        }
        public void Close()
        {
            _Form?.Close();
            _Form = null;
        }
    }

}
