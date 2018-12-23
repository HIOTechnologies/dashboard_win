using HIO.Backend;

namespace HIO.Setup
{
    public class TNewDeviceAddingPage3_2 : TSetupPageBase
    {
        public TNewDeviceAddingPage3_2(TWizard parent, double progressPercent) : base(parent, progressPercent)
        {
        }



        #region Properties

        public override bool CanMoveNextPage
        {
            get
            {
                return !HIOStaticValues.tmain.IsPinRequired;
            }
        }


        #endregion

        public override void MoveNextPage()
        {
            base.MoveNextPage();
            //TODO:????
        }

        public override void OnShow()
        {
            if (CanMoveNextPage)
            {
                MoveNextPage();
                return;
            }
            base.OnShow();
        }
    }
}
