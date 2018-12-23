using System;
using System.Windows;
using System.Windows.Input;

namespace HIO.Controls
{
    public class TEditableLabel : TTextBox
    {
        public TEditableLabel()
        {
            GotoReadOnlyState();
        }
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            GotoEditState();
            e.Handled = true;
        }
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            EndEdit();
        }

        private void GotoReadOnlyState()
        {
            if (CanEdit())
                Cursor = Cursors.Hand;
            else
                Cursor = Cursors.Arrow;

            IsReadOnly = true;
            BorderThickness = new Thickness(0);
            Keyboard.ClearFocus();
        }

        private bool CanEdit()
        {
            return EditCommand != null && EditCommand.CanExecute(Text);
        }

        private string _OldText;
        private void GotoEditState()
        {
            if (!CanEdit())
                return;
            Cursor = null;
            _OldText = Text;
            IsReadOnly = false;
            Focus();
            SelectAll();
            BorderThickness = new Thickness(0, 0, 0, 1);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.Key)
            {
                case Key.F2:
                    GotoEditState();
                    break;
                case Key.Cancel:
                case Key.Escape:
                    CancelEdit();
                    break;
                case Key.Enter:
                    EndEdit();
                    break;
            }
        }

        private void CancelEdit()
        {
            Text = _OldText;
            GotoReadOnlyState();
        }

        private void EndEdit()
        {
            if (Text.IsNullOrWhiteSpace() || _OldText == Text)
            {
                CancelEdit();
                return;
            }
            if (EditCommand != null)
                EditCommand.Execute(Text);
            GotoReadOnlyState();
        }


        public ICommand EditCommand
        {
            get { return (ICommand)GetValue(EditCommandProperty); }
            set { SetValue(EditCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EditCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register("EditCommand", typeof(ICommand), typeof(TEditableLabel), new PropertyMetadata(null));
    }
}
