using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace AutoComplete
{
    public partial class AutoCompleteTextBox : TextBox
    {
        #region Variable

        private const string PART_POPUP = "PART_Popup";
        private const string PART_LISTBOX = "PART_ListBox";

        private Popup _popup;
        private ListBox _listBox;

        private int _startIndex;

        #endregion

        #region Dependency Property

        #region ItemsSource

        private static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource",
                typeof(List<object>),
                typeof(AutoCompleteTextBox));

        public List<object> ItemsSource
        {
            get { return (List<object>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        #endregion

        #region ItemPath

        private static readonly DependencyProperty ItemPathProperty =
            DependencyProperty.Register("ItemPath",
                typeof(string),
                typeof(AutoCompleteTextBox));

        public string ItemPath
        {
            get { return (string)GetValue(ItemPathProperty); }
            set { SetValue(ItemPathProperty, value); }
        }

        #endregion

        #region StringFormat

        private static readonly DependencyProperty StringFormatProperty =
            DependencyProperty.Register("StringFormat",
                typeof(string),
                typeof(AutoCompleteTextBox));

        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }

        #endregion

        #region ItemTemplate

        private static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate",
                typeof(DataTemplate),
                typeof(AutoCompleteTextBox),
                new PropertyMetadata(ChangedItemTemplateProperty));

        private static void ChangedItemTemplateProperty(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var textbox = obj as AutoCompleteTextBox;

            if (textbox._listBox != null)
            {
                textbox._listBox.ItemTemplate = e.NewValue as DataTemplate;
            }
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        #endregion

        #region PopupVerticalOffset

        private static readonly DependencyProperty PopupVerticalOffsetProperty =
            DependencyProperty.Register("PopupVerticalOffset",
                typeof(double),
                typeof(AutoCompleteTextBox),
                new PropertyMetadata(2d));

        public double PopupVerticalOffset
        {
            get { return (double)GetValue(PopupVerticalOffsetProperty); }
            set { SetValue(PopupVerticalOffsetProperty, value); }
        }

        #endregion

        #endregion

        #region Constructor

        public AutoCompleteTextBox()
        {
            InitializeComponent();

            _startIndex = -1;

            LostFocus += (s, e) => _popup.IsOpen = false;
        }

        #endregion

        #region Public Method

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _popup = GetTemplateChild(PART_POPUP) as Popup;

            if (_popup == null)
            {
                throw new NotImplementedException("PART_Popup is not found.");
            }

            _listBox = GetTemplateChild(PART_LISTBOX) as ListBox;

            if (_listBox == null)
            {
                throw new NotImplementedException("PART_ListBox is not found.");
            }

            if (ItemTemplate != null)
            {
                _listBox.ItemTemplate = ItemTemplate;
            }

            _listBox.MouseDoubleClick += (s, e) => SelectItem();
        }

        #endregion

        #region Protected Method

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);

            if (_startIndex == -1)
            {
                _startIndex = SelectionStart;
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (ItemsSource == null || ItemsSource.Count == 0||
                _startIndex == -1)
            {
                return;
            }

            if(_startIndex > SelectionStart)
            {
                _startIndex = SelectionStart;
                return;
            }

            var text = Text.Substring(_startIndex, SelectionStart - _startIndex).Replace("\r\n", "");

            Suggestion(text);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Space)
            {
                _popup.IsOpen = false;
                _startIndex = -1;
            }
            else if (e.Key == Key.Back)
            {
                if (SelectionStart > 0)
                {
                    var ch = Text[SelectionStart - 1];

                    if (ch == ' ')
                    {
                        _startIndex = -1;
                    }
                }
            }
            else
            {
                if (_popup.IsOpen)
                {
                    if (e.Key == Key.Down)
                    {
                        _listBox.SelectedIndex = (_listBox.SelectedIndex + 1) % _listBox.Items.Count;
                    }
                    else if (e.Key == Key.Up)
                    {
                        _listBox.SelectedIndex = _listBox.SelectedIndex - 1 < 0 ? _listBox.Items.Count - 1 : _listBox.SelectedIndex - 1;
                    }
                    else if (e.Key == Key.Return || e.Key == Key.Tab)
                    {
                        SelectItem();

                        e.Handled = true;
                    }
                    else if (e.Key == Key.Escape)
                    {
                        _popup.IsOpen = false;
                        _listBox.ItemsSource = null;
                    }
                }
                else
                {
                    if (e.Key == Key.Return)
                    {
                        _startIndex = -1;
                    }
                }
            }
        }

        #endregion

        #region Private Method

        private void Suggestion(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                _popup.IsOpen = false;
                _listBox.ItemsSource = null;
            }
            else
            {
                var suggestions = ItemsSource.Where(s => GetItemText(s).StartsWith(text)).ToList();

                if (suggestions.Count > 0)
                {
                    _listBox.ItemsSource = suggestions;

                    if (!_popup.IsOpen)
                    {
                        _listBox.SelectedIndex = 0;

                        var rect = GetRectFromCharacterIndex(_startIndex);

                        _popup.HorizontalOffset = rect.X;
                        _popup.VerticalOffset = -ActualHeight + rect.Height * (GetLineIndexFromCharacterIndex(SelectionStart) + 1) + PopupVerticalOffset;
                        _popup.IsOpen = true;
                    }
                }
                else
                {
                    _popup.IsOpen = false;
                }
            }
        }

        private void SelectItem()
        {
            if (_listBox.SelectedItem == null)
            {
                return;
            }

            var text = GetItemText(_listBox.SelectedItem);

            if (!string.IsNullOrWhiteSpace(StringFormat))
            {
                text = string.Format(StringFormat, text);
            }

            Select(_startIndex, SelectionStart - _startIndex);
            SelectedText = text;
            SelectionStart = _startIndex + SelectedText.Length;

            _startIndex = -1;
            _popup.IsOpen = false;
        }

        private string GetItemText(object item)
        {
            if (string.IsNullOrWhiteSpace(ItemPath))
            {
                return item.ToString();
            }
            else
            {
                var property =item.GetType().GetProperty(ItemPath);

                if(property != null)
                {
                    return property.GetValue(item).ToString();
                }

                return null;
            }
        }

        #endregion
    }
}