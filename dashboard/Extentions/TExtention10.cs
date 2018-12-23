using HIO.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HIO.Backend;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using HIO.Backend.IconURL;
using System.Drawing;
using HIO.WPF;
using HIO.ViewModels.Accounts;
using System.Windows.Forms;

namespace HIO.Extentions
{
    public class TExtention10 : TViewModelBase, IExtention
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
        LoginFieldS _lf;
        public TExtention10()
        {
          
        }
        public void Initialize(LoginFieldS lf) {

            Commands.AddCommand("NotNow", NotNow);
            Commands.AddCommand("Add", Add);
          
            DrawingImage tmpDraw = new DrawingImage();
            try
            {
                Username = lf.userName;
                Password = lf.password;
                Title = lf.title;
                _lf = lf;
                tmpDraw = HIOStaticValues.PutTextInImage(lf.url.Substring(0, 1));
                IconUrl = tmpDraw;
                Task.Run(() =>
                {
                    Favicon fv = new Favicon();
                    fv.GetFromUrlAsync("http://" + lf.url);
                    fv.GetFromUrlAsyncCompleted += Fv_GetFromUrlAsyncCompleted;
                });
               
            }
            catch(Exception ex) {
          
            }

        }

        private void Fv_GetFromUrlAsyncCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Converts conv = new Converts();
                if ((sender as Favicon).Icon != null)
                {
                    IconUrl = conv.BitmapImageToDrawingImage((sender as Favicon).Icon);


                }
            }));
        }

        private TExtention10View _Form;
        public bool IsClosed { get; private set; }

        public string Username
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

        public string Password
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
        public string Title
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
        public bool IsFormOpen
        {
            get
            {
                return _Form != null;
            }
        }
        public ImageSource  IconUrl
        {
            get
            {
                return GetValue<ImageSource>();
            }
            set
            {
                SetValue(value);

            }
        }
        public Window Form
        {
            get
            {
                return _Form;
            }

        }

        private void Add()
        {

            if (HIOStaticValues.TPinStatus())
            {

                Commands cmd = new Backend.Commands();
                cmd.Insert(_lf.url, Username, Title, Password);
                try
                {
                    HIOStaticValues.tmain?.AccountManager?.LoadData();
                    HIOStaticValues.tmain?.AccountManager?.OnPropertyChanged(nameof(TAccountManagerViewModel.IsAllChecked));
                }
                catch { /*TODO: remove try catch*/}
                Close();
            }
        }

        private void NotNow()
        {
            Close();
        }

        public void Show()
        {
            IsClosed = false;
            _Form = new TExtention10View();
            _Form.DataContext = this;
            _Form.Closing += _Form_Closing;
            _Form.Deactivated += _Form_Deactivated;
            _Form.ShowAsActive();

        }

    private void _Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClosed = true;

        }

        private void _Form_Deactivated(object sender, EventArgs e)
        {
            if (!IsClosed)
                Close();
        }

      
        public void Close()
        {
            _Form?.Close();
            _Form = null;
        }
    }

}
