using HIO.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HIO.Backend;
namespace HIO.ViewModels
{
    public class TTabManager : TViewModelBase
    {
        public TTabManager(TMain parent)
        {
            Parent = parent;
        }

        public TMain Parent {
            get {
                return GetValue<TMain>();
            }
            private set {
                SetValue(value);

            }
        }
        public ObservableCollection<TTabBase> Items { get; private set; } = new ObservableCollection<TTabBase>();


        public void AddItem(TTabBase tabBase)
        {
            Items.Add(tabBase);
        }

        public bool IsCollapsed
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

        public TTabBase ActiveTab
        {
            get
            {
                return Items.FirstOrDefault(t => t.IsSelected);
            }
            set
            {
                if (value != null)
                {
                    value.IsSelected = true;
                }
            }
        }

    }
}

