using HIO.Backend;
using HIO.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace HIO.ViewModels.MagicLock
{
    public class TMagicLockManager : TTabBase
    {
        public TMagicLockManager(TTabManager parent) : base(parent)
        {
           
            Users = new ObservableCollection<TUser>();
            Commands.AddCommand("ChangePassword", ChangePassword);
           

        }

        #region Properties
        public ObservableCollection<TUser> Users { get; private set; } = new ObservableCollection<TUser>();
        private bool gettingData = false;
        public override string Title
        {
            get
            {
                return "Magic Lock";
            }
        }



        public override string HoverImageUrl
        {
            get
            {
                return "pack://application:,,,/HIO;component/Resources/Buttons/pc.png";
            }
        }

        public override string NormalImageUrl
        {
            get
            {
                return "pack://application:,,,/HIO;component/Resources/Buttons/pc2.png";
            }
        }


        public bool IsFirstTime
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

        public int Distance
        {
            get
            {
                return GetValue<int>();
            }
            set
            {
              
                SetValue(value);
               if(!gettingData)
                    SendData((byte)value);
            }
        }


        public bool UseThisDeviceToLockThePC
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                if (SetValue(value) && value)
                {
                    if (!IsFirstTime)
                    {
                        TPcLockerEditor Editor = new TPcLockerEditor(null, 0, this);
                        if (Editor.Show())
                        {
                            IsFirstTime = false;
                        }
                    }

                }
            }
        }


        public TUser SelectedUser
        {
            get
            {
                return GetValue<TUser>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region Methods
        private void SendData(byte rssi) {
            Commands cmd = new Commands();
            cmd.SetRssiTH(rssi);

        }
        protected override void OnIsActiveTabChanged(bool value)
        {
            if(value)
            Initialize();
        
        }
        private void Initialize()
        {
            //TODO: Load  IsFirstTime,Password,Distance,UseThisDeviceToLockThePC properties !
            IsFirstTime = true;
            Password = "";
            UseThisDeviceToLockThePC = true;
            LoadUsers();
            gettingData = true;
            Distance =(int) HIOStaticValues.BaS.GetRSSITHBLE();
            gettingData = false;
        }
        private void LoadUsers()
        {
            try
            {
                Users.Clear();
                ManagementObjectSearcher usersSearcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_UserAccount");
                ManagementObjectCollection users = usersSearcher.Get();

                var localUsers = users.Cast<ManagementObject>().Where(
                    u => (bool)u["LocalAccount"] == true &&
                         (bool)u["Disabled"] == false &&
                         (bool)u["Lockout"] == false &&
                         int.Parse(u["SIDType"].ToString()) == 1 &&
                         u["Name"].ToString() != "HomeGroupUser$");

                foreach (ManagementObject user in localUsers)
                {
                    Users.Add(new TUser() { Title = user["Name"].ToString() });
                   
                        
                        }




                //TODO: Please Load the selected user !
                SelectedUser = Users.FirstOrDefault();
            }
            catch (Exception e)
            {
               //add to all exception

            }
        }
        private void ChangePassword()
        {

            TPasswordEditor Editor = new TPasswordEditor();
            Editor.Show(this);
          
        }
        public void ChangePassword(string password)
        {
            Password = password;
            //TODO: Store Password for second time
        }
        public void ChangePassword(TUser selectedUser, string password)
        {
            //TODO:Change User And Password For First Time
            SelectedUser = selectedUser;
            Password = password;
        }
        #endregion
    }
    public class TUser : TViewModelBase
    {

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

    }
}
