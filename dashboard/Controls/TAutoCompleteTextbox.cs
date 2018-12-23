using HIO.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HIO.Controls
{
    public class TAutoCompleteTextbox : TextBox
    {
        static TAutoCompleteTextbox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TAutoCompleteTextbox), new FrameworkPropertyMetadata(typeof(TAutoCompleteTextbox)));
        }
        #region Fields
        private ListBox PART_Listbox;
        private FrameworkElement _PlaceHolder;
        private TListManager _ListManager;
        #endregion



        #region Properties


        #region Placeholder

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(TAutoCompleteTextbox), new PropertyMetadata(null));
        #endregion

        #region ItemsSource

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(TAutoCompleteTextbox), new PropertyMetadata(null));
        #endregion

        #region DisplayMemberPath


        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(TAutoCompleteTextbox), new PropertyMetadata(null));
        #endregion

        #region SelectedValuePath

        public string SelectedValuePath
        {
            get { return (string)GetValue(SelectedValuePathProperty); }
            set { SetValue(SelectedValuePathProperty, value); }
        }

        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(TAutoCompleteTextbox), new PropertyMetadata(null));

        #endregion

        #region ItemTemplate

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(TAutoCompleteTextbox), new PropertyMetadata(null));
        #endregion

        #region IsDropDownOpen


        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(TAutoCompleteTextbox), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsDropDownOpenChanged));

        private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)

        {

        }


        #endregion

        #region MaxDropDownHeight         
        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }

        public static readonly DependencyProperty MaxDropDownHeightProperty =
            DependencyProperty.Register("MaxDropDownHeight", typeof(double), typeof(TAutoCompleteTextbox), new PropertyMetadata(230.0));

        #endregion

        #region DropDownBackground         
        public Brush DropDownBackground
        {
            get { return (Brush)GetValue(DropDownBackgroundProperty); }
            set { SetValue(DropDownBackgroundProperty, value); }
        }

        public static readonly DependencyProperty DropDownBackgroundProperty =
            DependencyProperty.Register("DropDownBackground", typeof(Brush), typeof(TAutoCompleteTextbox), new PropertyMetadata(Brushes.White));

        #endregion


        #region SelectedItem

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }


        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(TAutoCompleteTextbox), new PropertyMetadata(null));
        #endregion


        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _PlaceHolder = (FrameworkElement)GetTemplateChild("PART_PlaceHolder");
            PART_Listbox = (ListBox)GetTemplateChild("PART_Listbox");

            _ListManager = new TListManager(this);
            PART_Listbox.DataContext = _ListManager;
            Dispatcher.BeginInvoke(new Action(UpdatePlaceholderPosition), System.Windows.Threading.DispatcherPriority.Loaded);
        }
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            UpdatePlaceholderPosition();
        }



        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            UpdatePlaceholderPosition();
        }

        private void UpdatePlaceholderPosition()
        {
            if (_PlaceHolder == null || Placeholder.IsNullOrEmpty()) return;

            if (!Text.IsNullOrEmpty() || IsKeyboardFocusWithin)
            {
                ThicknessAnimation thicknessAnimation = new ThicknessAnimation();
                thicknessAnimation.To = new Thickness(0, -_PlaceHolder.ActualHeight - 5, 0, 0);
                thicknessAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(500));
                _PlaceHolder.BeginAnimation(MarginProperty, thicknessAnimation);

                //_PlaceHolder.BeginAnimation(RenderTransformProperty,)
                ScaleTransform scaleTransform = new ScaleTransform(0.9, 0.9);
                _PlaceHolder.RenderTransform = scaleTransform;

            }
            else
            {
                ThicknessAnimation thicknessAnimation = new ThicknessAnimation();
                thicknessAnimation.To = new Thickness(5, 0, 0, 0);
                thicknessAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(500));
                _PlaceHolder.BeginAnimation(MarginProperty, thicknessAnimation);
                _PlaceHolder.RenderTransform = null;
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            UpdatePlaceholderPosition();
        }

        class TListManager : TViewModelBase
        {
            public TListManager(TAutoCompleteTextbox autoCompleteTextbox)
            {
                Parent = autoCompleteTextbox;
                Parent.PreviewKeyUp += Parent_PreviewKeyUp;
                Parent.PART_Listbox.PreviewKeyDown += PART_Listbox_PreviewKeyDown;
                Parent.PART_Listbox.MouseDoubleClick += PART_Listbox_MouseDoubleClick;
                Parent.PART_Listbox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("Items") { Source = this });
                Parent.PART_Listbox.SetBinding(Selector.SelectedItemProperty, new Binding("SelectedItem") { Source = this, Mode = BindingMode.TwoWay });
            }

            #region Fields
            private KeyConverter conv = new KeyConverter();
            #endregion


            #region Properties

            public TAutoCompleteTextbox Parent { get; private set; }
            public ObservableCollection<string> Items { get; private set; } = new ObservableCollection<string>();

            public bool IsDropDownOpen
            {
                get
                {
                    return Parent.IsDropDownOpen;
                }
                set
                {
                    Parent.IsDropDownOpen = value;
                }
            }
            public string Text
            {
                get
                {
                    return Parent.Text;
                }
                set
                {
                    Parent.Text = value;
                }
            }
            public bool CanShowSuggesttion
            {
                get
                {
                    if (Text.IsNullOrEmpty()) return false;
                    if (!(Items?.Any() ?? false)) return false;
                    if (Items.Count == 1)
                    {
                        if (Items.First().Equals(Text, StringComparison.InvariantCultureIgnoreCase)) return false;
                    }
                    return true;
                }
            }

            public string SelectedItem
            {
                get
                {
                    return GetValue<string>();
                }
                set
                {
                    if (SetValue(value) && value != null)
                    {
                        SelectFromSelectedItem();
                        if (!Parent.PART_Listbox.IsKeyboardFocusWithin)
                        {
                            Parent.PART_Listbox.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                Keyboard.Focus(Parent.PART_Listbox);
                                Parent.PART_Listbox.Focus();
                                Parent.PART_Listbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

                                //Trace.WriteLine("PART_Listbox.IsFocused 2=> " + Parent.PART_Listbox.IsKeyboardFocusWithin.ToString());

                            }), System.Windows.Threading.DispatcherPriority.Loaded);
                        }
                    }
                }
            }
            #endregion

            #region Methods
            private void Parent_PreviewKeyUp(object sender, KeyEventArgs e)
            {

                if (Parent.PART_Listbox.IsKeyboardFocusWithin) return;
                //Trace.WriteLine("Key => " + e.Key + " = " + DateTime.Now.ToString());
                if (e.Key == Key.Down)
                {
                    Suggest();
                    if (IsDropDownOpen)
                    {
                        if (SelectedItem == null) SelectedItem = Items?.FirstOrDefault();

                    }
                }
                else if (e.Key == Key.Back || e.Key == Key.Delete)
                {
                    UpdateItems();
                    Suggest();
                }
                else if (IsTextChangingKey(e.Key))
                {
                    UpdateItems();
                    Suggest();
                }
                else// if (e.Key == Key.Escape || IsDropDownOpen)
                {
                    Cancel();
                }
                //else if (1==1)
                //{
                //    Cancel();
                //}
                e.Handled = true;
            }
            private void PART_Listbox_PreviewKeyDown(object sender, KeyEventArgs e)
            {
                //Trace.WriteLine("Listbox_PreviewKeyDown => " + e.Key.ToString());

                if (e.Key == Key.Enter)
                {
                    Accept(true);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    Cancel();
                    e.Handled = true;
                }
                else if (e.Key == Key.Up && Items?.FirstOrDefault() == SelectedItem)
                {
                    Parent.Focus();
                    e.Handled = true;
                }
            }
            private void PART_Listbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            {
                Accept();
            }


            private bool IsTextChangingKey(Key key)
            {
                if (key >= Key.NumPad0 && key <= Key.NumPad9) return true;
                string keyString = (string)conv.ConvertTo(key, typeof(string));
                return keyString.Length == 1;
            }

            private void Suggest()
            {
                //Trace.WriteLine("Suggest => " + DateTime.Now.ToString());
                if (CanShowSuggesttion)
                {
                    //UpdateItems()
                    if (!IsDropDownOpen) IsDropDownOpen = true;

                }
                else if (IsDropDownOpen)
                {
                    Cancel();
                }
            }
            public void UpdateItems()
            {
                Items.Clear();
                if (Text.IsNullOrEmpty()) return;
                var all = Parent?.ItemsSource?.OfType<object>();
                if (all == null || !all.Any()) return;
                var TT = all.Select(t => GetStringValue(t))
                    .Distinct(StringComparer.InvariantCultureIgnoreCase)
                    .Where(t => !t.IsNullOrEmpty() && t.ContainsIgnoreCase(Text))
                    .OrderBy(t => t)
                    .Take(5).ToArray();
                foreach (var item in TT)
                {
                    Items.Add(item);
                }
                //foreach (var item in all.Where(t => GetStringValue(t).ContainsIgnoreCase(Text)).Take(5))
                //{
                //    Items.Add(item);
                //}
            }

            private void Accept(bool movesFocus = false)
            {
                SelectFromSelectedItem();
                SelectedItem = null;
                IsDropDownOpen = false;
                Parent.Focus();
                if (movesFocus)
                    Parent.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
            private void Cancel()
            {
                IsDropDownOpen = false;
                SelectedItem = null;
            }
            public string GetStringValue(object item)
            {
                return item?.GetType()?.GetProperty(Parent.DisplayMemberPath)?.GetValue(item, null)?.ToString();
            }
            private void SelectFromSelectedItem()
            {
                if (SelectedItem != null)
                {
                    Text = SelectedItem;
                    Parent.SelectionStart = Text.Length;
                }
            }

            #endregion
        }

    }
}
//private void Parent_PreviewKeyDown(object sender, KeyEventArgs e)
//{
//    //if (SelectedItem != null && e.Key == Key.Tab)
//    //{
//    //    Accept();

//    //    e.Handled = true;
//    //}
//}