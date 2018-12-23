
using HIO.Backend;
using HIO.ViewModels.Accounts;
using HIO.Controls;

using HIO.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Media;
using HIO.WPF;

namespace HIO.Extentions
{
    public class TExtentionManage : TViewModelBase, IExtention
    {
        private bool GetPass = false;
        private Source Source;
        string url = "";
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

        public TExtentionManage()

        {
            // HIOStaticValues.AdminExtention.ShowOnly(this);
        }
        public void Initialize(List<LoginFieldS> listlp, bool GETPASS, Source source, string url)
        {
            GetPass = GETPASS;
            Source = source;
            this.url = url;
            LoadData(listlp);

        }
        private TExtentionManageView _Form;

        private void LoadData(List<LoginFieldS> listlp)
        {
            SourceItems.Clear();
            SourceAllItems.Clear();
            DataBase db = new DataBase();
            List<LoginFieldS> llp = db.getInfoFromDB("*", "", "");
            Converts conv = new Converts();
            if (listlp != null && listlp.Any())
                foreach (LoginFieldS lf in listlp)
                {
                    DrawingImage tmpDraw = new DrawingImage();
                    if (lf.imageData == null || lf.imageData.Length == 0)
                        tmpDraw = HIOStaticValues.PutTextInImage(lf.url.Substring(0, 1));
                    else
                        tmpDraw = conv.BitmapImageToDrawingImage(conv.byteArrayToImage(lf.imageData));

                    SourceItems.Add(new TLinkItem { Title = lf.title, Description = lf.userName, ImageData = tmpDraw, Id = Int32.Parse(lf.rowid), Url = url });
                }
            if (llp != null && llp.Any())
                foreach (LoginFieldS lf in llp)
                {
                    DrawingImage tmpDraw = new DrawingImage();
                    if (lf.imageData == null || lf.imageData.Length == 0)
                        tmpDraw = HIOStaticValues.PutTextInImage(lf.url.Substring(0, 1));
                    else
                        tmpDraw = conv.BitmapImageToDrawingImage(conv.byteArrayToImage(lf.imageData));

                    SourceAllItems.Add(new TLinkItem { Title = lf.title, Description = lf.userName, ImageData = tmpDraw, Id = Int32.Parse(lf.rowid), Url = lf.url });
                }


            OnPropertyChanged(() => Items);
        }
        public bool IsClosed { get; private set; }
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
                }
            }
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


        public ObservableCollection<TLinkItem> SourceItems { get; private set; } = new ObservableCollection<TLinkItem>();
        public ObservableCollection<TLinkItem> SourceAllItems { get; private set; } = new ObservableCollection<TLinkItem>();
        public IEnumerable<TLinkItem> Items
        {
            get
            {
                if (SearchText.IsNullOrEmpty())
                {
                    // return SourceAllItems;
                    return SourceItems;
                }
                return SourceAllItems.Where(t => (t.Title?.ContainsIgnoreCase(SearchText) ?? false) || (t.Description?.ContainsIgnoreCase(SearchText) ?? false)).ToList();
                //   return SourceAllItems.Where(t => t.Title.IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) > -1);
            }
        }


        public void Close()
        {
            if (!IsClosed)
                _Form?.Close();
            _Form = null;
        }
        private void OnSelectedItemChanged()
        {

            if (SelectedItem != null)
            {
                DataBase db = new DataBase();
                Dictionary<string, string> dicData = new Dictionary<string, string>();
                db.Update_LastUsed_User(SelectedItem.Id.ToString());

                HIOStaticValues.username = SelectedItem.Description;
                byte[] rowidByteArray = BitConverter.GetBytes(SelectedItem.Id).ToArray();
                /////////////////////////////////////////////
                if (SelectedItem.Url == url)
                {

                    if (GetPass)
                    {

                        if (HIOStaticValues.TPinStatus())
                        {
                           
                            string pass = HIOStaticValues.BaS.GetPassFromSwitch(rowidByteArray).pass;
                            dicData.Add("CMD", "PASS");
                            dicData.Add("DATA", pass);
                            HIOStaticValues.BaS.Write(dicData, Source, true);
                        }
                    }
                    else
                    {
                        dicData.Add("CMD", "CHECKREADYUSER");
                 
                        HIOStaticValues.username = SelectedItem.Description;
                        HIOStaticValues.BaS.Write(dicData, Source, false);

                    }
                }
                else
                {
                    /////////////////////////////////////////////add user from another url

                    if (GetPass)
                    {

                        if (HIOStaticValues.TPinStatus())
                        {
                           

                            string pass = HIOStaticValues.BaS.GetPassFromSwitch(rowidByteArray).pass;
                            dicData.Add("CMD", "PASS");
                            dicData.Add("DATA", pass);
                            HIOStaticValues.BaS.Write(dicData, Source, true);
                        }
                    }
                    else
                    {
                        if (!db.CheckExistUser(SelectedItem.Description, url))
                        {
                            if (HIOStaticValues.TPinStatus())
                            {

                                if (HIOStaticValues.BaS.UpdateSwitch(SelectedItem.Id.ToString(), url, "", SelectedItem.Title, SelectedItem.Description, "", 0x0) == 0)
                                {
                                    if (db.UpdateDBFromRowID(SelectedItem.Id.ToString(), url, "", SelectedItem.Title, SelectedItem.Description))
                                    {


                                        HIOStaticValues.username = SelectedItem.Description;
                                        dicData.Add("CMD", "CHECKREADYUSER");
                               
                                        HIOStaticValues.BaS.Write(dicData, Source, false);
                                    }
                                    else
                                    {
                                        HIOStaticValues.popUp("Sorry,We have a problem while update record\nPlease try again.");
                                    }
                                }

                                else
                                {
                                    HIOStaticValues.popUp("Sorry,We have a problem while update record\nPlease try again.");
                                }
                            }
                        }
                        else HIOStaticValues.popUp("This user already exists,You can modify it in the Edit Form.");
                    }
                }
                Close();

            }


        }

        public void Show()
        {
            IsClosed = false;
            _Form = new TExtentionManageView();
            _Form.DataContext = this;
            _Form.Deactivated += _Form_Deactivated;
            _Form.Closing += _Form_Closing;
            SearchText = "";
            _Form.ShowAsActive();
        }


        private void _Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClosed = true;

        }

        private void _Form_Deactivated(object sender, EventArgs e)
        {
            Close();
        }

    }

    public class TLinkItem : TViewModelBase
    {
        public TLinkItem()
        {
            Commands.AddCommand("Edit", Edit);
            Commands.AddCommand("CopyUsername", CopyUsername);
            Commands.AddCommand("CopyPassword", CopyPassword);
            Commands.AddCommand("Launch", Launch);


        }




        public TLinkItem(string title, string description, DrawingImage imageData, int id, string url) : this()
        {
            Title = title;
            Description = description;
            ImageData = imageData;
            Url = url;
            Id = id;
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
        public int Id
        {
            get
            {
                return GetValue<int>();
            }
            set
            {
                SetValue(value);
            }
        }
        public string Description
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

        public string Url
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

        public DrawingImage ImageData
        {
            get
            {
                return GetValue<DrawingImage>();
            }
            set
            {
                SetValue(value);
            }
        }


        #region Methods


      //  private int QCopyPassword()
      //  {

           
        //}
        private void CopyPassword()
        {
            Task.Run(() =>
            {
                try
                {
                    if (HIOStaticValues.TPinStatus())
                    {

                        Commands ic = new Commands();

                        byte[] rowidByteArray = BitConverter.GetBytes(Id).ToArray();
                        while (true)
                        {
                            StatusPassword sp = ic.GetPassword(rowidByteArray);
                            if (sp.statusWord != null && sp.statusWord.SequenceEqual(new byte[] { 0x69, 0x85 }))
                            {

                                var res = System.Windows.Application.Current.Dispatcher.Invoke(new Func<bool>(() =>
                                {
                                    return HIOStaticValues.AdminExtention.Extention06a.Show();
                                }));
                                if (!res) return;
                            }
                            else if (sp.statusWord.SequenceEqual(new byte[] { 0x90, 0x00 }))

                            {
                               System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                                {

                                    Clipboard.SetDataObject(sp.pass);
                                    
                                }));
                                break;
                                // Clipboard.SetText(sp.pass);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {


                }
            });

        }

        private void CopyUsername()
        {
            // Clipboard.SetText(Description);
            Clipboard.SetDataObject(Description);
        }

        public void Edit()
        {
            TAccountEditor editForm = new TAccountEditor();
            editForm.Show(new TAccountItem { Name = Title, Username = Description, UserID = Id.ToString(), Url = Url });
        }
        private void Launch()
        {
            System.Diagnostics.Process.Start("http://" + Url);


        }

        #endregion
    }
}
