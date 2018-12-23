using HIO.Backend;
using HIO.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HIO.Extentions
{
    public class TExtention13 : TViewModelBase,IExtention
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }
        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();
        DispatcherTimer dt = new DispatcherTimer();
        public TExtention13()
        {
          //  HIOStaticValues.AdminExtention.ShowOnly(this);
            dt.Interval = TimeSpan.FromSeconds(3);
            
            dt.Tick += Dt_Tick;

             
        }

        private void Dt_Tick(object sender, EventArgs e)
        {
            dt?.Stop();
            _Form?.Close();
           // timer.Stop();
        }

        public string Text{

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

        private TExtention13View _Form;

        public void Show(string text)
        {
            IsClosed = false;
            _Form = new TExtention13View();
            _Form.DataContext = this;
            Text = text;
            var hwndDesktop= GetDesktopWindow();
            RECT rect=new RECT();
            GetWindowRect(new HandleRef(null, hwndDesktop), out rect);
            _Form.WindowStartupLocation = WindowStartupLocation.Manual;
            _Form.Left = (rect.Right / HIOStaticValues.scale) - _Form.Width+20;
            _Form.Top = (rect.Bottom / HIOStaticValues.scale)- _Form.Height-10;

            dt.Start();
            _Form.MouseMove += _Form_MouseMove;
            _Form.MouseLeave += _Form_MouseLeave;
            _Form.Show();

        }

        private void _Form_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            dt.Start();
        }

        private void _Form_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            dt.Stop();
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
