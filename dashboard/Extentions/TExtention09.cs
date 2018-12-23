using HIO.Backend;
using HIO.Core;
using HIO.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HIO.Extentions
{
    public class TExtention09 : TViewModelBase,IExtention
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);
        [StructLayout(LayoutKind.Sequential)]

        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }
        public bool IsClosed { get; private set; }
        string url, title;
        public TExtention09()
        {
          //  HIOStaticValues.AdminExtention.ShowOnly(this);
        }
        
        #region Fields

        private TExtention09View _Form;
        #endregion

        #region Methods
        public void Initialize(string url, string title)
        {
            Commands.AddCommand("NotNow", NotNow);
            Commands.AddCommand("Add", Add);
            this.url = url;
            this.title = title;
        }
        private  void Add()
        {
            TExtention02 addUser = new TExtention02();
            HIOStaticValues.AdminExtention.ShowOnly(addUser);
            Close();
            addUser.Show(new ViewModels.Accounts.TAccountItem { Url = url, Name = title });
          

        }

        private void NotNow()
        {
         Close();


        }

        public void Show()
        {
            IsClosed = false;
            _Form = new TExtention09View();
            _Form.DataContext = this;
            _Form.Closing += _Form_Closing;
            _Form.Deactivated += _Form_Deactivated;

            _Form.ShowAsActive();
        }
    
    


    private void _Form_Deactivated(object sender, EventArgs e)
        {
            if (!IsClosed)
                Close();
        }

        private void _Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClosed = true;
          
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
        #endregion

    }

}
