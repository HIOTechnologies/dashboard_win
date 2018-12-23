using HIO.Backend;
using HIO.Controls;
using System.Windows;
using System.Windows.Forms;

namespace HIO.WPF
{
    public static class DialogSizeSetter
    {
        public static void SetDialogStandardSize(this TWindow control)
        {
            
             
           

            if (SystemParameters.PrimaryScreenWidth < 1400)
            {
                control.Width = 635;
                control.Height = 420;
            }
            else
            {
                control.Width = 850;
                control.Height = 565;

            }
        }

        public static void ShowAsActive(this TWindow window)
        {
            window.ShowActivated = true;
            window.Topmost = true;
           
            window.Show();
            Screen s = Screen.FromPoint(Cursor.Position);
            window.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            window.Left = s.WorkingArea.Right / HIOStaticValues.scale - window.Width - 16;
            window.Top = (s.WorkingArea.Top / HIOStaticValues.scale) + 60;
            HIOStaticValues.scale = PresentationSource.FromVisual(window).CompositionTarget.TransformToDevice.M11;
            window.Activate();
            window.Focus();
        }
    }
}
