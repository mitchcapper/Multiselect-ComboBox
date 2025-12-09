using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Sdl.MultiSelectComboBox.API;
using Sdl.MultiSelectComboBox.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sdl.MultiSelectComboBox.WinUI.Controls
{
    [TemplatePart(Name = PART_MultiSelectComboBox, Type = typeof(Grid))]
    [TemplatePart(Name = PART_MultiSelectComboBox_Dropdown, Type = typeof(Popup))]
    [TemplatePart(Name = PART_MultiSelectComboBox_Dropdown_ListBox, Type = typeof(ListBox))]
    [TemplatePart(Name = PART_MultiSelectComboBox_Dropdown_Button, Type = typeof(Button))]
    [TemplatePart(Name = PART_MultiSelectComboBox_SelectedItemsPanel_ItemsControl, Type = typeof(ItemsControl))]
    [TemplatePart(Name = PART_MultiSelectComboBox_SelectedItemsPanel_Filter_TextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PART_MultiSelectComboBox_SelectedItemsPanel_Filter_AutoComplete_TextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PART_MultiSelectComboBox_SelectedItemsPanel_RemoveItem_Button, Type = typeof(Button))]
    public class MultiSelectComboBox : Control, IMultiSelectComboBox, IDisposable
    {
        private const string PART_MultiSelectComboBox = "PART_MultiSelectComboBox";
        private const string PART_MultiSelectComboBox_Dropdown = "PART_MultiSelectComboBox_Dropdown";
        private const string PART_MultiSelectComboBox_Dropdown_ListBox = "PART_MultiSelectComboBox_Dropdown_ListBox";
        private const string PART_MultiSelectComboBox_Dropdown_Button = "PART_MultiSelectComboBox_Dropdown_Button";
        private const string PART_MultiSelectComboBox_SelectedItemsPanel_ItemsControl = "PART_MultiSelectComboBox_SelectedItemsPanel_ItemsControl";
        private const string PART_MultiSelectComboBox_SelectedItemsPanel_Filter_TextBox = "PART_MultiSelectComboBox_SelectedItemsPanel_Filter_TextBox";
        private const string PART_MultiSelectComboBox_SelectedItemsPanel_Filter_AutoComplete_TextBox = "PART_MultiSelectComboBox_SelectedItemsPanel_Filter_AutoComplete_TextBox";
        private const string PART_MultiSelectComboBox_SelectedItemsPanel_RemoveItem_Button = "PART_MultiSelectComboBox_SelectedItemsPanel_RemoveItem_Button";
        private const string PART_MultiSelectComboBox_Dropdown_NewItem_CreatedOkButton = "PART_MultiSelectComboBox_Dropdown_NewItem_CreatedOkButton";
        private const string PART_MultiSelectComboBox_Dropdown_SelectAllButton = "PART_MultiSelectComboBox_Dropdown_SelectAllButton";
        private const string PART_MultiSelectComboBox_Dropdown_ClearAllButton = "PART_MultiSelectComboBox_Dropdown_ClearAllButton";
        private const string PART_MultiSelectComboBox_Dropdown_NewItem_TextBox = "PART_MultiSelectComboBox_Dropdown_NewItem_TextBox";

        // UI Parts
        private Grid _mainGrid;
        private Popup _dropdown;
        private ListBox _listBox;
        private Button _dropdownButton;
        private ItemsControl _selectedItemsControl;
        private TextBox _filterTextBox;
        private TextBox _autoCompleteTextBox;
        private Button _removeItemButton;

        private ObservableCollection<object> _selectedItems;
        private CancellationTokenSource _suggestionProviderCancellationTokenSource;
        private bool _isDisposed;

        public MultiSelectComboBox()
        {
            this.DefaultStyleKey = typeof(MultiSelectComboBox);

            Loaded += MultiSelectComboBox_Loaded;
            Unloaded += MultiSelectComboBox_Unloaded;

            _selectedItems = new ObservableCollection<object>();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _mainGrid = GetTemplateChild(PART_MultiSelectComboBox) as Grid;
            _dropdown = GetTemplateChild(PART_MultiSelectComboBox_Dropdown) as Popup;
            _listBox = GetTemplateChild(PART_MultiSelectComboBox_Dropdown_ListBox) as ListBox;
            _dropdownButton = GetTemplateChild(PART_MultiSelectComboBox_Dropdown_Button) as Button;
            _selectedItemsControl = GetTemplateChild(PART_MultiSelectComboBox_SelectedItemsPanel_ItemsControl) as ItemsControl;
            _filterTextBox = GetTemplateChild(PART_MultiSelectComboBox_SelectedItemsPanel_Filter_TextBox) as TextBox;
            _autoCompleteTextBox = GetTemplateChild(PART_MultiSelectComboBox_SelectedItemsPanel_Filter_AutoComplete_TextBox) as TextBox;
            _removeItemButton = GetTemplateChild(PART_MultiSelectComboBox_SelectedItemsPanel_RemoveItem_Button) as Button;

            // Set up event handlers
            if (_dropdownButton != null)
                _dropdownButton.Click += DropdownButton_Click;

            if (_filterTextBox != null)
            {
                _filterTextBox.TextChanged += FilterTextBox_TextChanged;
                _filterTextBox.GotFocus += FilterTextBox_GotFocus;
            }

            if (_listBox != null)
            {
                _listBox.SelectionChanged += ListBox_SelectionChanged;
            }

            if (_removeItemButton != null)
            {
                _removeItemButton.Click += RemoveItemButton_Click;
            }

            // Set ItemsSource for the selected items control
            if (_selectedItemsControl != null)
            {
                _selectedItemsControl.ItemsSource = _selectedItems;
            }
        }

        private void MultiSelectComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeSelectedItemsNotifyingCollection();
        }

        private void MultiSelectComboBox_Unloaded(object sender, RoutedEventArgs e)
        {
            CleanUpSelectedItemsNotifyingCollection();
        }

        private void InitializeSelectedItemsNotifyingCollection()
        {
            if (_selectedItems is INotifyCollectionChanged notifying)
            {
                notifying.CollectionChanged += SelectedItems_CollectionChanged;
            }
        }

        private void CleanUpSelectedItemsNotifyingCollection()
        {
            if (_selectedItems is INotifyCollectionChanged notifying)
            {
                notifying.CollectionChanged -= SelectedItems_CollectionChanged;
            }
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Implement selection change logic
        }

        private void DropdownButton_Click(object sender, RoutedEventArgs e)
        {
            IsDropDownOpen = !IsDropDownOpen;
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Update the filter based on text
            UpdateFilter();
        }

        private void FilterTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Open the dropdown when the filter gets focus
            IsDropDownOpen = true;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle list box selection changes
            foreach (var item in e.AddedItems)
            {
                if (!_selectedItems.Contains(item))
                {
                    _selectedItems.Add(item);
                }
            }

            foreach (var item in e.RemovedItems)
            {
                if (_selectedItems.Contains(item))
                {
                    _selectedItems.Remove(item);
                }
            }
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedItems.Clear();
        }

        #region IMultiSelectComboBox Implementation

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(MultiSelectComboBox),
                new PropertyMetadata(null, OnItemsSourcePropertyChanged));

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiSelectComboBox control)
            {
                control.OnItemsSourceChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
            }
        }

        protected virtual void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (_listBox != null)
            {
                _listBox.ItemsSource = newValue;
                UpdateFilter();
            }
        }

        public IEnumerable SelectedItems => _selectedItems;

        public string FilterText
        {
            get => (string)GetValue(FilterTextProperty);
            set => SetValue(FilterTextProperty, value);
        }

        public static readonly DependencyProperty FilterTextProperty =
            DependencyProperty.Register(nameof(FilterText), typeof(string), typeof(MultiSelectComboBox),
                new PropertyMetadata(string.Empty, OnFilterTextPropertyChanged));

        private static void OnFilterTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiSelectComboBox control && control._filterTextBox != null)
            {
                control._filterTextBox.Text = e.NewValue as string;
                control.UpdateFilter();
            }
        }

        public string DisplayMemberPath
        {
            get => (string)GetValue(DisplayMemberPathProperty);
            set => SetValue(DisplayMemberPathProperty, value);
        }

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(MultiSelectComboBox),
                new PropertyMetadata(null));

        public IFilterService FilterService
        {
            get => (IFilterService)GetValue(FilterServiceProperty);
            set => SetValue(FilterServiceProperty, value);
        }

        public static readonly DependencyProperty FilterServiceProperty =
            DependencyProperty.Register(nameof(FilterService), typeof(IFilterService), typeof(MultiSelectComboBox),
                new PropertyMetadata(null));

        public ISuggestionProvider SuggestionProvider
        {
            get => (ISuggestionProvider)GetValue(SuggestionProviderProperty);
            set => SetValue(SuggestionProviderProperty, value);
        }

        public static readonly DependencyProperty SuggestionProviderProperty =
            DependencyProperty.Register(nameof(SuggestionProvider), typeof(ISuggestionProvider), typeof(MultiSelectComboBox),
                new PropertyMetadata(null));

        public IAutoCompleteService AutoCompleteService
        {
            get => (IAutoCompleteService)GetValue(AutoCompleteServiceProperty);
            set => SetValue(AutoCompleteServiceProperty, value);
        }

        public static readonly DependencyProperty AutoCompleteServiceProperty =
            DependencyProperty.Register(nameof(AutoCompleteService), typeof(IAutoCompleteService), typeof(MultiSelectComboBox),
                new PropertyMetadata(DefaultAutoCompleteService.Instance));

        public bool IsDropDownOpen
        {
            get => (bool)GetValue(IsDropDownOpenProperty);
            set => SetValue(IsDropDownOpenProperty, value);
        }

        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(MultiSelectComboBox),
                new PropertyMetadata(false, OnIsDropDownOpenChanged));

        private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiSelectComboBox control && control._dropdown != null)
            {
                control._dropdown.IsOpen = (bool)e.NewValue;
            }
        }

        public void SelectAll()
        {
            if (_listBox != null && ItemsSource != null)
            {
                _selectedItems.Clear();
                foreach (var item in ItemsSource)
                {
                    _selectedItems.Add(item);
                }

                // Update ListBox selection
                _listBox.SelectAll();
            }
        }

        public void UnselectAll()
        {
            _selectedItems.Clear();

            if (_listBox != null)
            {
                _listBox.SelectedItems.Clear();
            }
        }

        public void UpdateFilter()
        {
            if (_listBox == null || FilterService == null)
                return;

            FilterService.SetFilter(FilterText);

            // More filter implementation would go here
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                // Free managed resources
                _suggestionProviderCancellationTokenSource?.Cancel();
                _suggestionProviderCancellationTokenSource?.Dispose();
                _suggestionProviderCancellationTokenSource = null;
            }

            _isDisposed = true;
        }

        #endregion
    }
}
