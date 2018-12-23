using HIO.Core;
using HIO.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIO.Setup
{
    public abstract class TSetupPageBase : TViewModelBase
    {
        public TSetupPageBase(TWizard parent, double progressPercent)
        {
            ParentWizard = parent;
            ProgressPercent = progressPercent;
            ProgressAnimationDuration = 1500;
            Commands.AddCommand("MoveNextPage", MoveNextPage, () => CanMoveNextPage);
            Commands.AddCommand("MovePreviousPage", MovePreviousPage, () => CanMovePreviousPage);
        }

        #region Properties

        public virtual bool CanMoveNextPage
        {
            get
            {
                return NextPage != null;
            }
        }
        public virtual bool CanMovePreviousPage
        {
            get
            {
                return PreviousPage != null;
            }
        }
        public virtual bool AllowShowToolbox
        {
            get
            {
                return true;
            }
        }

        public TWizard ParentWizard { get; private set; }

        public virtual TSetupPageBase NextPage
        {
            get
            {
                return ParentWizard.Pages.ElementAtOrDefault(ParentWizard.Pages.IndexOf(this) + 1);
            }
        }
        public virtual TSetupPageBase PreviousPage
        {
            get
            {
                return ParentWizard.Pages.ElementAtOrDefault(ParentWizard.Pages.IndexOf(this) - 1);
            }
        }


        public double ProgressPercent
        {
            get
            {
                return GetValue<double>();
            }
            set
            {
                SetValue(value);
            }
        }
        public double ProgressAnimationDuration
        {
            get
            {
                return GetValue<double>();
            }
            set
            {
                SetValue(value);
            }
        }
        
          public bool BoxMsgError {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }
        public string Message
        {
            get
            {
                return ParentWizard.Message;
            }
            set
            {
                ParentWizard.Message = value;
                Commands.Update();
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        public virtual void MoveNextPage()
        {
            ParentWizard?.MoveNextPage();
        }
        public virtual void MovePreviousPage()
        {
            ParentWizard?.MovePreviousPage();
        }

        public virtual void OnShow()
        {
         //   if (ParentWizard.ActivePage.ToString() == "HIO.Setup.TNewDeviceAddingPage3")
          //      ParentWizard.BackgroundParent = "#123456";
          //  if(ParentWizard.ActivePage.)
        }
        public virtual void OnHide()
        {

        }
        #endregion

    }
}
