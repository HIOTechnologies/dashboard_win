using HIO.Backend;
using HIO.Controls;
using HIO.Core;
using HIO.ViewModels;
using HIO.ViewModels.Accounts;
using HIO.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace HIO.Extentions
{
    public class TExtentionMenu : TViewModelBase, IExtention
    {
      
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
        string url, title;
        public TExtentionMenu()
        {
            Commands.AddCommand("AddItem", AddItem);
            Commands.AddCommand("GenerateSecurePassword", GenerateSecurePassword);
            Commands.AddCommand("Dashboard", Dashboard);
            Commands.AddCommand("AllItems", AllItems);
        }
        public void Initialize(string url, string title)
        {
            this.url = url;
            this.title = title;

        }

        public bool IsClosed { get; private set; }
        private TExtentionMenuView _Form;
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
        private void Dashboard()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                TMain.Instance?.Show();
            }));
        }
        private void AllItems()
        {
            DataBase db = new DataBase();
            List<LoginFieldS> lnFields = db.getInfoFromDB("*", "", "");
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {


                HIOStaticValues.AdminExtention.CloseAll();
                HIOStaticValues.AdminExtention.Extention08.Initialize(lnFields);
                HIOStaticValues.AdminExtention.Extention08.Show();

            }));
        }
        private void GenerateSecurePassword()
        {

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {

                try
                {
                    HIOStaticValues.AdminExtention.CloseAll();
                    HIOStaticValues.AdminExtention.Extentiongp.Initialize((Source)HIOStaticValues.SOURCE);
                    HIOStaticValues.AdminExtention.Extentiongp.Show(false, false);

                }
                catch (Exception ex)
                {
                   
                }
            }));


        }

        private void AddItem()
        {

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                HIOStaticValues.AdminExtention.CloseAll();
                HIOStaticValues.AdminExtention.Extention02.Show(new TAccountItem { Url = url, Name = title });
            }));
        }
        private void _Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClosed = true;

        }

        private void _Form_Deactivated(object sender, EventArgs e)
        {
            Close();
        }


        public void Close()
        {
            if (!IsClosed)
                _Form?.Close();
            _Form = null;
        }
        public void Show()
        {
          
            IsClosed = false;
            _Form = new TExtentionMenuView();
            _Form.DataContext = this;
            _Form.Deactivated += _Form_Deactivated;
            _Form.Closing += _Form_Closing;
          

            _Form.ShowAsActive();


        }


    }
}
