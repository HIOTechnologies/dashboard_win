using HIO.Controls;
using HIO.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIO.Setup
{
    public abstract class TWizard : TViewModelBase
    {

        public TWizard()
        {
            Commands.AddCommand("ErrorOK", ErrorOK);
            ProgressAnimationDuration = 1500;
        }

        #region Properties
        public List<TSetupPageBase> Pages { get; set; } = new List<TSetupPageBase>();
        public TSetupPageBase ActivePage
        {
            get
            {
                return GetValue<TSetupPageBase>();
            }
            set
            {
                var old = ActivePage;
                
                if (SetValue(value))
                {
                    old?.OnHide();
                    value?.OnShow();
                    if (value != null)
                    {
                        ProgressAnimationDuration = value.ProgressAnimationDuration;
                        ProgressPercent = value.ProgressPercent;

                    }

                }
            }
        }
        public AnimationDirection Direction
        {
            get
            {
                return GetValue<AnimationDirection>();
            }
            set
            {
                SetValue(value);
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
        public string BackgroundParent
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

        public string Message
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                if (SetValue(value))
                {
                    Commands.Update();
                }
            }
        }
        #endregion

        #region Methods

        public void ErrorOK()
        {
            Message = null;
        }


        public void MoveNextPage()
        {
            Direction = AnimationDirection.RightToLeft;
            if (ActivePage == null)
            {
                ActivePage = Pages.FirstOrDefault();
            }
            else if (ActivePage.NextPage != null)
            {
                ActivePage = ActivePage.NextPage;
            }
            else
            {
                Complete();
            }
            ErrorOK();
        }

        public void MovePreviousPage()
        {
            Direction = AnimationDirection.LeftToRight;

            if (ActivePage == null)
            {
                ActivePage = Pages.FirstOrDefault();
            }
            else if (ActivePage.PreviousPage != null)
            {
                ActivePage = ActivePage.PreviousPage;
            }
            ErrorOK();
        }
        public virtual void Complete()
        {

        }
        #endregion


    }
}
