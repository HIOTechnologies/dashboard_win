using HIO.Core;
using HIO.Backend;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using HIO.WPF;
using System.Windows.Forms;

namespace HIO.Extentions
{
    public class TExtention11 : TViewModelBase,IExtention
    {
        private bool GetPass = false;
        private Source Source;
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
        public TExtention11()
        {
         //   HIOStaticValues.AdminExtention.ShowOnly(this);
        }
        public void Initialize(List<LoginFieldS> lf, bool GETPASS, Source src)
        {
            GetPass = GETPASS;
            Source = src;
            LoadData(lf);
        }
        #region Fields

        private TExtention11View _Form;
        #endregion

        #region Properties

        public bool IsClosed { get; private set; }
        public ObservableCollection<TLinkItem> Items { get; private set; } = new ObservableCollection<TLinkItem>();

        public TLinkItem SelectedItem
        {
            get
            {
                return GetValue<TLinkItem>();
            }
            set
            {
                if (SetValue(value))
                {
                    OnSelectedItemChanged();
                  //  Commands.Update();
                }
            }
        }


        #endregion

        #region Methods

        private void LoadData(List<LoginFieldS> lf)
        {
            Items.Clear();
            if (lf != null && lf.Count > 0) {
                Converts conv = new Converts();
                foreach (LoginFieldS fields in lf ) {
                    DrawingImage tmpDraw = new DrawingImage();
                    if (fields.imageData == null || fields.imageData.Length == 0)
                        tmpDraw = HIOStaticValues.PutTextInImage(fields.url.Substring(0, 1));
                    else
                        tmpDraw = conv.BitmapImageToDrawingImage(conv.byteArrayToImage(fields.imageData));

                    Items.Add(new TLinkItem(fields.title, fields.userName, tmpDraw, Int32.Parse (fields.rowid),fields.url));
                }
            }

           
         
        }
        private void OnSelectedItemChanged()
        {
            if (SelectedItem != null)
            {
                Dictionary<string, string> dicData = new Dictionary<string, string>();
                DataBase db = new DataBase();
              
                db.Update_LastUsed_User(SelectedItem.Id.ToString());
                if (GetPass)
                {
                    byte[] rowidByteArray = BitConverter.GetBytes(SelectedItem.Id).ToArray();
                    string pass = HIOStaticValues.BaS.GetPassFromSwitch(rowidByteArray).pass;
                    dicData.Add("CMD", "PASS");
                    dicData.Add("DATA", pass);
                    HIOStaticValues.BaS.Write(dicData, Source, true);
                }
                else
                {
                    dicData.Add("CMD", "CHECKREADYUSER");
            
                    HIOStaticValues.username = SelectedItem.Description;
                    HIOStaticValues.BaS.Write(dicData, Source, false);
                }
            }
            Close();
        }
        public void Show()
        {
            IsClosed = false;
        
            _Form = new TExtention11View();
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
            if (!IsClosed)
                _Form?.Close();
            _Form = null;
        }
        #endregion

    }

}
