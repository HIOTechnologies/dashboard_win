using HIO.Backend;
using HIO.Controls;
using HIO.Core;
using HIO.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace HIO.Extentions
{
    public class TExtention08 : TViewModelBase,IExtention
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
        public TExtention08()
        {
          
        }

  

        public void Initialize(List<LoginFieldS> lf) {

            LoadData(lf);
        }
        private TExtention08View _Form;
        private void LoadData(List<LoginFieldS> lf)
        {
            SourceItems.Clear();


            if (lf != null && lf.Count > 0)
            {
                Converts conv = new Converts();
                foreach (LoginFieldS fields in lf)
                {
                    DrawingImage tmpDraw = new DrawingImage();
                    if (fields.imageData == null || fields.imageData.Length == 0)
                        tmpDraw = HIOStaticValues.PutTextInImage(fields.url.Substring(0, 1));
                    else
                        tmpDraw = conv.BitmapImageToDrawingImage(conv.byteArrayToImage(fields.imageData));

                    SourceItems.Add(new TLinkItem(fields.title, fields.userName, tmpDraw, Int32.Parse(fields.rowid), fields.url));
                }
            }
            OnPropertyChanged(() => Items);
        }

        public string SearchText
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                if (SetValue(value))
                {
                    OnPropertyChanged(() => Items);
                }
            }
        }
        public bool IsClosed { get; private set; }
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
                    Commands.Update();
                }
            }
        }

        public ObservableCollection<TLinkItem> SourceItems { get; private set; } = new ObservableCollection<TLinkItem>();
        public IEnumerable<TLinkItem> Items
        {
            get
            {
                if (SearchText.IsNullOrEmpty())
                {
                    return SourceItems;
                }
              //  return SourceItems.Where(t => t.Title.IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) > -1);
                return SourceItems.Where(t => (t.Title?.ContainsIgnoreCase(SearchText) ?? false) || (t.Description?.ContainsIgnoreCase(SearchText) ?? false)).ToList();

            }
        }


        public void Show()
        {
            IsClosed = false;
            _Form = new TExtention08View();
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
    }
    
}
