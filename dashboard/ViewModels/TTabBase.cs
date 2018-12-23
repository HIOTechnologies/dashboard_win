using HIO.Core;
using System.Linq;

namespace HIO.ViewModels
{
    public abstract class TTabBase : TViewModelBase
    {
        public TTabBase(TTabManager parent)
        {
            Parent = parent;
            IsEnabled = false;
        }
        public TTabManager Parent { get; set; }
        public abstract string Title { get; }
        public abstract string HoverImageUrl { get; }
        public abstract string NormalImageUrl { get; }

        public bool IsSelected
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                if (SetValue(value))
                {
                    if (value)
                    {
                        foreach (var item in Parent.Items.Where(t => t != this && t.IsSelected).ToArray())
                        {
                            item.IsSelected = false;
                        }
                    }

                    Parent.OnPropertyChanged(t => t.ActiveTab);
                    OnIsActiveTabChanged(value);
                }
            }
        }



        public bool IsEnabled
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

        protected virtual void OnIsActiveTabChanged(bool value)
        {

        }
    }
}
