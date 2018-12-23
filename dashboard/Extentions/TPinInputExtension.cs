using HIO.Backend;
using HIO.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;

namespace HIO.Extentions
{
    public class TPinInputExtension : TViewModelBase, IExtention
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

        private TPinInputExtensionView _Form;

        public bool IsClosed { get; private set; }

        public bool? Show()
        {
            IsClosed = false;
            int topSize = 50;
            _Form = new TPinInputExtensionView();
            HIOStaticValues.PinInputExtensionVM.OnSubmit += (s, e) => { try { _Form.DialogResult = true; } catch { } };
            _Form.DataContext = HIOStaticValues.PinInputExtensionVM;
            //  _Form.DialogResult = false;
            _Form.Closing += _Form_Closing;
            _Form.Deactivated += _Form_Deactivated;
            Screen scr = Screen.FromPoint(Cursor.Position);
            _Form.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            _Form.Left = scr.WorkingArea.Right / HIOStaticValues.scale - _Form.Width - 16;
            _Form.Top = (scr.WorkingArea.Top / HIOStaticValues.scale) + topSize;

            _Form.ShowActivated = true;
            _Form.Topmost = true;

            _Form.Activate();
            _Form.Focus();
            var res = _Form.ShowDialog();
            return res;
        }

        private void _Form_Deactivated(object sender, EventArgs e)
        {
            if (!IsClosed)
                Close();
        }

        private void _Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (HIOStaticValues.PinInputExtensionVM != null)
                HIOStaticValues.PinInputExtensionVM.PersonalPin = null;
            IsClosed = true;
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
