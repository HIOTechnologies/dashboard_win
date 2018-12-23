using System.Windows;
using System.Windows.Controls;

namespace HIO.Controls
{
    public class TMessageBox
    {
        public static MessageBoxResult Show(string message, string title = "Message", MessageBoxButton button = MessageBoxButton.OK)
        {
            TMessageBoxView wnd = new TMessageBoxView();
            switch (button)
            {
                case MessageBoxButton.OKCancel:
                    PrepareButton(wnd.Btn_Left, MessageBoxResult.OK, true);
                    PrepareButton(wnd.Btn_Center, MessageBoxResult.Cancel, false, true);
                    PrepareButton(wnd.Btn_Right, MessageBoxResult.None);
                    break;
                case MessageBoxButton.YesNoCancel:
                    PrepareButton(wnd.Btn_Left, MessageBoxResult.Yes, true);
                    PrepareButton(wnd.Btn_Center, MessageBoxResult.No, false);
                    PrepareButton(wnd.Btn_Right, MessageBoxResult.Cancel, false, true);

                    break;
                case MessageBoxButton.YesNo:
                    PrepareButton(wnd.Btn_Left, MessageBoxResult.Yes, true);
                    PrepareButton(wnd.Btn_Center, MessageBoxResult.No, false, true);
                    PrepareButton(wnd.Btn_Right, MessageBoxResult.None);
                    break;
                case MessageBoxButton.OK:
                default:
                    PrepareButton(wnd.Btn_Left, MessageBoxResult.OK, true);
                    PrepareButton(wnd.Btn_Center, MessageBoxResult.None);
                    PrepareButton(wnd.Btn_Right, MessageBoxResult.None);
                    break;
            }
            wnd.Txt_Message.Text = message;
            wnd.Title = title;
            //MessageBox.Show("Test");
            wnd.ShowDialog();
            return wnd.Result;
        }

        private static void PrepareButton(Button button, MessageBoxResult result, bool isDefault = false, bool isCancel = false)
        {
            button.Tag = result;
            button.IsDefault = isDefault;
            button.IsCancel = isCancel;
            if (result != MessageBoxResult.None)
            {
                button.Click += (a, b) =>
                {
                    TMessageBoxView wnd = (TMessageBoxView)Window.GetWindow(button);
                    wnd.Result = result;
                    wnd.Close();
                };
            }
            switch (result)
            {
                case MessageBoxResult.OK:
                    button.Content = "OK";
                    break;
                case MessageBoxResult.Cancel:
                    button.Content = "Cancel";
                    break;
                case MessageBoxResult.Yes:
                    button.Content = "Yes";
                    break;
                case MessageBoxResult.No:
                    button.Content = "No";
                    break;
                case MessageBoxResult.None:
                default:
                    button.Visibility = Visibility.Collapsed;
                    break;
            }
        }
    }
}
