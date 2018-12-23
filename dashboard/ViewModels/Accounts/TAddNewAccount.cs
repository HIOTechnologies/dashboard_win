using HIO.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HIO.Backend;
using HIO.ViewModels.Accounts;
using System.Windows;
using HIO.WPF.Services;

namespace HIO.ViewModels
{
    public class TAddNewAccount : TViewModelBase
    {
        public TAddNewAccount(TAccountManagerViewModel parent)
        {
            titleRequiredImage = Visibility.Hidden;
            Parent = parent;
            Commands.AddCommand("Close", Close);
            Commands.AddCommand("Save", Save);
            Commands.AddCommand("ShowHide", ShowHide);
           
        }



        public TAccountManagerViewModel Parent { get; private set; }

        public bool IsVisible
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
        public Visibility titleRequiredImage
        {
            get
            {
                return GetValue<Visibility>();
            }
            set
            {
                SetValue(value);
            }
        }
        public TAccountItem AccountItem
        {
            get
            {
                return GetValue<TAccountItem>();
            }
            set
            {
                SetValue(value);
            }
        }

        private void ShowHide()
        {
            if (IsVisible)
            {
                Close();
            }
            else
            {
                Show();
            }
        }
        private void AddNewAccount(object obj)
        {
            this.Show();
        }

        private void Save(object obj)
        {
            if (AccountItem.Name==null || AccountItem.Name.TrimStart() == "")
            {
               
             //   _form.titleRequiredImage.Visibility =Visibility.Visible;

             //   AccountItem.Name = " ";
                return;
            }
            if (AccountItem.Url == null ||  AccountItem.Url.TrimStart() == "")
            {
              //  _form. urlRequiredImage.Visibility = Visibility.Visible;

           //     AccountItem.Url = " ";
                return;
            }
            if (AccountItem.Password == null ||  AccountItem.Password.TrimStart() == "")
            {
               // _form. passRequiredImage.Visibility = Visibility.Visible;

                return;
            }
       
            HIOStaticValues.CheckingData(AccountItem);
       
            HIOStaticValues.commandQ.Add(async () =>
           {
               await UIService.Execute(async () =>
               {
                   Commands ic = new Commands();
                   if (ic.Insert(AccountItem.Url, AccountItem.Username, AccountItem.Name, AccountItem.Password) == 1)
                   {
                       Parent.LoadData();
                       Close();
                   }
               });
           });
            
          
           
        }

        private void Close()
        {
            IsVisible = false;
        }

        public void Show()
        {

            AccountItem = new TAccountItem();
            IsVisible = true;
        }


    }
}
