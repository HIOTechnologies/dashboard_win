using HIO.Controls;
using HIO.Core;
using HIO.Backend;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using HIO.Extentions;
using System.Windows.Media;
using HIO.WPF.Services;

namespace HIO.ViewModels.Accounts
{
    public class TAccountItem : TViewModelBase
    {
        public TAccountItem()
        {

        }
        public TAccountItem(TAccountManagerViewModel parent) : this()
        {
            Parent = parent;
        }
        public TAccountItem(TAccountManagerViewModel parent, string name, DrawingImage imageData, string subTitle1, string subTitle2, string url, int popularity, string date) : this(parent)
        {
            Name = name;
            SubTitle1 = subTitle1;
            SubTitle2 = subTitle2;
            ImageData = imageData;///   TEmbeddedResource.OpenFileByteArray(imageUrl);
            Url = url;
            Date = date;
            Popularity = popularity;

        }

        #region Properties

        public TAccountManagerViewModel Parent { get; set; }

        public bool IsChecked
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                if (SetValue(value))
                {
                    Parent?.OnIsCheckedChanged(this);
                }
            }
        }


        public string Name
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
        public string UserID
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

        public int Popularity
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

        public string Date
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

        public string AppID
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

        public string SubTitle1
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

        public string SubTitle2
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

        public int Index
        {
            get
            {
                return Array.IndexOf(Parent.Items.ToArray(), this);
            }
        }

        public TAccountItem Previous
        {
            get
            {
                int preIndex = Index - 1;
                if (preIndex < 0) return null;
                return Parent.Items.FirstOrDefault(t => t.Index == preIndex);
            }
        }
        public TAccountItem Next
        {
            get
            {
                int nextIndex = Index + 1;
                if (nextIndex > Parent.Items.Count() - 1) return null;
                return Parent.Items.FirstOrDefault(t => t.Index == nextIndex);
            }
        }
        private const double Corner = 5;
        public CornerRadius CornerRadius
        {
            get
            {
                if (IsChecked)
                {
                    var pre = Previous;
                    var next = Next;

                    if ((pre?.IsChecked ?? false) && (next?.IsChecked ?? false))
                    {
                        return new CornerRadius(0);
                    }
                    else if (pre?.IsChecked ?? false)
                    {
                        return new CornerRadius(0, 0, Corner, Corner);
                    }
                    else if (next?.IsChecked ?? false)
                    {
                        return new CornerRadius(Corner, Corner, 0, 0);
                    }
                }
                return new CornerRadius(Corner);
            }
        }
        #endregion

        #region Methods


        public void CopyPassword()
        {
            Task.Run(() =>
            {
                try
                {
                    if (HIOStaticValues.TPinStatus())
                    {

                        Commands ic = new Commands();

                        
                        Converts conv = new Converts();
                        int userIdInt = Int32.Parse(UserID);
                        byte[] rowidByteArray = BitConverter.GetBytes(userIdInt);
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
                            }
                        }
                    }
                }
                catch (Exception ex)
                {


                }


            });
          
            



        }

        TAccountItem _Clone;
        public void StartEdit()
        {
            //_Clone = (TAccountItem)MemberwiseClone();
            _Clone = new TAccountItem
            {
                Name = Name,
                UserID = UserID,
                Username = Username,
                Password = Password,
                Url = Url,
                AppID = AppID
            };
            TAccountEditor Editor = new TAccountEditor();
            Editor.Show(this);
        }

        public void EndEdit()
        {
            _Clone = null;
        }

        public void CancelEdit()
        {
            if (_Clone != null)
            {
                Name = _Clone.Name;
                UserID = _Clone.UserID;
                Username = _Clone.Username;
                Password = _Clone.Password;
                Url = _Clone.Url;
                AppID = _Clone.AppID;
                _Clone = null;
            }
        }

        public async void Delete()
        {
            Commands cmd = new Backend.Commands();
            if (TMessageBox.Show("Are you sure you want to delete this account?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                while (true)
                {
                    var res = cmd.AmISync();
                    if (res == SyncronizingStateEnum.Synced)
                    {
                        break;
                    }
                    else
                        HIOStaticValues.SyncOpration();
                }

                await UIService.Execute(async () =>
                {
                    Commands ic = new Commands();
                    ic.Delete(UserID);
                    await Parent.LoadData();
                });
                Parent?.OnPropertyChanged(nameof(TAccountManagerViewModel.IsAllChecked));
            }
        }

        public void CopyUrl()
        {
            //  Clipboard.SetText(Url);
            Clipboard.SetDataObject(Url);

        }

        public void CopyUsername()
        {
            //Clipboard.SetText(Username);
            Clipboard.SetDataObject(Username);

        }

        public void Reset()
        {
            Name = null;
            Username = null;
            Password = null;
            Url = null;
            AppID = null;
            UserID = null;
            ImageData = null;
            SubTitle1 = null;
            SubTitle2 = null;
        }
        public override string ToString()
        {
            return $"{Name} - {SubTitle1} - {SubTitle2} - {Username}";
        }
        #endregion

    }
}
