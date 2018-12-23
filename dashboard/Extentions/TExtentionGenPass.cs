using HIO.Core;
using HIO.Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using HIO.WPF;
using System.Windows.Forms;

namespace HIO.Extentions
{
    public class TExtentionGenPass : TViewModelBase, IExtention
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
        RandomPassword rp = new RandomPassword();
        Source _source;
        public TExtentionGenPass()
        {
            // HIOStaticValues.AdminExtention.ShowOnly(this);
            ShowHIO = true;
            Commands.AddCommand("Refresh", Refresh);
            Commands.AddCommand("AutoFill", AutoFill);
            Commands.AddCommand("Copy", Copy);
            Text = rp.Generate();
        }
        public void Initialize( Source source)
        {
            _source = source;
        }
        private TExtentionGenPassView _Form;


        public bool ShowHIO
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }
        public bool IsClosed { get; private set; }

        public string Text
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
                    
            }
        }

        public bool Lowercase
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
                Text = rp.Generate(16, Upercase, Lowercase, Symbols, Numbers);
            }
        }
        public bool Upercase
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
                Text = rp.Generate(16, Upercase, Lowercase, Symbols, Numbers);
            }
        }
        public bool Numbers
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
                Text = rp.Generate(16, Upercase, Lowercase, Symbols, Numbers);
            }
        }
        public bool Symbols
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
                Text = rp.Generate(16, Upercase, Lowercase, Symbols, Numbers);
            }
        }



        private void Refresh()
        {

            Text = rp.Generate(16, Upercase, Lowercase, Symbols, Numbers);
        }
        private void AutoFill()
        {
            Dictionary<string, string> dicData = new Dictionary<string, string> { { "CMD", "PASSVALUEGEN" }, { "DATA", Text } };

            HIOStaticValues.BaS.Write(dicData, _source, true);
            Close();
        }

        private void Copy()
        {
            System.Windows.Clipboard.SetDataObject(Text); //fix win
            // Clipboard.SetText(Text);
            Close();
        }
        public void Show(bool AUTOFILL,bool longClick)
        {

            try
            {
                IsClosed = false;
                Symbols = true;
                Numbers = true;
                int topSize = 50;
                Upercase = true;
                Lowercase = true;
                _Form = new TExtentionGenPassView();
                Text = rp.Generate();
                _Form.DataContext = this;
                _Form.Deactivated += _Form_Deactivated;
                _Form.Closing += _Form_Closing;

                if (!longClick)
                {
                    ShowHIO = false;
                   
                    var  s = System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position);
                    _Form.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                    _Form.Left = s.WorkingArea.Right / HIOStaticValues.scale - _Form.Width - 16;
                    _Form.Top = (s.WorkingArea.Top / HIOStaticValues.scale) + topSize;
         



                }
                else
                {
                    _Form.Width = 340;
                    _Form.Height = 300;


                }
                _Form.ShowActivated = true;
                _Form.Topmost = true;

                _Form.Show();
                _Form.Activate();
                _Form.Focus();
            }
            catch (Exception ex) {
              
            }



        }

        private void _Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClosed = true;

        }

        private void _Form_Deactivated(object sender, EventArgs e)
        {
            if (!IsClosed)
                _Form?.Close();
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
            if (!IsClosed)
                _Form?.Close();
            _Form = null;
        }
    }
}
