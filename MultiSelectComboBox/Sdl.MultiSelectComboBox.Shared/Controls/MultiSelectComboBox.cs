using Sdl.MultiSelectComboBox.API;
using Sdl.MultiSelectComboBox.Controls;
using Sdl.MultiSelectComboBox.EventArgs;
using Sdl.MultiSelectComboBox.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Data;
using Windows.System;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;
#else
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
#endif

#if WINUI
namespace Sdl.MultiSelectComboBox.Controls
#else
namespace Sdl.MultiSelectComboBox.Themes.Generic
#endif
{
	[TemplatePart(Name = PART_MultiSelectComboBox, Type = typeof(Grid))]
	[TemplatePart(Name = PART_MultiSelectComboBox_Dropdown, Type = typeof(Popup))]
#if WINUI
	[TemplatePart(Name = PART_MultiSelectComboBox_Dropdown_ListBox, Type = typeof(ListView))]
#else
	[TemplatePart(Name = PART_MultiSelectComboBox_Dropdown_ListBox, Type = typeof(ListBox))]
#endif
	[TemplatePart(Name = PART_MultiSelectComboBox_Dropdown_Button, Type = typeof(Button))]
	[TemplatePart(Name = PART_MultiSelectComboBox_SelectedItemsPanel_ItemsControl, Type = typeof(ItemsControl))]
	[TemplatePart(Name = PART_MultiSelectComboBox_SelectedItemsPanel_Filter_TextBox, Type = typeof(TextBox))]
	[TemplatePart(Name = PART_MultiSelectComboBox_SelectedItemsPanel_Filter_AutoComplete_TextBox, Type = typeof(TextBox))]
	[TemplatePart(Name = PART_MultiSelectComboBox_SelectedItemsPanel_RemoveItem_Button, Type = typeof(Button))]
	public class MultiSelectComboBox : Control, IDisposable
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
		private const string MultiSelectComboBox_SelectedItems_Searchable_ItemTemplate = "MultiSelectComboBox.SelectedItems.Searchable.ItemTemplate";
		private const string MultiSelectComboBox_Dropdown_ListBox_ItemTemplate = "MultiSelectComboBox.Dropdown.ListBox.ItemTemplate";

		private const string MultiSelectComboBox_SelectedItems_ItemTemplate = "MultiSelectComboBox.SelectedItems.ItemTemplate";

		public MultiSelectComboBox()
		{
#if WINUI
			this.DefaultStyleKey = typeof(MultiSelectComboBox);
#endif
			Loaded += MultiSelectComboBox_Loaded;
			Unloaded += MultiSelectComboBox_Unloaded;

#if !WINUI
			InputBindings.Add(new KeyBinding(OpenDropDownListCommand, Key.Up, ModifierKeys.Alt));
			InputBindings.Add(new KeyBinding(OpenDropDownListCommand, Key.Down, ModifierKeys.Alt));

			CommandBindings.Add(new CommandBinding(OpenDropDownListCommand, OpenDropDownListCommandExecuted));
#endif
		}

		private void MultiSelectComboBox_Loaded(object sender, RoutedEventArgs e)
		{
			InitializeSelectedItemsNotifyingCollection();
		}

		private void MultiSelectComboBox_Unloaded(object sender, RoutedEventArgs e)
		{
			CleanUpSelectedItemsNotifyingCollection();
		}

#if !WINUI
		private Window _controlWindow;
		private Window ControlWindow
		{
			get => _controlWindow;
			set
			{
				if (_controlWindow != null)
				{
					_controlWindow.LocationChanged -= ControlWindowLocationChanged;
					_controlWindow.Deactivated -= ControlWindowDeactivated;
				}

				_controlWindow = value;

				if (_controlWindow != null)
				{
					_controlWindow.LocationChanged += ControlWindowLocationChanged;
					_controlWindow.Deactivated += ControlWindowDeactivated;
				}
			}
		}
#endif

		private Grid _multiSelectComboBoxGrid;
		private Grid MultiSelectComboBoxGrid
		{
			get => _multiSelectComboBoxGrid;
			set
			{
				if (_multiSelectComboBoxGrid != null)
				{
#if WINUI
					_multiSelectComboBoxGrid.PointerPressed -= MultiSelectComboBoxOnPointerPressed;
					_multiSelectComboBoxGrid.GotFocus -= MultiSelectComboBoxGotFocus;
					_multiSelectComboBoxGrid.LostFocus -= MultiSelectComboBoxLostFocus;
					_multiSelectComboBoxGrid.KeyUp -= MultiSelectComboBoxKeyUp;
					_multiSelectComboBoxGrid.SizeChanged -= MultiSelectComboBoxGridSizeChanged;

					this.KeyUp -= MultiSelectComboBox_KeyUp;
					this.KeyDown -= MultiSelectComboBox_KeyDown;
#else
					_multiSelectComboBoxGrid.PreviewMouseDown -= MultiSelectComboBoxOnPreviewMouseDown;
					_multiSelectComboBoxGrid.GotFocus -= MultiSelectComboBoxGotFocus;
					_multiSelectComboBoxGrid.LostFocus -= MultiSelectComboBoxLostFocus;
					_multiSelectComboBoxGrid.KeyUp -= MultiSelectComboBoxKeyUp;
					_multiSelectComboBoxGrid.SizeChanged -= MultiSelectComboBoxGridSizeChanged;

					PreviewKeyUp -= MultiSelectComboBox_PreviewKeyUp;
					PreviewKeyDown -= MultiSelectComboBox_PreviewKeyDown;
#endif
				}

				_multiSelectComboBoxGrid = value;

				if (_multiSelectComboBoxGrid != null)
				{
#if WINUI
					_multiSelectComboBoxGrid.PointerPressed += MultiSelectComboBoxOnPointerPressed;
					_multiSelectComboBoxGrid.GotFocus += MultiSelectComboBoxGotFocus;
					_multiSelectComboBoxGrid.LostFocus += MultiSelectComboBoxLostFocus;
					_multiSelectComboBoxGrid.KeyUp += MultiSelectComboBoxKeyUp;
					_multiSelectComboBoxGrid.SizeChanged += MultiSelectComboBoxGridSizeChanged;

					this.KeyUp += MultiSelectComboBox_KeyUp;
					this.KeyDown += MultiSelectComboBox_KeyDown;
#else
					_multiSelectComboBoxGrid.PreviewMouseDown += MultiSelectComboBoxOnPreviewMouseDown;
					_multiSelectComboBoxGrid.GotFocus += MultiSelectComboBoxGotFocus;
					_multiSelectComboBoxGrid.LostFocus += MultiSelectComboBoxLostFocus;
					_multiSelectComboBoxGrid.KeyUp += MultiSelectComboBoxKeyUp;
					_multiSelectComboBoxGrid.SizeChanged += MultiSelectComboBoxGridSizeChanged;

					PreviewKeyUp += MultiSelectComboBox_PreviewKeyUp;
					PreviewKeyDown += MultiSelectComboBox_PreviewKeyDown;
#endif
				}
			}
		}

#if !WINUI
		private void MultiSelectComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Tab || !IsDropDownOpen)
				return;

			if (Keyboard.Modifiers == ModifierKeys.None)
				DropdownListBoxPreviewKeyDown(this, new KeyEventArgs(e.KeyboardDevice, e.InputSource, e.Timestamp, Key.Return));
			else if (Keyboard.Modifiers == ModifierKeys.Shift)
			{
				IsDropDownOpen = false;
				UpdateAutoCompleteFilterText(string.Empty, null);
			}
			else
				return;
			e.Handled = true;
		}

		private void MultiSelectComboBox_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			// allows the user to switch to edit mode when control as focus and typing F2
			if (e.Key == Key.F2 && !IsEditMode)
			{
				AssignIsEditMode();
			}
		}
#else
		private void MultiSelectComboBox_KeyDown(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key != VirtualKey.Tab || !IsDropDownOpen)
				return;

			var shiftState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
			//bool isShiftPressed = (shiftState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down;
			bool isShiftPressed = false;
			if (!isShiftPressed)
			{
				// Simulate Enter key behavior
				SelectComboBoxItem();
				IsDropDownOpen = false;
				if (SelectedItemsFilterTextBox != null)
					SelectedItemsFilterTextBox.Text = string.Empty;
				FilterTextApplied = string.Empty;
				UpdateItems(string.Empty);
			}
			else
			{
				IsDropDownOpen = false;
				UpdateAutoCompleteFilterText(string.Empty, null);
			}
			e.Handled = true;
		}

		private void MultiSelectComboBox_KeyUp(object sender, KeyRoutedEventArgs e)
		{
			// allows the user to switch to edit mode when control has focus and typing F2
			if (e.Key == VirtualKey.F2 && !IsEditMode)
			{
				AssignIsEditMode();
			}
		}
#endif

		private Popup _dropdownMenu;
		private Popup DropdownMenu
		{
			get => _dropdownMenu;
			set
			{
				if (_dropdownMenu != null)
				{
					_dropdownMenu.Closed -= DropdownMenuClosed;
					_dropdownMenu.Opened -= DropdownMenuOpened;
				}

				_dropdownMenu = value;

				if (_dropdownMenu != null)
				{
					_dropdownMenu.Closed += DropdownMenuClosed;
					_dropdownMenu.Opened += DropdownMenuOpened;
				}
			}
		}

#if WINUI
		private ListView _dropdownListBox;
		private ListView DropdownListBox
#else
		private ListBox _dropdownListBox;
		private ListBox DropdownListBox
#endif
		{
			get => _dropdownListBox;
			set
			{
#if WINUI
				System.Diagnostics.Debug.WriteLine($"[WINUI] DropdownListBox set: {(value != null ? "Found" : "NULL")}");
#endif
				if (_dropdownListBox != null)
				{
					_dropdownListBox.SelectionChanged -= DropdownListBoxSelectionChanged;
#if WINUI
					_dropdownListBox.ItemClick -= DropdownListBoxItemClick;
					_dropdownListBox.KeyDown -= DropdownListBoxKeyDown;
#else
					_dropdownListBox.PreviewMouseUp -= DropdownListBoxPreviewMouseUp;
					_dropdownListBox.PreviewKeyDown -= DropdownListBoxPreviewKeyDown;
					_dropdownListBox.ItemContainerGenerator.StatusChanged -= DropDownListBoxItemContainerGenerator_StatusChanged;
					_dropdownListBox.RemoveHandler(ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(DropDownListBoxScrolled));
#endif
				}

				_dropdownListBox = value;

				if (_dropdownListBox != null)
				{
#if WINUI
					// WinUI/Uno uses ListView with Single selection mode - we manage multi-selection separately
					_dropdownListBox.SelectionMode = ListViewSelectionMode.Single;
					_dropdownListBox.ItemsSource = ItemsSource;
					
					if (DropdownItemTemplate == null && _dropdownListBox.Resources.ContainsKey(MultiSelectComboBox_Dropdown_ListBox_ItemTemplate))
					{
						DropdownItemTemplate = _dropdownListBox.Resources[MultiSelectComboBox_Dropdown_ListBox_ItemTemplate] as DataTemplate;
					}
					
					if (DropdownItemTemplate != null)
					{
						_dropdownListBox.ItemTemplate = DropdownItemTemplate;
					}
#else
					if (DropdownItemTemplate == null)
					{
						DropdownItemTemplate = _dropdownListBox.FindResource(MultiSelectComboBox_Dropdown_ListBox_ItemTemplate) as DataTemplate;
					}

					DropdownItemTemplateSelector = new DropdownItemTemplateService(DropdownItemTemplate);

					// this should always be set to Single; multiple selection feature is managed separately.
					_dropdownListBox.SelectionMode = System.Windows.Controls.SelectionMode.Single;
					_dropdownListBox.ItemsSource = ItemsCollectionViewSource?.View;

					_dropdownListBox.ItemContainerGenerator.StatusChanged += DropDownListBoxItemContainerGenerator_StatusChanged;
					_dropdownListBox.AddHandler(ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(DropDownListBoxScrolled));
#endif

					_dropdownListBox.SelectionChanged += DropdownListBoxSelectionChanged;
#if WINUI
					_dropdownListBox.ItemClick += DropdownListBoxItemClick;
					_dropdownListBox.KeyDown += DropdownListBoxKeyDown;
#else
					_dropdownListBox.PreviewMouseUp += DropdownListBoxPreviewMouseUp;
					_dropdownListBox.PreviewKeyDown += DropdownListBoxPreviewKeyDown;
#endif
				}
			}
		}

#if !WINUI
		private CollectionViewSource _itemsCollectionViewSource;
		private CollectionViewSource ItemsCollectionViewSource
		{
			get => _itemsCollectionViewSource;
			set
			{
				_itemsCollectionViewSource = value;

				if (ItemsCollectionViewSource != null && ItemsSource != null)
				{
					if (EnableGrouping)
					{
						// check that the items are groupable before adding a default group definition
						if (ItemsCollectionViewSource.GroupDescriptions.Count == 0)
						{
							var isGenericTypeGroupable = ItemsSource.GetType().IsGenericType
								&& typeof(IItemGroupAware).IsAssignableFrom(ItemsSource.GetType().GetGenericArguments()[0]);
							if (isGenericTypeGroupable || ItemsSource.Count > 0 && ItemsSource[0] is IItemGroupAware)
							{
								ItemsCollectionViewSource.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
							}
						}

						foreach (var groupDescription in ItemsCollectionViewSource.GroupDescriptions)
						{
							groupDescription.CustomSort = GroupComparerService;
						}
					}
					else
					{
						ItemsCollectionViewSource?.GroupDescriptions.Clear();
					}

					CurrentFilterService = FilterService ?? new DefaultFilterService();
					CurrentFilterService.SetFilter(EnableFiltering ? SelectedItemsFilterTextBox?.Text : string.Empty);
				}

				InitializeInternalElements();
			}
		}
#endif

		private ItemsControl _selectedItemsControl;
		private ItemsControl SelectedItemsControl
		{
			get => _selectedItemsControl;
			set
			{
#if WINUI
				System.Diagnostics.Debug.WriteLine($"[WINUI] SelectedItemsControl set: {(value != null ? "Found" : "NULL")}");
#endif
				if (_selectedItemsControl != null)
				{
#if WINUI
					_selectedItemsControl.PointerPressed -= SelectedItemsControl_OnPointerPressed;
					_selectedItemsControl.KeyUp -= SelectedItemsControl_OnKeyUp;
#else
					_selectedItemsControl.Items.CurrentChanged -= SelectedItemsControl_CurrentChanged;
					_selectedItemsControl.PreviewMouseDown -= SelectedItemsControl_OnPreviewMouseDown;
					_selectedItemsControl.KeyUp -= SelectedItemsControl_OnKeyUp;
#endif
				}

				_selectedItemsControl = value;

				if (_selectedItemsControl != null)
				{
					AddFilterPlaceholderIfNeeded();

					_selectedItemsControl.ItemsSource = SelectedItemsInternal;

					if (SelectedItemTemplate == null)
					{
#if WINUI
						if (_selectedItemsControl.Resources.ContainsKey(MultiSelectComboBox_SelectedItems_ItemTemplate))
							SelectedItemTemplate = _selectedItemsControl.Resources[MultiSelectComboBox_SelectedItems_ItemTemplate] as DataTemplate;
#else
						SelectedItemTemplate = _selectedItemsControl.FindResource(MultiSelectComboBox_SelectedItems_ItemTemplate) as DataTemplate;
#endif
						System.Diagnostics.Debug.WriteLine($"[WINUI] SelectedItemTemplate loaded: {SelectedItemTemplate != null}");
					}

					DataTemplate searchableTemplate = null;
#if WINUI
					if (_selectedItemsControl.Resources.ContainsKey(MultiSelectComboBox_SelectedItems_Searchable_ItemTemplate))
						searchableTemplate = _selectedItemsControl.Resources[MultiSelectComboBox_SelectedItems_Searchable_ItemTemplate] as DataTemplate;
#else
					searchableTemplate = _selectedItemsControl.FindResource(MultiSelectComboBox_SelectedItems_Searchable_ItemTemplate) as DataTemplate;
#endif
					System.Diagnostics.Debug.WriteLine($"[WINUI] SearchableTemplate loaded: {searchableTemplate != null}");
					
					SelectedItemTemplateSelector = new SelectedItemTemplateService(SelectedItemTemplate, searchableTemplate);
#if WINUI
					_selectedItemsControl.ItemTemplateSelector = SelectedItemTemplateSelector;
#endif

#if WINUI
					_selectedItemsControl.PointerPressed += SelectedItemsControl_OnPointerPressed;
#else
					_selectedItemsControl.Items.CurrentChanged += SelectedItemsControl_CurrentChanged;
					_selectedItemsControl.PreviewMouseDown += SelectedItemsControl_OnPreviewMouseDown;
#endif
					_selectedItemsControl.KeyUp += SelectedItemsControl_OnKeyUp;
				}
			}
		}

		private TextBox _selectedItemsFilterTextBox;
		private TextBox SelectedItemsFilterTextBox
		{
			get => _selectedItemsFilterTextBox ?? (SelectedItemsFilterTextBox =
					   VisualTreeService.FindVisualChild<TextBox>(SelectedItemsControl, PART_MultiSelectComboBox_SelectedItemsPanel_Filter_TextBox));
			set
			{
				if (_selectedItemsFilterTextBox != null)
				{
#if WINUI
					_selectedItemsFilterTextBox.BeforeTextChanging -= SelectedItemsFilterTextBoxBeforeTextChanging;
					_selectedItemsFilterTextBox.TextChanged -= SelectedItemsFilterTextBoxTextChanged;
#else
					_selectedItemsFilterTextBox.RemoveHandler(CommandManager.ExecutedEvent, (ExecutedRoutedEventHandler)Execute_TextBoxCommand);
					_selectedItemsFilterTextBox.RemoveHandler(CommandManager.PreviewCanExecuteEvent, (CanExecuteRoutedEventHandler)CanExecute_TextBoxCommand);
					_selectedItemsFilterTextBox.PreviewTextInput -= SelectedItemsFilterTextBoxPreviewTextInput;
					_selectedItemsFilterTextBox.TextChanged -= SelectedItemsFilterTextBoxTextChanged;
#endif
				}

				_selectedItemsFilterTextBox = value;

				if (_selectedItemsFilterTextBox != null)
				{
#if WINUI
					_selectedItemsFilterTextBox.BeforeTextChanging += SelectedItemsFilterTextBoxBeforeTextChanging;
					_selectedItemsFilterTextBox.TextChanged += SelectedItemsFilterTextBoxTextChanged;
#else
					_selectedItemsFilterTextBox.PreviewTextInput += SelectedItemsFilterTextBoxPreviewTextInput;
					_selectedItemsFilterTextBox.TextChanged += SelectedItemsFilterTextBoxTextChanged;
					_selectedItemsFilterTextBox.AddHandler(CommandManager.PreviewCanExecuteEvent, (CanExecuteRoutedEventHandler)CanExecute_TextBoxCommand);
					_selectedItemsFilterTextBox.AddHandler(CommandManager.ExecutedEvent, (ExecutedRoutedEventHandler)Execute_TextBoxCommand);
#endif
				}
			}
		}

		private TextBox _selectedItemsFilterAutoCompleteTextBox;
		private TextBox SelectedItemsFilterAutoCompleteTextBox
		{
			get => _selectedItemsFilterAutoCompleteTextBox ?? (SelectedItemsFilterAutoCompleteTextBox =
					   VisualTreeService.FindVisualChild<TextBox>(SelectedItemsControl, PART_MultiSelectComboBox_SelectedItemsPanel_Filter_AutoComplete_TextBox));
			set
			{
				_selectedItemsFilterAutoCompleteTextBox = value;

				if (AutoCompleteForeground != null && _selectedItemsFilterAutoCompleteTextBox != null)
				{
					_selectedItemsFilterAutoCompleteTextBox.Foreground = AutoCompleteForeground;
				}
			}
		}

		private IComparer _groupComparerService;
		private IComparer GroupComparerService => _groupComparerService ?? (_groupComparerService = new GroupComparerService());

		private IFilterService _currentFilterService;
		private IFilterService CurrentFilterService
		{
			get => _currentFilterService ?? (_currentFilterService = new DefaultFilterService());
			set => _currentFilterService = value;
		}

		private ObservableCollection<object> _selectedItemsInternal;
		private ObservableCollection<object> SelectedItemsInternal
		{
			get => _selectedItemsInternal ?? (_selectedItemsInternal = new ObservableCollection<object>());
			set => _selectedItemsInternal = value;
		}

#if !WINUI
		static MultiSelectComboBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(typeof(MultiSelectComboBox)));
			EventManager.RegisterClassHandler(typeof(MultiSelectComboBox), Mouse.MouseEnterEvent, new MouseEventHandler(OneMouseEnter), true);
			EventManager.RegisterClassHandler(typeof(MultiSelectComboBox), Mouse.MouseLeaveEvent, new MouseEventHandler(OneMouseLeave), true);
			EventManager.RegisterClassHandler(typeof(MultiSelectComboBox), Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(OnPreviewMouseDownOutside), true);
		}

		private object _previousSelectedValue;
		private DateTime _suggestionProviderLastRequest;
		private static void OneMouseLeave(object sender, MouseEventArgs e)
		{
			var comboBox = sender as MultiSelectComboBox;
			if (comboBox.IsDropDownOpen && !comboBox.IsMouseCaptured)
			{
				Mouse.Capture(comboBox, CaptureMode.SubTree);
			}
		}
		private static void OneMouseEnter(object sender, MouseEventArgs e)
		{

			var comboBox = sender as MultiSelectComboBox;
			if (comboBox.IsDropDownOpen && comboBox.IsMouseCaptured)
			{
				comboBox.CaptureMouse();
				comboBox.ReleaseMouseCapture();
			}
		}

		private static void OnPreviewMouseDownOutside(object sender, MouseButtonEventArgs e)
		{

			MultiSelectComboBox comboBox = sender as MultiSelectComboBox;
			if (comboBox != null)
			{
				if (comboBox.IsDropDownOpen)
					comboBox.IgnoreDropdownClosingFocusPlanUntil = DateTime.Now.AddSeconds(1);
				comboBox.CloseDropdownMenu(comboBox.ClearFilterOnDropdownClosing, false);
				comboBox.CaptureMouse();
				comboBox.ReleaseMouseCapture();
			}
		}
		private DateTime? IgnoreDropdownClosingFocusPlanUntil;
#else
		private object _previousSelectedValue;
#endif

#if !WINUI
		private void DropDownListBoxScrolled(object sender, RoutedEventArgs e)
		{
			var suggestionProvider = SuggestionProvider;
			if (_dropdownListBox == null || suggestionProvider == null)
				return;
			if (DateTime.Now.Subtract(_suggestionProviderLastRequest).TotalSeconds < 0.2)
				return;
			var scrollViewer = VisualTreeService.FindVisualChild<ScrollViewer>(_dropdownListBox, null);
			if (scrollViewer == null || scrollViewer.ContentVerticalOffset / scrollViewer.ScrollableHeight < 0.85)
				return;
			_suggestionProviderLastRequest = DateTime.Now;
			if (!suggestionProvider.HasMoreSuggestions)
				return;
			IsLoadingSuggestions = true;
			DropDownListBoxScrolledAsync().ContinueWith(t => IsLoadingSuggestions = false, TaskContinuationOptions.ExecuteSynchronously);
		}

		private async Task DropDownListBoxScrolledAsync()
		{
			var suggestionProvider = SuggestionProvider;
			var items = await suggestionProvider.GetSuggestionsAsync(_suggestionProviderToken.Token);
			await Dispatcher.BeginInvoke(new Action(() =>
			{
				foreach (var item in items)
					ItemsSource.Add(item);
				_suggestionProviderLastRequest = _suggestionProviderLastRequest.AddSeconds(-1);
			}));
		}
#endif

#if WINUI
		protected override void OnApplyTemplate()
		{
			System.Diagnostics.Debug.WriteLine("[WINUI] OnApplyTemplate called");
			base.OnApplyTemplate();

			MultiSelectComboBoxGrid = GetTemplateChild(PART_MultiSelectComboBox) as Grid;
			System.Diagnostics.Debug.WriteLine($"[WINUI] MultiSelectComboBoxGrid: {(MultiSelectComboBoxGrid != null ? "Found" : "NULL")}");

			if (MultiSelectComboBoxGrid != null)
			{
				// WinUI doesn't have Window.GetWindow - we handle window events differently
				ApplyInternalTemplates(MultiSelectComboBoxGrid);
			}

			InitializeInternalElements();

			// Ensure initializing SelectedItems if it was not set
			if (SelectedItems == null)
			{
				System.Diagnostics.Debug.WriteLine("[WINUI] Initializing SelectedItems collection");
				SelectedItems = new ObservableCollection<object>();
			}
			System.Diagnostics.Debug.WriteLine("[WINUI] OnApplyTemplate completed");
		}
#else
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			MultiSelectComboBoxGrid = GetTemplateChild(PART_MultiSelectComboBox) as Grid;

			if (MultiSelectComboBoxGrid != null)
			{
				ControlWindow = Window.GetWindow(MultiSelectComboBoxGrid);

				// We expect internal SelectedItemsControl to have its template applied upon InitializeInternalElements.
				ApplyInternalTemplates(MultiSelectComboBoxGrid);
			}

			InitializeInternalElements();

			// Ensure initializing SelectedItems if it was not set by the developer so that SelectedItem
			// property (singular), that is based on this collection internally, would still work properly.
			if (SelectedItems == null)
			{
				SelectedItems = new ObservableCollection<object>();
			}
		}
#endif

		private void ApplyInternalTemplates(FrameworkElement parent)
		{
			if (parent == null)
				return;

			if (parent == null)
				return;

			// Ensure template is applied so we can traverse the visual tree (critical for WinUI ScrollViewer content)
			//parent.ApplyTemplate();

			int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < childrenCount; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
				ApplyInternalTemplates(child);
			}
		}

		private TextBox TextBoxNewItem;

		private void InitializeInternalElements()
		{
			System.Diagnostics.Debug.WriteLine("[WINUI] InitializeInternalElements called");
			if (SelectedItemsControl == null)
			{
				SelectedItemsControl = GetTemplateChild(PART_MultiSelectComboBox_SelectedItemsPanel_ItemsControl) as ItemsControl;
				if (SelectedItemsControl == null && MultiSelectComboBoxGrid != null)
				{
					// Fallback
					SelectedItemsControl = VisualTreeService.FindVisualChild<ItemsControl>(MultiSelectComboBoxGrid, PART_MultiSelectComboBox_SelectedItemsPanel_ItemsControl);
				}
				System.Diagnostics.Debug.WriteLine($"[WINUI] SelectedItemsControl set: {(SelectedItemsControl != null ? "Found" : "NULL")}");
			}

			if (DropdownListBox == null)
			{
				if (DropdownMenu == null)
				{
					DropdownMenu = GetTemplateChild(PART_MultiSelectComboBox_Dropdown) as Popup;
					if (DropdownMenu == null && MultiSelectComboBoxGrid != null)
					{
						DropdownMenu = VisualTreeService.FindVisualChild<Popup>(MultiSelectComboBoxGrid, PART_MultiSelectComboBox_Dropdown);
					}
					System.Diagnostics.Debug.WriteLine($"[WINUI] DropdownMenu: {(DropdownMenu != null ? "Found" : "NULL")}");
				}

				if (DropdownMenu != null)
				{
#if WINUI
					DropdownListBox = VisualTreeService.FindVisualChild<ListView>(DropdownMenu.Child as DependencyObject, PART_MultiSelectComboBox_Dropdown_ListBox);
					System.Diagnostics.Debug.WriteLine($"[WINUI] DropdownListBox (ListView): {(DropdownListBox != null ? "Found" : "NULL")}");
					if (DropdownListBox != null)
					{
						DropdownListBox.SelectionMode = ListViewSelectionMode.None; // We handle selection manually
						DropdownListBox.IsItemClickEnabled = true;
						DropdownListBox.ItemClick += DropdownListBoxItemClick;
						DropdownListBox.KeyDown += DropdownListBoxKeyDown;
						DropdownListBox.ContainerContentChanging += DropdownListBox_ContainerContentChanging;
					}
#else
					DropdownListBox = VisualTreeService.FindVisualChild<ListBox>(DropdownMenu.Child, PART_MultiSelectComboBox_Dropdown_ListBox);
#endif
				}

				if (DropdownMenu != null)
				{
#if WINUI
					var popupChild = DropdownMenu.Child as DependencyObject;
#else
					var popupChild = DropdownMenu.Child;
#endif
					var newItemCreated = VisualTreeService.FindVisualChild<Button>(popupChild, PART_MultiSelectComboBox_Dropdown_NewItem_CreatedOkButton);
					if (newItemCreated != null)
					{
						TextBoxNewItem = VisualTreeService.FindVisualChild<TextBox>(popupChild, PART_MultiSelectComboBox_Dropdown_NewItem_TextBox);
						if (TextBoxNewItem != null)
						{
							newItemCreated.Click += NewItemCreated_Click;
#if WINUI
							TextBoxNewItem.KeyDown += (s, e) => { if (e.Key == VirtualKey.Enter) { e.Handled = true; NewItemCreated_Click(newItemCreated, null); } };
#else
							TextBoxNewItem.KeyDown += (s, e) => { if (e.Key == Key.Enter) { e.Handled = true; NewItemCreated_Click(newItemCreated, null); } };
#endif
						}
					}
					var selectAllBtn = VisualTreeService.FindVisualChild<Button>(popupChild, PART_MultiSelectComboBox_Dropdown_SelectAllButton);
					if (selectAllBtn != null)
						selectAllBtn.Click += SelectAll_Click;

					var clearAllBtn = VisualTreeService.FindVisualChild<Button>(popupChild, PART_MultiSelectComboBox_Dropdown_ClearAllButton);
					if (clearAllBtn != null)
						clearAllBtn.Click += ClearAll_Click;
				}
			}

			if (ItemsSource != null)
			{
#if !WINUI
				if (ItemsCollectionViewSource?.Source != ItemsSource)
				{
					ItemsCollectionViewSource = new CollectionViewSource
					{
						Source = ItemsSource
					};
				}

				if (DropdownListBox != null)
				{
					DropdownListBox.ItemsSource = ItemsCollectionViewSource?.View;
				}
#else
				System.Diagnostics.Debug.WriteLine($"[WINUI] ItemsSource count: {ItemsSource.Count}");
				if (DropdownListBox != null)
				{
					DropdownListBox.ItemsSource = ItemsSource;
					System.Diagnostics.Debug.WriteLine($"[WINUI] Set DropdownListBox.ItemsSource with {ItemsSource.Count} items");
				}
				else
				{
					System.Diagnostics.Debug.WriteLine("[WINUI] WARNING: DropdownListBox is NULL, cannot set ItemsSource!");
				}
#endif

				if (ItemsSource.Count > 0)
				{
					UpdateSelectedItemsContainer(ItemsSource);
				}

				UpdateItems(SelectedItemsFilterTextBox?.Text ?? string.Empty);
			}
#if WINUI
			System.Diagnostics.Debug.WriteLine("[WINUI] InitializeInternalElements END");
#endif
		}

		private void SelectAll_Click(object sender, RoutedEventArgs e)
		{
			foreach (var itm in ItemsSource)
			{
				var listBoxItem = GetListViewItem(itm);
				if (listBoxItem == null)
					continue;
				if (itm is IItemEnabledAware enabledAware == false || enabledAware.IsEnabled)
					listBoxItem.IsChecked = true;
			}

			UpdateSelectedItemsContainer(ItemsSource);
		}

		private void ClearAll_Click(object sender, RoutedEventArgs e)
		{
			foreach (var itm in ItemsSource)
			{
				var listBoxItem = GetListViewItem(itm);
				if (listBoxItem == null)
					continue;
				if (itm is IItemEnabledAware enabledAware == false || enabledAware.IsEnabled)
					listBoxItem.IsChecked = false;
			}

			UpdateSelectedItemsContainer(ItemsSource);
		}

		private void NewItemCreated_Click(object sender, RoutedEventArgs e)
		{
			RaiseNewItemAddRequestEvent(TextBoxNewItem.Text);
			TextBoxNewItem.Text = "";
		}

		public enum SelectionModes
		{
			Multiple = 0,
			Single
		}

		#region Dependency Properties

		public static readonly DependencyProperty EnableBatchSelectionProperty =
			DependencyProperty.Register("EnableBatchSelection", typeof(bool), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(false));
#else
				new FrameworkPropertyMetadata(false));
#endif

		public bool EnableBatchSelection
		{
			get => (bool)GetValue(EnableBatchSelectionProperty);
			set => SetValue(EnableBatchSelectionProperty, value);
		}

		public static readonly DependencyProperty EnableAutoCompleteProperty =
			DependencyProperty.Register("EnableAutoComplete", typeof(bool), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(true));
#else
				new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
#endif

		public bool EnableAutoComplete
		{
			get => (bool)GetValue(EnableAutoCompleteProperty);
			set => SetValue(EnableAutoCompleteProperty, value);
		}

		public static readonly DependencyProperty AutoCompleteBackgroundProperty =
			DependencyProperty.Register("AutoCompleteBackground", typeof(Brush), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(null));
#else
				new FrameworkPropertyMetadata(Brushes.Gainsboro, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
#endif

		public Brush AutoCompleteBackground
		{
			get => (Brush)GetValue(AutoCompleteBackgroundProperty);
			set => SetValue(AutoCompleteBackgroundProperty, value);
		}

		public static readonly DependencyProperty AutoCompleteForegroundProperty =
			DependencyProperty.Register("AutoCompleteForeground", typeof(Brush), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(null));
#else
				new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
#endif

		public Brush AutoCompleteForeground
		{
			get => (Brush)GetValue(AutoCompleteForegroundProperty);
			set => SetValue(AutoCompleteForegroundProperty, value);
		}

		public static readonly DependencyProperty AutoCompleteMaxLengthProperty =
			DependencyProperty.Register("AutoCompleteMaxLength", typeof(int), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(0));
#else
				new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
#endif

		public int AutoCompleteMaxLength
		{
			get => (int)GetValue(AutoCompleteMaxLengthProperty);
			set => SetValue(AutoCompleteMaxLengthProperty, value);
		}

		// Events - these need platform-specific handling
#if !WINUI
		public static readonly RoutedEvent NewItemAddRequestEvent =
			EventManager.RegisterRoutedEvent(nameof(NewItemAddRequest), RoutingStrategy.Direct,
				typeof(EventHandler<NewItemAddRequestEventArgs>), typeof(MultiSelectComboBox));

		public event EventHandler<NewItemAddRequestEventArgs> NewItemAddRequest
		{
			add => AddHandler(NewItemAddRequestEvent, value);
			remove => RemoveHandler(NewItemAddRequestEvent, value);
		}

		public static readonly RoutedEvent ItemDeleteRequestEvent =
			EventManager.RegisterRoutedEvent(nameof(ItemDeleteRequest), RoutingStrategy.Direct,
				typeof(EventHandler<ItemDeleteRequestEventArgs>), typeof(MultiSelectComboBox));

		public event EventHandler<ItemDeleteRequestEventArgs> ItemDeleteRequest
		{
			add => AddHandler(ItemDeleteRequestEvent, value);
			remove => RemoveHandler(ItemDeleteRequestEvent, value);
		}

		public static readonly RoutedEvent FilterTextChangedEvent =
			EventManager.RegisterRoutedEvent("FilterTextChanged", RoutingStrategy.Direct,
				typeof(EventHandler<FilterTextChangedEventArgs>), typeof(MultiSelectComboBox));

		public event EventHandler<FilterTextChangedEventArgs> FilterTextChanged
		{
			add => AddHandler(FilterTextChangedEvent, value);
			remove => RemoveHandler(FilterTextChangedEvent, value);
		}

		public static readonly RoutedEvent SelectedItemsChangedEvent =
			EventManager.RegisterRoutedEvent("SelectedItemsChanged", RoutingStrategy.Direct,
				typeof(EventHandler<SelectedItemsChangedEventArgs>), typeof(MultiSelectComboBox));

		public event EventHandler<SelectedItemsChangedEventArgs> SelectedItemsChanged
		{
			add => AddHandler(SelectedItemsChangedEvent, value);
			remove => RemoveHandler(SelectedItemsChangedEvent, value);
		}
#else
		// WinUI events - use standard CLR events
		public event EventHandler<NewItemAddRequestEventArgs> NewItemAddRequest;
		public event EventHandler<ItemDeleteRequestEventArgs> ItemDeleteRequest;
		public event EventHandler<FilterTextChangedEventArgs> FilterTextChanged;
		public event EventHandler<SelectedItemsChangedEventArgs> SelectedItemsChanged;
#endif

		public static readonly DependencyProperty EnableGroupingProperty =
			DependencyProperty.Register("EnableGrouping", typeof(bool), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(true, EnableGroupingPropertyChangedCallback));
#else
				new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.None, EnableGroupingPropertyChangedCallback));
#endif

		public bool EnableGrouping
		{
			get => (bool)GetValue(EnableGroupingProperty);
			set => SetValue(EnableGroupingProperty, value);
		}

		private static void EnableGroupingPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var control = dependencyObject as MultiSelectComboBox;

#if !WINUI
			if (control?.MultiSelectComboBoxGrid != null)
			{
				control.ItemsCollectionViewSource = control.ItemsCollectionViewSource;
			}
#endif
		}

		public static readonly DependencyProperty EnableSelectAllUIProperty =
			DependencyProperty.Register(nameof(EnableSelectAllUI), typeof(bool), typeof(MultiSelectComboBox),
				new PropertyMetadata(false));

		public bool EnableSelectAllUI
		{
			get { return (bool)GetValue(EnableSelectAllUIProperty); }
			set { SetValue(EnableSelectAllUIProperty, value); }
		}

		public static readonly DependencyProperty EnableClearAllUIProperty =
			DependencyProperty.Register(nameof(EnableClearAllUI), typeof(bool), typeof(MultiSelectComboBox),
				new PropertyMetadata(false));

		public bool EnableClearAllUI
		{
			get { return (bool)GetValue(EnableClearAllUIProperty); }
			set { SetValue(EnableClearAllUIProperty, value); }
		}

		public static readonly DependencyProperty EnableNewItemAddUIProperty =
			DependencyProperty.Register(nameof(EnableNewItemAddUI), typeof(bool), typeof(MultiSelectComboBox),
				new PropertyMetadata(false));

		public bool EnableNewItemAddUI
		{
			get { return (bool)GetValue(EnableNewItemAddUIProperty); }
			set { SetValue(EnableNewItemAddUIProperty, value); }
		}

		public static readonly DependencyProperty EnableDeleteItemUIProperty =
			DependencyProperty.Register(nameof(EnableDeleteItemUI), typeof(bool), typeof(MultiSelectComboBox),
				new PropertyMetadata(false));

		public bool EnableDeleteItemUI
		{
			get { return (bool)GetValue(EnableDeleteItemUIProperty); }
			set { SetValue(EnableDeleteItemUIProperty, value); }
		}

		public static readonly DependencyProperty EnableFilteringProperty =
			DependencyProperty.Register("EnableFiltering", typeof(bool), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(true, EnableFilteringPropertyChangedCallback));
#else
				new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.None, EnableFilteringPropertyChangedCallback));
#endif

		public bool EnableFiltering
		{
			get => (bool)GetValue(EnableFilteringProperty);
			set => SetValue(EnableFilteringProperty, value);
		}

		private static void EnableFilteringPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var control = dependencyObject as MultiSelectComboBox;

#if !WINUI
			if (control?.MultiSelectComboBoxGrid != null)
			{
				control.ItemsCollectionViewSource = control.ItemsCollectionViewSource;
			}
#endif
		}

		public static readonly DependencyProperty FilterServiceProperty =
			DependencyProperty.Register("FilterService", typeof(IFilterService), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(null, FilterServicePropertyChangedCallback));
#else
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, FilterServicePropertyChangedCallback));
#endif

		public IFilterService FilterService
		{
			get => (IFilterService)GetValue(FilterServiceProperty);
			set => SetValue(FilterServiceProperty, value);
		}

		private static void FilterServicePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var control = dependencyObject as MultiSelectComboBox;

#if !WINUI
			if (control?.MultiSelectComboBoxGrid != null)
			{
				control.ItemsCollectionViewSource = control.ItemsCollectionViewSource;
			}
#endif
		}

		public static readonly DependencyProperty AutoCompleteServiceProperty =
			DependencyProperty.Register("AutoCompleteService", typeof(IAutoCompleteService), typeof(MultiSelectComboBox),
				new PropertyMetadata(null));

		public IAutoCompleteService AutoCompleteService
		{
			get { return (IAutoCompleteService)GetValue(AutoCompleteServiceProperty); }
			set { SetValue(AutoCompleteServiceProperty, value); }
		}

		private IAutoCompleteService CurrentAutoCompleteService => AutoCompleteService ?? DefaultAutoCompleteService.Instance;

		public static readonly DependencyProperty IsDropDownOpenProperty =
			DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(false, OnIsDropDownOpenChanged));
#else
				new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
#endif

		public bool IsDropDownOpen
		{
			get => (bool)GetValue(IsDropDownOpenProperty);
			set => SetValue(IsDropDownOpenProperty, value);
		}

#if WINUI
		private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine($"[WINUI] IsDropDownOpen changed: {e.OldValue} -> {e.NewValue}");
			if (d is MultiSelectComboBox control && control._dropdownMenu != null)
			{
				control._dropdownMenu.IsOpen = (bool)e.NewValue;
				if ((bool)e.NewValue)
				{
					control.UpdateDropdownPosition();
				}
				System.Diagnostics.Debug.WriteLine($"[WINUI] Set Popup.IsOpen = {e.NewValue}");
			}
			else if (d is MultiSelectComboBox ctrl)
			{
				System.Diagnostics.Debug.WriteLine($"[WINUI] WARNING: _dropdownMenu is NULL!");
			}
		}
#endif

		public static readonly DependencyProperty SelectionModeProperty =
			DependencyProperty.Register("SelectionMode", typeof(SelectionModes), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(SelectionModes.Multiple, SelectionModePropertyChangedCallback));
#else
				new FrameworkPropertyMetadata(SelectionModes.Multiple, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectionModePropertyChangedCallback));
#endif

		private static void SelectionModePropertyChangedCallback(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var control = dependencyObject as MultiSelectComboBox;
			if (control?.MultiSelectComboBoxGrid != null)
			{
				control.UpdateSelectedItemsContainer(control.ItemsSource);
			}
		}

		public SelectionModes SelectionMode
		{
			get => (SelectionModes)GetValue(SelectionModeProperty);
			set => SetValue(SelectionModeProperty, value);
		}

		public static readonly DependencyProperty MaxDropDownHeightProperty =
			DependencyProperty.Register("MaxDropDownHeight", typeof(int), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(360));
#else
				new FrameworkPropertyMetadata(360, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
#endif

		public int MaxDropDownHeight
		{
			get => (int)GetValue(MaxDropDownHeightProperty);
			set => SetValue(MaxDropDownHeightProperty, value);
		}

		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof(IList), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(null, ItemsPropertyChangedCallback));
#else
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ItemsPropertyChangedCallback));
#endif

		public IList ItemsSource
		{
			get => (IList)GetValue(ItemsSourceProperty);
			set => SetValue(ItemsSourceProperty, value);
		}

		private static void ItemsPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			if (!(dependencyObject is MultiSelectComboBox control))
			{
				return;
			}

			control.InitializeInternalElements();
		}

		public static readonly DependencyProperty SelectedItemProperty =
			DependencyProperty.Register("SelectedItem", typeof(object), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(null, SelectedItemPropertyChangedCallback));
#else
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedItemPropertyChangedCallback));
#endif

		public object SelectedItem
		{
			get => GetValue(SelectedItemProperty);
			set => SetValue(SelectedItemProperty, value);
		}

		public static readonly DependencyProperty SelectedItemsProperty =
			DependencyProperty.Register("SelectedItems", typeof(IList), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(null, SelectedItemsPropertyChangedCallback));
#else
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedItemsPropertyChangedCallback));
#endif

		public IList SelectedItems
		{
			get => (IList)GetValue(SelectedItemsProperty);
			set => SetValue(SelectedItemsProperty, value);
		}

		public static readonly DependencyProperty ClearSelectionOnFilterChangedProperty =
			DependencyProperty.Register("ClearSelectionOnFilterChanged", typeof(bool), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(false));
#else
				new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
#endif

		public bool ClearSelectionOnFilterChanged
		{
			get => (bool)GetValue(ClearSelectionOnFilterChangedProperty);
			set => SetValue(ClearSelectionOnFilterChangedProperty, value);
		}

		public static readonly DependencyProperty ClearFilterOnDropdownClosingProperty =
			DependencyProperty.Register("ClearFilterOnDropdownClosing", typeof(bool), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(true));
#else
				new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
#endif

		public bool ClearFilterOnDropdownClosing
		{
			get => (bool)GetValue(ClearFilterOnDropdownClosingProperty);
			set => SetValue(ClearFilterOnDropdownClosingProperty, value);
		}

		public static readonly DependencyProperty DropdownItemTemplateProperty =
			DependencyProperty.Register("DropdownItemTemplate", typeof(DataTemplate), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(null));
#else
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
#endif

		public DataTemplate DropdownItemTemplate
		{
			get => (DataTemplate)GetValue(DropdownItemTemplateProperty);
			set => SetValue(DropdownItemTemplateProperty, value);
		}

		public static readonly DependencyProperty SelectedItemTemplateProperty =
			DependencyProperty.Register("SelectedItemTemplate", typeof(DataTemplate), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(null));
#else
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
#endif

		public DataTemplate SelectedItemTemplate
		{
			get => (DataTemplate)GetValue(SelectedItemTemplateProperty);
			set => SetValue(SelectedItemTemplateProperty, value);
		}

		public static readonly DependencyProperty IsEditableProperty =
			DependencyProperty.Register("IsEditable", typeof(bool), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(true));
#else
				new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
#endif

		public bool IsEditable
		{
			get => (bool)GetValue(IsEditableProperty);
			set => SetValue(IsEditableProperty, value);
		}

#if WINUI
		public static readonly DependencyProperty IsEditModeProperty =
			DependencyProperty.Register("IsEditMode", typeof(bool), typeof(MultiSelectComboBox),
				new PropertyMetadata(false));

		public bool IsEditMode
		{
			get => (bool)GetValue(IsEditModeProperty);
			private set => SetValue(IsEditModeProperty, value);
		}
#else
		private static readonly DependencyPropertyKey IsEditModePropertyKey =
			DependencyProperty.RegisterReadOnly("IsEditMode", typeof(bool),
				typeof(MultiSelectComboBox), new PropertyMetadata(false));

		public static readonly DependencyProperty IsEditModeProperty = IsEditModePropertyKey.DependencyProperty;

		public bool IsEditMode => (bool)GetValue(IsEditModeProperty);
#endif

		public static readonly DependencyProperty WatermarkEmptyHintTextProperty =
			DependencyProperty.Register(nameof(WatermarkEmptyHintText), typeof(string), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(""));
#else
				new FrameworkPropertyMetadata(""));
#endif

		public string WatermarkEmptyHintText
		{
			get => (string)GetValue(WatermarkEmptyHintTextProperty);
			set => SetValue(WatermarkEmptyHintTextProperty, value);
		}

		public static readonly DependencyProperty WatermarkFloatOnNonEmptyProperty =
			DependencyProperty.Register(nameof(WatermarkFloatOnNonEmpty), typeof(bool), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(false));
#else
				new FrameworkPropertyMetadata(false));
#endif

		public bool WatermarkFloatOnNonEmpty
		{
			get => (bool)GetValue(WatermarkFloatOnNonEmptyProperty);
			set => SetValue(WatermarkFloatOnNonEmptyProperty, value);
		}

		public SelectedItemTemplateService SelectedItemTemplateSelector { get; private set; }

		public DropdownItemTemplateService DropdownItemTemplateSelector { get; private set; }

		public static readonly DependencyProperty DisableFilterUpdateOnDropDownItemSelectionChangeProperty =
			DependencyProperty.Register("DisableFilterUpdateOnDropDownItemSelectionChange", typeof(bool), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(false));
#else
				new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
#endif

		public bool DisableFilterUpdateOnDropDownItemSelectionChange
		{
			get => (bool)GetValue(DisableFilterUpdateOnDropDownItemSelectionChangeProperty);
			set => SetValue(DisableFilterUpdateOnDropDownItemSelectionChangeProperty, value);
		}

		public static readonly DependencyProperty SetFocusOnFirstSelectedItemOnDropDownProperty =
			DependencyProperty.Register("SetFocusOnFirstSelectedItemOnDropDown", typeof(bool), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(false));
#else
				new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
#endif

		public bool SetFocusOnFirstSelectedItemOnDropDown
		{
			get => (bool)GetValue(SetFocusOnFirstSelectedItemOnDropDownProperty);
			set => SetValue(SetFocusOnFirstSelectedItemOnDropDownProperty, value);
		}

		private string FilterTextApplied { get; set; }

		private bool MultiSelectComboBoxHasFocus { get; set; }

		public static readonly DependencyProperty OpenDropDownListAlsoWhenNotInEditModeProperty =
			DependencyProperty.Register("OpenDropDownListAlsoWhenNotInEditMode", typeof(bool), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(false));
#else
				new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
#endif

		public bool OpenDropDownListAlsoWhenNotInEditMode
		{
			get => (bool)GetValue(OpenDropDownListAlsoWhenNotInEditModeProperty);
			set => SetValue(OpenDropDownListAlsoWhenNotInEditModeProperty, value);
		}

		public static readonly DependencyProperty SuggestionProviderProperty =
			DependencyProperty.Register("SuggestionProvider", typeof(ISuggestionProvider), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(null, SuggestionProviderPropertyChangedCallback));
#else
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, SuggestionProviderPropertyChangedCallback));
#endif

		private static void SuggestionProviderPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			if (!(dependencyObject is MultiSelectComboBox control))
				return;

			control.UpdateItems(string.Empty);
		}

		public ISuggestionProvider SuggestionProvider
		{
			get => (ISuggestionProvider)GetValue(SuggestionProviderProperty);
			set => SetValue(SuggestionProviderProperty, value);
		}

		public static readonly DependencyProperty IsLoadingSuggestionsProperty =
			DependencyProperty.Register("IsLoadingSuggestions", typeof(bool), typeof(MultiSelectComboBox),
#if WINUI
				new PropertyMetadata(false));
#else
				new FrameworkPropertyMetadata(false));
#endif

		public bool IsLoadingSuggestions
		{
			get => (bool)GetValue(IsLoadingSuggestionsProperty);
			set => SetValue(IsLoadingSuggestionsProperty, value);
		}

		public static readonly DependencyProperty SelectedItemContainerCornerRadiusProperty =
			DependencyProperty.Register("SelectedItemContainerCornerRadius", typeof(double), typeof(MultiSelectComboBox),
				new PropertyMetadata(0.0));

		public double SelectedItemContainerCornerRadius
		{
			get => (double)GetValue(SelectedItemContainerCornerRadiusProperty);
			set => SetValue(SelectedItemContainerCornerRadiusProperty, value);
		}

		public static readonly DependencyProperty SelectedItemsPanelBackgroundProperty =
			DependencyProperty.Register("SelectedItemsPanelBackground", typeof(Brush), typeof(MultiSelectComboBox),
				new PropertyMetadata(null));

		public Brush SelectedItemsPanelBackground
		{
			get => (Brush)GetValue(SelectedItemsPanelBackgroundProperty);
			set => SetValue(SelectedItemsPanelBackgroundProperty, value);
		}

		public static readonly DependencyProperty DropDownPopupBackgroundProperty =
			DependencyProperty.Register("DropDownPopupBackground", typeof(Brush), typeof(MultiSelectComboBox),
				new PropertyMetadata(null));

		public Brush DropDownPopupBackground
		{
			get => (Brush)GetValue(DropDownPopupBackgroundProperty);
			set => SetValue(DropDownPopupBackgroundProperty, value);
		}

		public static readonly DependencyProperty RemoveToolTipStringProperty =
			DependencyProperty.Register("RemoveToolTipString", typeof(string), typeof(MultiSelectComboBox),
				new PropertyMetadata("Remove"));

		public string RemoveToolTipString
		{
			get { return (string)GetValue(RemoveToolTipStringProperty); }
			set { SetValue(RemoveToolTipStringProperty, value); }
		}

		#endregion

		#region Helper Methods

		private bool IsSelectedItem(object item)
		{
			return SelectedItemsInternal.Contains(item);
		}

		private ExtendedListBoxItem GetListViewItem(object item)
		{
#if WINUI
			// WinUI doesn't have ItemContainerGenerator - use ContainerFromItem
			return DropdownListBox?.ContainerFromItem(item) as ExtendedListBoxItem;
#else
			return DropdownListBox?.ItemContainerGenerator.ContainerFromItem(item) as ExtendedListBoxItem;
#endif
		}



		private void DumpVisualTree(DependencyObject parent, int indent = 0)
		{
#if WINUI
			if (parent == null) return;
			var prefix = new string(' ', indent * 2);
			var name = (parent as FrameworkElement)?.Name ?? "Unnamed";
			var type = parent.GetType().Name;
			System.Diagnostics.Debug.WriteLine($"[WINUI] VTDump: {prefix}{type} ({name})");

			var count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < count; i++)
			{
				var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
				DumpVisualTree(child, indent + 1);
			}
#endif
		}

		private void UpdateDropdownPosition()
		{
#if WINUI
			if (DropdownMenu == null || MultiSelectComboBoxGrid == null) return;
			
			if (DropdownMenu.IsOpen)
			{
			    DropdownMenu.VerticalOffset = MultiSelectComboBoxGrid.ActualHeight;
			}
#endif
		}

		#endregion

		#region Selection Handling

		private static void RemoveSelectedItems(IList from, IList basedOn, ref Collection<object> itemsRemoved)
		{
			for (var i = from.Count - 1; i >= 0; i--)
			{
				var item = from[i];
				if (RemoveSelectedItem(from, i, basedOn))
				{
					itemsRemoved.Add(item);
				}
			}
		}

		private static bool RemoveSelectedItem(IList from, int index, IList basedOn)
		{
			if (from[index] != null && !basedOn.Contains(from[index]))
			{
				from.RemoveAt(index);
				return true;
			}
			return false;
		}

		private static void AddSelectedItems(IList to, IList basedOn, ref Collection<object> itemsAdded, MultiSelectComboBox control)
		{
			foreach (var item in basedOn)
			{
				if (AddSelectedItem(to, item, basedOn))
				{
					itemsAdded.Add(item);
					if (control.SelectionMode == SelectionModes.Single)
					{
						control._previousSelectedValue = item;
					}
				}
			}
		}

		private static bool AddSelectedItem(IList to, object item, IList sourceList = null)
		{
			if (to.Contains(item))
			{
				return false;
			}
			var insert_at = sourceList?.IndexOf(item) ?? -1;
			if (insert_at == -1)
				insert_at = to.Count;
			if (to.Count < insert_at)
				insert_at = to.Count;

			if (to.Count > 0 && insert_at > 0 && to[insert_at - 1] == null)
			{
				insert_at--;
			}
			if (insert_at == to.Count)
			{
				to.Add(item);
			}
			else
				to.Insert(insert_at, item);

			return true;
		}

		private static void SelectedItemPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			if (dependencyObject is MultiSelectComboBox control)
			{
				control.SelectedItemPropertyChangedCallback();
			}
		}

		private void SelectedItemPropertyChangedCallback()
		{
			if (_isHandlingSelectedItemInternally || SelectedItems == null)
			{
				return;
			}
			var selectedItem = SelectedItem;
			foreach (var item in SelectedItems.Cast<object>().Where(i => i != selectedItem).ToArray())
			{
				SelectedItems.Remove(item);
			}
			if (selectedItem != null && !SelectedItems.Contains(selectedItem))
			{
				SelectedItems.Add(selectedItem);
			}
		}

		private static void SelectedItemsPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			if (dependencyObject is MultiSelectComboBox control)
			{
				control.SelectedItemsPropertyChangedCallback();
			}
		}

		private void SelectedItemsPropertyChangedCallback()
		{
			CleanUpSelectedItemsNotifyingCollection();
			HandleSelectedItemsChanged();
			InitializeSelectedItemsNotifyingCollection();
		}

		private void CleanUpSelectedItemsNotifyingCollection()
		{
			if (_selectedItemsNotifyingCollection != null)
			{
				_selectedItemsNotifyingCollection.CollectionChanged -= SelectedItemsNotifyingCollection_CollectionChanged;
			}
			_selectedItemsNotifyingCollection = null;
		}

		private void InitializeSelectedItemsNotifyingCollection()
		{
			_selectedItemsNotifyingCollection = SelectedItems as INotifyCollectionChanged;
			if (_selectedItemsNotifyingCollection != null)
			{
				_selectedItemsNotifyingCollection.CollectionChanged += SelectedItemsNotifyingCollection_CollectionChanged;
			}
		}

		private INotifyCollectionChanged _selectedItemsNotifyingCollection;

		private void SelectedItemsNotifyingCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// Allow client code to perform multiple changes and handle them only once at the end
			if (!_isWaitingToHandleSelectedItemsChanged)
			{
				_isWaitingToHandleSelectedItemsChanged = true;
#if WINUI
				DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
				{
					HandleSelectedItemsChanged(e.Action == NotifyCollectionChangedAction.Move ? e.OldItems : null);
					_isWaitingToHandleSelectedItemsChanged = false;
				});
#else
				Dispatcher.BeginInvoke((Action)delegate
				{
					HandleSelectedItemsChanged(e.Action == NotifyCollectionChangedAction.Move ? e.OldItems : null);
					_isWaitingToHandleSelectedItemsChanged = false;
				}, DispatcherPriority.ContextIdle);
#endif
			}
		}

		private bool _isWaitingToHandleSelectedItemsChanged;
		private bool _isHandlingSelectedItemInternally;

		private void HandleSelectedItemsChanged(IList items_if_only_move = null)
		{
			if (SelectedItems == null)
			{
				return;
			}

			var itemsAdded = new Collection<object>();
			var itemsRemoved = new Collection<object>();

			RemoveSelectedItems(SelectedItemsInternal, SelectedItems, ref itemsRemoved);
			if (items_if_only_move != null)
			{
				foreach (var itm in items_if_only_move)
				{
					if (SelectedItemsInternal.Remove(itm))
						itemsRemoved.Add(itm);
				}
			}
			AddSelectedItems(SelectedItemsInternal, SelectedItems, ref itemsAdded, this);

			ToggleDropdownListItemsCheckState(itemsRemoved, false);
			ToggleDropdownListItemsCheckState(itemsAdded, true);
			_isHandlingSelectedItemInternally = true;
			SelectedItem = SelectedItems.Cast<object>().FirstOrDefault();
			_isHandlingSelectedItemInternally = false;

			if (itemsAdded.Count > 0 || itemsRemoved.Count > 0)
			{
				RaiseSelectedItemsChangedEvent(itemsAdded, itemsRemoved, SelectedItemsInternal.Where(a => a != null).ToList());
			}
		}

		private void UpdateSelectedItemsContainer(IList comboBoxItems) 
        {
            // Renamed internal logic to SyncContainersFromSelection but kept this method signature for compatibility if needed.
            // But logic is now: sync visuals FROM selection.
            SyncContainersFromSelection();
        }

		private void SyncContainersFromSelection()
		{
			if (DropdownListBox == null || ItemsSource == null) return;

			foreach (var item in ItemsSource)
			{
				var container = GetListViewItem(item); // This will be null for off-screen items
				if (container != null)
				{
					bool isSelected = IsSelectedItem(item);
					if (container.IsChecked != isSelected)
					{
						container.IsChecked = isSelected;
					}
				}
			}
            AddFilterPlaceholderIfNeeded();
		}

		private void ConfigureSingleSelectionMode(ref Collection<object> itemsRemoved)
		{
			if (SelectionMode != SelectionModes.Single || SelectedItemsInternal.Count(a => a != null) <= 1)
			{
				return;
			}

			var lastSelectedItem = SelectedItemsInternal.LastOrDefault(a => a != null);

			for (var i = SelectedItemsInternal.Count - 1; i >= 0; i--)
			{
				var selectedComboBoxItem = SelectedItemsInternal[i];
				if (selectedComboBoxItem == null || selectedComboBoxItem == lastSelectedItem)
					continue;
				var selectedListBoxItem = GetListViewItem(selectedComboBoxItem);
				if (selectedListBoxItem != null)
				{
					selectedListBoxItem.IsChecked = false;
				}

				SelectedItemsInternal.RemoveAt(i);
				itemsRemoved.Add(selectedComboBoxItem);
			}
		}

		private void AttemptToRemoveSelectedItem(object comboBoxItem)
		{
			var listBoxItem = GetListViewItem(comboBoxItem);
			if (listBoxItem != null)
			{
				listBoxItem.IsChecked = false;
			}

			if (IsDropDownOpen && listBoxItem != null)
			{
				UpdateSelectedItemsContainer(ItemsSource);
			}
			else
			{
				SelectedItemsInternal.Remove(comboBoxItem);

				var selectedItems = SelectedItemsInternal.Where(a => a != null).ToList();
				UpdateSelectedItems(selectedItems);

				RaiseSelectedItemsChangedEvent(new List<object>(), new List<object> { comboBoxItem }, selectedItems);
			}
		}

		private void UpdateSelectedItems(IList selectedItems)
		{
			if (SelectedItems != null)
			{
				for (var i = SelectedItems.Count - 1; i >= 0; i--)
				{
					if (!selectedItems.Contains(SelectedItems[i]))
					{
						SelectedItems.RemoveAt(i);
					}
				}

				foreach (var item in selectedItems)
				{
					if (!SelectedItems.Contains(item))
					{
						SelectedItems.Add(item);
						if (SelectionMode == SelectionModes.Single && _previousSelectedValue != item)
						{
							_previousSelectedValue = item;
						}
					}
				}
			}
		}

		private void ToggleDropdownListItemsCheckState(IList items, bool isChecked)
		{
			var listItems = items
				.Cast<object>()
				.Select(GetListViewItem)
				.Where(e => e != null && (e as IItemEnabledAware)?.IsEnabled != false);

			foreach (var item in listItems)
			{
				item.IsChecked = isChecked;
			}
		}

		private void AddFilterPlaceholderIfNeeded()
		{
			if (!SelectedItemsInternal.Contains(null))
			{
				SelectedItemsInternal.Add(null);
			}
		}

		#endregion

		#region Event Raising

		private void RaiseNewItemAddRequestEvent(string TypedText)
		{
#if WINUI
			DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
			{
				var args = new NewItemAddRequestEventArgs(TypedText);
				NewItemAddRequest?.Invoke(this, args);
			});
#else
			Dispatcher.BeginInvoke(new Action(
				delegate
				{
					var args = new NewItemAddRequestEventArgs(NewItemAddRequestEvent, TypedText);
					RaiseEvent(args);
				}));
#endif
		}

		private void RaiseItemDeleteRequestEvent(ICollection items)
		{
#if WINUI
			DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
			{
				var args = new ItemDeleteRequestEventArgs(items);
				ItemDeleteRequest?.Invoke(this, args);
			});
#else
			Dispatcher.BeginInvoke(new Action(
				delegate
				{
					var args = new ItemDeleteRequestEventArgs(ItemDeleteRequestEvent, items);
					RaiseEvent(args);
				}));
#endif
		}

		private void RaiseFilterTextChangedEvent()
		{
#if WINUI
			DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
			{
				var args = new FilterTextChangedEventArgs(SelectedItemsFilterTextBox?.Text, DropdownListBox?.Items.Cast<object>().ToList());
				FilterTextChanged?.Invoke(this, args);
			});
#else
			Dispatcher.BeginInvoke(new Action(
				delegate
				{
					var args = new FilterTextChangedEventArgs(FilterTextChangedEvent, SelectedItemsFilterTextBox?.Text, DropdownListBox?.Items.Cast<object>().ToList());
					RaiseEvent(args);
				}));
#endif
		}

		private void RaiseSelectedItemsChangedEvent(ICollection added, ICollection removed, ICollection selected)
		{
#if WINUI
			DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
			{
				var args = new SelectedItemsChangedEventArgs(added, removed, selected);
				SelectedItemsChanged?.Invoke(this, args);
			});
#else
			Dispatcher.BeginInvoke(new Action(
				delegate
				{
					var args = new SelectedItemsChangedEventArgs(SelectedItemsChangedEvent, added, removed, selected);
					RaiseEvent(args);
				}));
#endif
		}

		#endregion

		#region Keyboard and Mouse Handling

		private void MultiSelectComboBoxKeyUp(object sender,
#if WINUI
			KeyRoutedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine($"[WINUI] KeyUp: {e.Key}");
			if (e.Key != VirtualKey.Down && e.Key != VirtualKey.Up)
#else
			KeyEventArgs e)
		{
			if (e.Key != Key.Down && e.Key != Key.Up)
#endif
			{
				return;
			}

			System.Diagnostics.Debug.WriteLine($"[WINUI] Calling OpenDropDownList");
			OpenDropDownList();
		}

#if !WINUI
		private void OpenDropDownListCommandExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			if (!IsEditMode && IsEditable)
			{
				AssignIsEditMode();
			}

			OpenDropDownList();
		}

		private static ICommand OpenDropDownListCommand = new RoutedCommand();
#endif

		private void OpenDropDownList()
		{
#if WINUI
			System.Diagnostics.Debug.WriteLine($"[WINUI] OpenDropDownList - DropdownListBox: {(DropdownListBox != null ? $"Found with {DropdownListBox.Items.Count} items" : "NULL")}");
			if (DropdownListBox == null)
#else
			if (DropdownListBox == null || DropdownListBox.IsKeyboardFocusWithin)
#endif
			{
#if WINUI
				System.Diagnostics.Debug.WriteLine("[WINUI] OpenDropDownList - Returning early, DropdownListBox is NULL");
#endif
				return;
			}

			IsDropDownOpen = true;
#if WINUI
			System.Diagnostics.Debug.WriteLine($"[WINUI] OpenDropDownList - Set IsDropDownOpen = true");
#endif

			if (DropdownListBox.Items.Count > 0)
			{
#if WINUI
				System.Diagnostics.Debug.WriteLine($"[WINUI] OpenDropDownList - Setting focus on first item");
#endif
				SetVisualFocusOnItem(DropdownListBox.SelectedItem);

#if WINUI
				DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
				{
					SetKeyBoardFocusOnItem(DropdownListBox.SelectedItem);
				});
#else
				Dispatcher.BeginInvoke(DispatcherPriority.Input,
					new Action(delegate
					{
						SetKeyBoardFocusOnItem(DropdownListBox.SelectedItem);
					}));
#endif
			}
		}

#if WINUI
		private void MultiSelectComboBoxOnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine($"[WINUI] PointerPressed - IsEditMode: {IsEditMode}, OpenDropDownListAlsoWhenNotInEditMode: {OpenDropDownListAlsoWhenNotInEditMode}");
			if (!IsEditMode && OpenDropDownListAlsoWhenNotInEditMode == false)
			{
				e.Handled = true;
			}
			else
			{
				System.Diagnostics.Debug.WriteLine($"[WINUI] PointerPressed - Toggling IsDropDownOpen from {IsDropDownOpen} to {!IsDropDownOpen}");
				IsDropDownOpen = !IsDropDownOpen;

				if (!IsDropDownOpen)
				{
					UpdateAutoCompleteFilterText(FilterTextApplied, null);
				}
			}

			AssignIsEditMode();
		}
#else
		private void MultiSelectComboBoxOnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (IsScrollBar(e) || IsRemoveItemButton(e) || IsComboBoxItemDataContext(e))
			{
				if (IsComboBoxItemDataContext(e))
				{
					UpdateAutoCompleteFilterText(FilterTextApplied, null);
				}
				return;
			}

			if (!IsEditMode && OpenDropDownListAlsoWhenNotInEditMode == false)
			{
				e.Handled = true;
			}
			else if (IsDropdownButton(e) || IsItemsControl(e))
			{
				if (ClearFilterOnDropdownClosing && DropdownListBox != null && DropdownListBox.IsKeyboardFocusWithin)
				{
					CloseDropdownMenu(true, false);
				}
				else
				{
					IsDropDownOpen = !IsDropDownOpen;

					if (!IsDropDownOpen)
					{
						UpdateAutoCompleteFilterText(FilterTextApplied, null);
					}
				}

				e.Handled = true;
			}

			AssignIsEditMode();
		}
#endif

		private void MultiSelectComboBoxGotFocus(object sender, RoutedEventArgs e)
		{
#if WINUI
			System.Diagnostics.Debug.WriteLine("[WINUI] MultiSelectComboBox GotFocus");
#endif
			MultiSelectComboBoxHasFocus = true;
		}

		private void MultiSelectComboBoxLostFocus(object sender, RoutedEventArgs e)
		{
#if WINUI
			System.Diagnostics.Debug.WriteLine("[WINUI] MultiSelectComboBox LostFocus");
#endif
			MultiSelectComboBoxHasFocus = false;

			if (IsEditable)
			{
				AttemptToCloseEditMode();
			}
		}

#if !WINUI
		private void SelectedItemsControl_CurrentChanged(object sender, System.EventArgs e)
		{
			FocusCursorOnFilterTextBox();
		}

		private void SelectedItemsControl_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (IsScrollBar(e))
			{
				return;
			}

			if (IsEditMode)
			{
				if (IsEditable && IsRemoveItemButton(e))
				{
					var element = e.OriginalSource as FrameworkElement;
					if (element?.DataContext is object item)
					{
						if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && EnableDeleteItemUI)
						{
							RaiseItemDeleteRequestEvent(new Collection<object>() { item });
							e.Handled = true;
						}
						else
							AttemptToRemoveSelectedItem(item);
					}
				}
				else
				{
					IsDropDownOpen = !IsDropDownOpen;
				}
			}

			AssignIsEditMode();
		}
#else
		private void SelectedItemsControl_OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			if (IsEditMode)
			{
				IsDropDownOpen = !IsDropDownOpen;
			}

			AssignIsEditMode();
		}
#endif

		private void SelectedItemsControl_OnKeyUp(object sender,
#if WINUI
			KeyRoutedEventArgs e)
		{
			if (e.Key == VirtualKey.Shift || e.Key == VirtualKey.Tab)
				return;
			if (e.OriginalSource is TextBox textBox && IsEditMode)
			{
				var previousFilterText = FilterTextApplied;
				FilterTextApplied = textBox.Text.Trim();
				textBox.Focus(FocusState.Programmatic);

				if (e.Key == VirtualKey.Delete)
				{
					if (!string.IsNullOrEmpty(FilterTextApplied))
					{
						textBox.Text = string.Empty;
						FilterTextApplied = string.Empty;
						UpdateItems(string.Empty);
					}
					else if (IsEditable)
					{
						UnSelectComboBoxItem();
					}
				}
				else if (e.Key == VirtualKey.Back && string.IsNullOrEmpty(previousFilterText))
				{
					if (IsEditable)
					{
						UnSelectComboBoxItem();
					}
				}
				else if (e.Key == VirtualKey.Enter)
				{
					if (IsDropDownOpen)
					{
						SelectComboBoxItem();
						IsDropDownOpen = false;
					}

					SelectedItemsFilterTextBox.Text = string.Empty;
					FilterTextApplied = string.Empty;
					UpdateItems(string.Empty);
				}
				else if (e.Key == VirtualKey.Escape)
				{
					IsDropDownOpen = false;
					UpdateAutoCompleteFilterText(string.Empty, null);
				}
				else
				{
					UpdateItems(textBox.Text);

					if (!IsDropDownOpen && EnableFiltering)
					{
						IsDropDownOpen = true;
					}
				}
			}
		}
#else
			KeyEventArgs e)
		{
			if (e.Key == Key.RightShift || e.Key == Key.LeftShift || e.Key == Key.Tab)
				return;
			if (e.OriginalSource is TextBox textBox && IsEditMode)
			{
				var perviousFilterText = FilterTextApplied;
				FilterTextApplied = textBox.Text.Trim();
				textBox.Focus();

				switch (e.Key)
				{
					case Key.Delete:
					case Key.Back when textBox.CaretIndex == 0 && string.IsNullOrEmpty(perviousFilterText):
						if (e.Key == Key.Delete && !string.IsNullOrEmpty(FilterTextApplied))
						{
							textBox.Text = string.Empty;
							FilterTextApplied = string.Empty;

							UpdateItems(string.Empty);
						}
						else if (IsEditable)
						{
							UnSelectComboBoxItem();
						}
						break;
					case Key.Return:
						if (IsDropDownOpen)
						{
							SelectComboBoxItem();
							IsDropDownOpen = false;
						}

						SelectedItemsFilterTextBox.Text = string.Empty;
						FilterTextApplied = string.Empty;

						UpdateItems(string.Empty);
						break;
					case Key.Escape:
						IsDropDownOpen = false;
						UpdateAutoCompleteFilterText(string.Empty, null);
						break;
					default:
						UpdateItems(textBox.Text);

						if (!IsDropDownOpen && EnableFiltering)
						{
							IsDropDownOpen = true;
						}
						break;
				}
			}
		}
#endif

		#endregion

		#region Dropdown Event Handlers

		private void DropdownMenuClosed(object sender,
#if WINUI
			object e)
#else
			System.EventArgs e)
#endif
		{
#if !WINUI
			if (IgnoreDropdownClosingFocusPlanUntil > DateTime.Now)
			{
				IgnoreDropdownClosingFocusPlanUntil = null;
				return;
			}
#endif
			FocusCursorOnFilterTextBox();
		}

		private void DropdownMenuOpened(object sender,
#if WINUI
			object e)
#else
			System.EventArgs e)
#endif
		{
			if (SelectedItems?.Count > 0 && SetFocusOnFirstSelectedItemOnDropDown)
			{
				SetVisualFocusOnItem(SelectedItems[0]);
			}
			
			if (DropdownListBox?.Items.Count > 0)
				{
					// Only set focus if we are NOT editing text (filter mode)
					if (!IsEditMode)
					{
						SetVisualFocusOnItem(DropdownListBox.Items[0]);
					}
				}
#if !WINUI
			Mouse.Capture(this, CaptureMode.SubTree);
#endif
		}

#if !WINUI
		private void ControlWindowLocationChanged(object sender, System.EventArgs e)
		{
			ResetDropdownMenu();
		}

		private void ControlWindowDeactivated(object sender, System.EventArgs e)
		{
			if (DropdownMenu != null)
			{
				DropdownMenu.IsOpen = false;
			}
		}
#endif

		private void MultiSelectComboBoxGridSizeChanged(object sender, SizeChangedEventArgs e)
		{
			ResetDropdownMenu();
		}

		private void DropdownListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((SelectionMode == SelectionModes.Single && SelectedItems != null && SelectedItems.Count == 0 && DisableFilterUpdateOnDropDownItemSelectionChange) || !DisableFilterUpdateOnDropDownItemSelectionChange)
			{
				if (e.AddedItems.Count > 0 && e.AddedItems[0] is object comboBoxItemAdded)
				{
					UpdateAutoCompleteFilterText(FilterTextApplied, comboBoxItemAdded);
				}
				else if (e.RemovedItems.Count > 0 && e.RemovedItems[0] is object comboBoxItemRemoved)
				{
					UpdateAutoCompleteFilterText(FilterTextApplied, comboBoxItemRemoved);
				}
			}
		}

#if WINUI
		private void DropdownListBoxKeyDown(object sender, KeyRoutedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine($"[WINUI] DropdownListBoxKeyDown Key={e.Key}");
			if (DropdownListBox != null && DropdownListBox.SelectedItem is object item)
			{
				if (e.Key == VirtualKey.Space)
				{
					var listBoxItem = GetListViewItem(item);
					listBoxItem.IsChecked = !listBoxItem.IsChecked;
					UpdateSelectedItemsContainer(ItemsSource);
				}
				else if (e.Key == VirtualKey.Enter)
				{
					SelectComboBoxItem();
					IsDropDownOpen = false;

					if (SelectedItemsFilterTextBox != null)
						SelectedItemsFilterTextBox.Text = string.Empty;
					FilterTextApplied = string.Empty;
					UpdateItems(string.Empty);
				}
				else if (e.Key == VirtualKey.Escape)
				{
					if (ClearFilterOnDropdownClosing)
					{
						CloseDropdownMenu(true, false);
					}
					else
					{
						IsDropDownOpen = false;
					}
				}
			}
		}

		private void DropdownListBoxItemClick(object sender, ItemClickEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine($"[WINUI] DropdownListBoxItemClick - Item: {e.ClickedItem}");
			if (e.ClickedItem != null)
			{
				ToggleItemSelection(e.ClickedItem);

				if (SelectionMode == SelectionModes.Single)
				{
					IsDropDownOpen = false;
				}
			}
		}

		private void ToggleItemSelection(object item)
		{
			var itemsAdded = new Collection<object>();
			var itemsRemoved = new Collection<object>();

			if (IsSelectedItem(item))
			{
				if (RemoveSelectedItem(SelectedItemsInternal, -1, SelectedItems)) // index -1 to use remove by object
				{
					SelectedItemsInternal.Remove(item); // Ensure removal
					itemsRemoved.Add(item);
				}
			}
			else
			{
				if (SelectionMode == SelectionModes.Single)
				{
					// Clear others
					foreach (var selected in SelectedItemsInternal.ToList())
					{
						if (selected != item) 
						{
							SelectedItemsInternal.Remove(selected);
							itemsRemoved.Add(selected);
						}
					}
				}

				if (AddSelectedItem(SelectedItemsInternal, item))
				{
					itemsAdded.Add(item);
				}
			}

			// Update Visuals
			SyncContainersFromSelection();

			if (itemsAdded.Count > 0 || itemsRemoved.Count > 0)
			{
				RaiseSelectedItemsChangedEvent(itemsAdded, itemsRemoved, SelectedItemsInternal.Where(a => a != null).ToList());
				UpdateSelectedItems(SelectedItemsInternal); // Sync public property
			}
            
            AddFilterPlaceholderIfNeeded();
		}
#if WINUI
		private void DropdownListBox_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (args.Item != null && args.ItemContainer is ExtendedListBoxItem container)
			{
				container.IsChecked = IsSelectedItem(args.Item);
			}
		}
#endif

#else
		private void DropdownListBoxPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (DropdownListBox != null && DropdownListBox.SelectedItem is object item)
			{
				switch (e.Key)
				{
					case Key.Space:
						var listBoxItem = GetListViewItem(item);
						listBoxItem.IsChecked = !listBoxItem.IsChecked;

						UpdateSelectedItemsContainer(ItemsSource);

						break;
					case Key.Return:
						SelectComboBoxItem();
						IsDropDownOpen = false;

						if (SelectedItemsFilterTextBox != null)
							SelectedItemsFilterTextBox.Text = string.Empty;
						FilterTextApplied = string.Empty;

						UpdateItems(string.Empty);

						break;
					case Key.Escape:
						if (ClearFilterOnDropdownClosing && DropdownListBox != null && DropdownListBox.IsKeyboardFocusWithin)
						{
							CloseDropdownMenu(true, false);
						}
						else
						{
							IsDropDownOpen = false;
						}

						break;
				}

				if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 && (e.Key == Key.Down || e.Key == Key.Up))
				{
					var originalSource = e.OriginalSource as FrameworkElement;
					if (originalSource?.DataContext is object comboBoxItem)
					{
						var lbi = GetListViewItem(comboBoxItem);
						lbi.IsChecked = !lbi.IsChecked;

						UpdateSelectedItemsContainer(ItemsSource);

					}
				}
			}
		}

		private void DropdownListBoxPreviewMouseUp(object sender, MouseButtonEventArgs e)
		{
			var originalSource = e.OriginalSource as FrameworkElement;
			if (DropdownListBox.SelectedItem is object comboBoxItemFrom && (Keyboard.Modifiers & ModifierKeys.Shift) != 0)
			{
				if (originalSource?.DataContext is object comboBoxItemTo)
				{
					var listBoxItemFrom = (IInputElement)DropdownListBox.ItemContainerGenerator.ContainerFromItem(comboBoxItemFrom);
					var listBoxItemTo = (IInputElement)DropdownListBox.ItemContainerGenerator.ContainerFromItem(comboBoxItemTo);

					if (listBoxItemFrom != null && listBoxItemTo != null)
					{
						var itemIndexFrom = -1;
						var itemIndexTo = -1;

						GetComboBoxItemIndexes(comboBoxItemFrom, ref itemIndexFrom, comboBoxItemTo, ref itemIndexTo);

						if (itemIndexTo > itemIndexFrom && itemIndexTo - itemIndexFrom > 1)
						{
							for (var i = itemIndexFrom + 1; i <= itemIndexTo - 1; i++)
							{
								if (DropdownListBox.Items[i] is object itm)
								{
									var lbi = GetListViewItem(itm);
									if (lbi != null)
									{
										lbi.IsChecked = !lbi.IsChecked;
									}
								}
							}
						}
						else if (itemIndexFrom - itemIndexTo > 1)
						{
							for (var i = itemIndexFrom - 1; i >= itemIndexTo + 1; i--)
							{
								if (DropdownListBox.Items[i] is object itm)
								{
									var lbi = GetListViewItem(itm);
									if (lbi != null)
									{
										lbi.IsChecked = !lbi.IsChecked;
									}
								}
							}
						}
					}

					SetKeyBoardFocusOnItem(comboBoxItemTo);
					UpdateSelectedItemsContainer(ItemsSource);
				}
			}

			if (originalSource?.DataContext is object comboBoxItem)
			{
				var listBoxItem = GetListViewItem(comboBoxItem);
				if (listBoxItem != null)
				{
					if (SelectionMode != SelectionModes.Single || !listBoxItem.IsChecked)
					{
						listBoxItem.IsChecked = !listBoxItem.IsChecked;

						SetKeyBoardFocusOnItem(comboBoxItem);
						UpdateSelectedItemsContainer(ItemsSource);
					}

					if (SelectionMode == SelectionModes.Single)
					{
						CloseDropdownMenu(true, false);
					}
				}
			}
		}

		private void DropDownListBoxItemContainerGenerator_StatusChanged(object sender, System.EventArgs e)
		{
			foreach (var item in SelectedItemsInternal)
			{
				if (item != null && _dropdownListBox.ItemContainerGenerator.ContainerFromItem(item) is ExtendedListBoxItem listBoxItem)
				{
					listBoxItem.IsChecked = true;
				}
			}
		}

		private void GetComboBoxItemIndexes(object comboBoxItemFrom, ref int itemIndexFrom, object comboBoxItemTo, ref int itemIndexTo)
		{
			for (var i = 0; i < DropdownListBox.Items.Count; i++)
			{
				if (!(DropdownListBox.Items[i] is object item))
				{
					continue;
				}

				if (item.Equals(comboBoxItemFrom))
				{
					itemIndexFrom = i;
				}
				else if (item.Equals(comboBoxItemTo))
				{
					itemIndexTo = i;
				}
			}
		}
#endif

		private void SetKeyBoardFocusOnItem(object comboBoxItem)
		{
			if (comboBoxItem != null)
			{
#if WINUI
				DropdownListBox.SelectedItem = comboBoxItem;
				var listBoxItem = DropdownListBox?.ContainerFromItem(comboBoxItem) as Control;
				listBoxItem?.Focus(FocusState.Keyboard);
#else
				ItemsCollectionViewSource.View.MoveCurrentTo(comboBoxItem);
				DropdownListBox.Items.MoveCurrentTo(comboBoxItem);

				var listBoxItemTo = (IInputElement)DropdownListBox.ItemContainerGenerator.ContainerFromItem(comboBoxItem);
				if (listBoxItemTo != null)
				{
					listBoxItemTo.Focus();
					DropdownListBox.SelectedItem = listBoxItemTo;
				}
#endif
			}
		}

		private void SetVisualFocusOnItem(object comboBoxItem)
		{
			if (DropdownListBox?.Items.Count > 0)
			{
#if WINUI
				DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
				{
					var isEnableAware = comboBoxItem is IItemEnabledAware;
					if (isEnableAware)
					{
						DropdownListBox.SelectedItem = ((IItemEnabledAware)comboBoxItem).IsEnabled
							? comboBoxItem
							: DropdownListBox.Items.Cast<object>().FirstOrDefault(a => ((IItemEnabledAware)a).IsEnabled);
					}
					else
					{
						DropdownListBox.SelectedItem = comboBoxItem;
					}

					var selectedItem = DropdownListBox.SelectedItem;
					if (selectedItem != null)
					{
						DropdownListBox.ScrollIntoView(selectedItem);
						UpdateAutoCompleteFilterText(FilterTextApplied, comboBoxItem);
					}
				});
#else
				Dispatcher.BeginInvoke(DispatcherPriority.Input,
					new Action(delegate
					{
						var isEnableAware = comboBoxItem is IItemEnabledAware;
						if (isEnableAware)
						{
							DropdownListBox.SelectedItem = ((IItemEnabledAware)comboBoxItem).IsEnabled
								? comboBoxItem
								: DropdownListBox.Items.Cast<object>().FirstOrDefault(a => ((IItemEnabledAware)a).IsEnabled);
						}
						else
						{
							DropdownListBox.SelectedItem = comboBoxItem;
						}


						if (DropdownListBox.SelectedItem == null)
						{
							ItemsCollectionViewSource.View.MoveCurrentTo(comboBoxItem);
							DropdownListBox.Items.MoveCurrentTo(comboBoxItem);
						}
						var selectedItem = DropdownListBox.SelectedItem;
						if (selectedItem != null)
						{
							DropdownListBox.ScrollIntoView(selectedItem);

							UpdateAutoCompleteFilterText(FilterTextApplied, comboBoxItem);
						}
					}));
#endif
			}
		}

		#endregion

		#region Filter and AutoComplete

#if WINUI
		private void SelectedItemsFilterTextBoxBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
		{
			if (!EnableFiltering && !string.IsNullOrEmpty(args.NewText))
			{
				args.Cancel = true;
			}
		}
#else
		private void SelectedItemsFilterTextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (!EnableFiltering && !string.IsNullOrEmpty(e.Text))
			{
				e.Handled = true;
			}
		}
#endif

		private void SelectedItemsFilterTextBoxTextChanged(object sender, TextChangedEventArgs e)
		{
#if WINUI
			var textBox = sender as TextBox;
			var criteria = textBox?.Text;
			System.Diagnostics.Debug.WriteLine($"[WINUI] TextChanged: '{criteria}' (TextBox: {textBox != null})");
#else
			var criteria = ((TextBox)e.OriginalSource).Text;
#endif

			if (ClearSelectionOnFilterChanged && !string.IsNullOrEmpty(criteria) && SelectionMode == SelectionModes.Single && SelectedItems != null)
			{
				SelectedItems.Clear();
				SetValue(SelectedItemsProperty, SelectedItems);
			}

			UpdateAutoCompleteFilterText(criteria, DropdownListBox != null && DropdownListBox.Items.Count > 0 ? DropdownListBox.Items[0] : null);
		}

		private void ResetDropdownMenu()
		{
			if (DropdownMenu == null)
			{
				return;
			}

			var offset = DropdownMenu.HorizontalOffset;
			DropdownMenu.HorizontalOffset = offset + 0.001;
			DropdownMenu.HorizontalOffset = offset;
		}

		private CancellationTokenSource _suggestionProviderToken;

		private void UpdateItems(string criteria)
		{
#if WINUI
			System.Diagnostics.Debug.WriteLine($"[WINUI] UpdateItems called with criteria: '{criteria}', SuggestionProvider: {(SuggestionProvider != null ? "exists" : "NULL")}");
#endif
			if (SuggestionProvider == null)
			{
				ApplyItemsFilter(criteria);
				return;
			}
			IsLoadingSuggestions = true;
			LoadSuggestionsAsync(criteria).ContinueWith(t => IsLoadingSuggestions = false, TaskContinuationOptions.ExecuteSynchronously);
		}

		private async Task LoadSuggestionsAsync(string criteria)
		{
			var suggestionProvider = SuggestionProvider;
			_suggestionProviderToken?.Cancel(true);

			var suggestionProviderToken = _suggestionProviderToken = new CancellationTokenSource();
			var items = await suggestionProvider.GetSuggestionsAsync(criteria, _suggestionProviderToken.Token);
#if WINUI
			System.Diagnostics.Debug.WriteLine($"[WINUI] LoadSuggestionsAsync: Got {items?.Count ?? 0} items from provider");
#endif
#if WINUI
			DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
			{
				if (suggestionProviderToken.IsCancellationRequested)
				{
					return;
				}

				ItemsSource.Clear();
				foreach (var item in items)
				{
					ItemsSource.Add(item);
				}

				if (!suggestionProviderToken.IsCancellationRequested)
				{
					ApplyItemsFilter(criteria);
				}
			});
#else
			await Dispatcher.BeginInvoke(new Action(() =>
			{
				if (suggestionProviderToken.IsCancellationRequested)
				{
					return;
				}

				ItemsSource.Clear();
				foreach (var item in items)
				{
					ItemsSource.Add(item);
				}

				if (!suggestionProviderToken.IsCancellationRequested)
				{
					ApplyItemsFilter(criteria);
				}
			}));
#endif
		}

		private void ApplyItemsFilter(string criteria)
		{
			System.Diagnostics.Debug.WriteLine($"[WINUI] ApplyItemsFilter called - Criteria: '{criteria}', EnableFiltering: {EnableFiltering}");
#if WINUI
			// WinUI uses different filtering approach - direct filtering on ItemsSource or CollectionViewSource
			if (EnableFiltering)
			{
				CurrentFilterService = FilterService ?? new DefaultFilterService();
				CurrentFilterService.SetFilter(criteria);

				// For WinUI, we need to filter manually or use CollectionViewSource
				if (DropdownListBox != null && ItemsSource != null)
				{
					var filteredItems = ItemsSource.Cast<object>()
						.Where(item => CurrentFilterService.Filter == null || CurrentFilterService.Filter(item))
						.ToList();

					DropdownListBox.ItemsSource = filteredItems;
					System.Diagnostics.Debug.WriteLine($"[WINUI] Filtered {ItemsSource.Count} items down to {filteredItems.Count} items");
				}
				else
				{
					System.Diagnostics.Debug.WriteLine($"[WINUI] WARNING: DropdownListBox={DropdownListBox != null}, ItemsSource={ItemsSource != null}");
				}

				if (DropdownListBox?.Items.Count > 0)
				{
					var item = DropdownListBox.Items[0];
					System.Diagnostics.Debug.WriteLine($"[WINUI] First filtered item: {item}");
					SetVisualFocusOnItem(item);
					UpdateAutoCompleteFilterText(criteria, item);
				}
				else
				{
					System.Diagnostics.Debug.WriteLine("[WINUI] No items after filtering");
					UpdateAutoCompleteFilterText(criteria, null);
				}

				RaiseFilterTextChangedEvent();
			}
			else
			{
				UpdateAutoCompleteFilterText(criteria, null);
			}
#else
			if (EnableFiltering && ItemsCollectionViewSource?.View != null)
			{
				ItemsCollectionViewSource.View.Filter = CurrentFilterService.Filter;
				CurrentFilterService.SetFilter(criteria);

				ItemsCollectionViewSource.View.Refresh();

				if (DropdownListBox?.Items.Count > 0)
				{
					var item = DropdownListBox.Items[0];
					SetVisualFocusOnItem(item);

					UpdateAutoCompleteFilterText(criteria, item);
				}
				else
				{
					UpdateAutoCompleteFilterText(criteria, null);
				}

				RaiseFilterTextChangedEvent();
			}
			else
			{
				UpdateAutoCompleteFilterText(criteria, null);
			}
#endif
		}

		private void UpdateAutoCompleteFilterText(string criteria, object item)
		{
			System.Diagnostics.Debug.WriteLine($"[WINUI] UpdateAutoCompleteFilterText called - Criteria: '{criteria}', Item: {item != null}, IsDropDownOpen: {IsDropDownOpen}");
			if (SelectedItemsFilterAutoCompleteTextBox == null)
			{
				System.Diagnostics.Debug.WriteLine("[WINUI] SelectedItemsFilterAutoCompleteTextBox is NULL. Retrying find...");
				_selectedItemsFilterAutoCompleteTextBox = null; // Force re-find
				if (SelectedItemsFilterAutoCompleteTextBox == null)
				{
					System.Diagnostics.Debug.WriteLine("[WINUI] SelectedItemsFilterAutoCompleteTextBox still NULL after retry.");
					
					// Force realization of the template if possible?
					// In WinUI, we might need to wait for layout.
					// Let's try to search the specific container if we can find it.
					if (SelectedItemsControl != null && SelectedItems != null)
					{
						var lastItem = SelectedItems.Cast<object>().LastOrDefault();
						if (lastItem == null) // This is our search item (null placeholder)
						{
#if ! WINUI
							var container = SelectedItemsControl.ItemContainerGenerator.ContainerFromItem(lastItem);
#else
							var container = SelectedItemsControl.ContainerFromItem(lastItem); // This might be null if not generated yet
#endif
							if (container is DependencyObject depObj)
							{
								_selectedItemsFilterAutoCompleteTextBox = VisualTreeService.FindVisualChild<TextBox>(depObj, PART_MultiSelectComboBox_SelectedItemsPanel_Filter_AutoComplete_TextBox);
							}
						}
					}
					
					if (SelectedItemsFilterAutoCompleteTextBox == null)
					{
						System.Diagnostics.Debug.WriteLine("[WINUI] VTDUMP: Dumping SelectedItemsControl visual tree:");
						DumpVisualTree(SelectedItemsControl);
						return;
					}
				}
			}

			if (EnableAutoComplete && IsDropDownOpen)
			{
				if (SelectionMode == SelectionModes.Multiple && EnableBatchSelection && TrySelectBatchItemsAsync(criteria))
				{
#if WINUI
					DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
					{
						CloseDropdownMenu(true, true);
						AssignIsEditMode();
					});
#else
					Dispatcher.BeginInvoke((Action)delegate
					{
						CloseDropdownMenu(true, true);
						AssignIsEditMode();
					}, DispatcherPriority.ContextIdle);
#endif
				}
				else if (item != null && !IsSelectedItem(item))
				{
					string autoCompleteString = CurrentAutoCompleteService.GetAutoCompleteString(item) ?? string.Empty;
					var index = criteria?.Length > 0 ? autoCompleteString.IndexOf(criteria, StringComparison.InvariantCultureIgnoreCase) : 0;
					var autoCompleteText = index > -1 ? autoCompleteString.Substring(index + (criteria?.Length ?? 0)) : string.Empty;

					if (AutoCompleteMaxLength > 0 && autoCompleteText.Length >= AutoCompleteMaxLength)
					{
						autoCompleteText = autoCompleteText.Substring(0, AutoCompleteMaxLength) + "...";
					}

					SelectedItemsFilterAutoCompleteTextBox.Text = autoCompleteText;
					if (AutoCompleteBackground != null)
						SelectedItemsFilterAutoCompleteTextBox.Background = AutoCompleteBackground;
					return;
				}
			}

			SelectedItemsFilterAutoCompleteTextBox.Text = string.Empty;
#if WINUI
			SelectedItemsFilterAutoCompleteTextBox.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
#else
			SelectedItemsFilterAutoCompleteTextBox.Background = Brushes.Transparent;
#endif
		}

		private bool TrySelectBatchItemsAsync(string criteria)
		{
			if (string.IsNullOrEmpty(criteria) || ItemsSource == null || SuggestionProvider == null || SelectedItems == null)
			{
				return false;
			}

			var valuesToSelect = criteria.Split(',', ';')
				.Select(i => i.Trim().ToLower())
				.ToArray();

			var suggestionProviderToken = _suggestionProviderToken = new CancellationTokenSource();
			var itemsToSelect = valuesToSelect
				.Select(value => SuggestionProvider.GetSuggestionsAsync(value, _suggestionProviderToken.Token).GetAwaiter().GetResult().FirstOrDefault())
				.Where(item => item != null && valuesToSelect.Contains(item.ToString().ToLower()))
				.ToArray();

			if (itemsToSelect.Length < valuesToSelect.Length)
			{
				return false;
			}

			foreach (var item in itemsToSelect)
			{
				if (!SelectedItems.Contains(item))
				{
					SelectedItems.Add(item);
				}
			}

			return true;
		}

		#endregion

		#region Edit Mode

		private void AssignIsEditMode()
		{
			if (SelectedItemsInternal?.Count == 0)
				SelectedItemsInternal.Add(null);
#if WINUI
			IsEditMode = true;
#else
			SetValue(IsEditModePropertyKey, true);
#endif

			FocusCursorOnFilterTextBox();
		}

		private void UnSelectComboBoxItem()
		{
			if (SelectedItemsInternal?.Count > 1)
			{
				// we take the second last item; understanding that the last item is always the searchable textbox
				var item = SelectedItemsInternal[SelectedItemsInternal.Count - 2];
				if (item != null)
				{
					AttemptToRemoveSelectedItem(item);
				}
			}
		}

		private void SelectComboBoxItem()
		{
			var selectedItem = DropdownListBox.SelectedItem;
			if (selectedItem == null && DropdownListBox.Items.Count > 0)
			{
				selectedItem = DropdownListBox.SelectedItem = DropdownListBox.Items[0];
			}

			if (selectedItem != null)
			{
				var listBoxItem = GetListViewItem(selectedItem);
				if (listBoxItem != null)
					listBoxItem.IsChecked = true;

				UpdateSelectedItemsContainer(ItemsSource);
			}
		}

		private void FocusCursorOnFilterTextBox()
		{
			if (IsEditMode)
			{
#if WINUI
				DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
				{
					if (SelectedItemsControl != null && SelectedItemsFilterTextBox != null)
					{
						SelectedItemsFilterTextBox.Visibility = Visibility.Visible;
						SelectedItemsFilterTextBox.Focus(FocusState.Programmatic);
						// Move caret to end
						SelectedItemsFilterTextBox.SelectionStart = SelectedItemsFilterTextBox.Text.Length;
					}
				});
#else
				Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(
					delegate
					{
						if (SelectedItemsControl != null && SelectedItemsFilterTextBox != null)
						{
							SelectedItemsFilterTextBox.Visibility = Visibility.Visible;
							SelectedItemsFilterTextBox.Focus();
							SelectedItemsFilterTextBox.ForceCursor = true;
							SelectedItemsFilterTextBox.ScrollToEnd();
							SelectedItemsFilterTextBox.CaretIndex = SelectedItemsFilterTextBox.Text.Trim().Length;
						}
					}));
#endif
			}
		}

#if !WINUI
		private bool IsComboBoxItemDataContext(RoutedEventArgs e)
		{
			var inline = e.OriginalSource as FrameworkContentElement;
			if (inline?.DataContext != null)
			{
				return true;
			}

			var source = e.OriginalSource as FrameworkElement;
			if (source?.DataContext != null)
			{
				return true;
			}

			var sourceParent = source?.Parent as FrameworkElement;
			if (sourceParent?.DataContext != null)
			{
				return true;
			}

			return false;
		}

		private bool IsItemsControl(RoutedEventArgs e)
		{
			var itemsControl = VisualTreeService.FindVisualTemplatedParent<ItemsControl>(e.OriginalSource as FrameworkElement, PART_MultiSelectComboBox_SelectedItemsPanel_ItemsControl);
			return itemsControl != null;
		}

		private bool IsDropdownButton(RoutedEventArgs e)
		{
			var button = VisualTreeService.FindVisualTemplatedParent<Button>(e.OriginalSource as FrameworkElement, PART_MultiSelectComboBox_Dropdown_Button);
			return button != null;
		}

		private bool IsRemoveItemButton(RoutedEventArgs e)
		{
			var button = VisualTreeService.FindVisualTemplatedParent<Button>(e.OriginalSource as FrameworkElement, PART_MultiSelectComboBox_SelectedItemsPanel_RemoveItem_Button);
			return button != null;
		}

		private bool IsScrollBar(RoutedEventArgs e)
		{
			var source = e.OriginalSource as FrameworkElement;
			if (source?.TemplatedParent?.GetType() == typeof(ScrollBar))
			{
				return true;
			}

			var sourceParent = source?.TemplatedParent as FrameworkElement;
			if (sourceParent?.TemplatedParent?.GetType() == typeof(ScrollBar))
			{
				return true;
			}

			return false;
		}
#endif

		private void RestorePreviousSelection()
		{
			SelectedItems.Clear();
			SelectedItems.Add(_previousSelectedValue);
			SetValue(SelectedItemsProperty, SelectedItems);
			SelectedItemsFilterTextBox.Text = string.Empty;
			SelectedItemsFilterAutoCompleteTextBox.Text = string.Empty;
			FilterTextApplied = string.Empty;
			UpdateItems(string.Empty);
		}

		private void AttemptToCloseEditMode()
		{
			if (SelectedItemsControl != null)
			{
				var task = Task.Run(
					delegate
					{
						System.Threading.Thread.Sleep(500);
					});

				task.ContinueWith(
					delegate
					{
						if (CanCloseEditMode())
						{
#if WINUI
							DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
							{
								CloseDropdownMenu(true, true);
							});
#else
							Dispatcher.BeginInvoke(
								new Action(delegate
								{
									CloseDropdownMenu(true, true);
								}));
#endif
						}
					}
				);
			}
		}

		public void CloseDropdownMenu(bool clearFilter, bool moveFocus)
		{
			if (clearFilter)
			{
				if (SelectedItemsFilterTextBox != null)
				{
					SelectedItemsFilterTextBox.Text = string.Empty;
				}

				FilterTextApplied = string.Empty;
				UpdateItems(string.Empty);
			}

			if (moveFocus)
			{
				if (SelectedItemsFilterTextBox != null)
				{
					SelectedItemsFilterTextBox.Visibility = Visibility.Collapsed;
				}

#if WINUI
				IsEditMode = false;
#else
				SetValue(IsEditModePropertyKey, false);
#endif
			}

			if (IsDropDownOpen && _previousSelectedValue != null && SelectedItems != null && SelectedItems.Count == 0)
			{
				RestorePreviousSelection();
			}

			IsDropDownOpen = false;
		}

		private bool CanCloseEditMode()
		{
			return !MultiSelectComboBoxHasFocus;
		}

		#endregion

		#region Clipboard and Automation (WPF only)

#if !WINUI
		private void CanExecute_TextBoxCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			if (e.Command == ApplicationCommands.Copy)
			{
				e.CanExecute = SelectedItems != null && SelectedItems.Count > 0;
				e.Handled = true;
			}
		}

		private void Execute_TextBoxCommand(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Command == ApplicationCommands.Copy)
			{
				Clipboard.SetText(SelectedItemsAsText);
				e.Handled = true;
			}
		}

		protected override AutomationPeer OnCreateAutomationPeer()
		{
			return new MultiSelectComboBoxAutomationPeer(this);
		}
#endif

		public string SelectedItemsAsText =>
			SelectedItems != null
				? string.Join(", ", SelectedItems.Cast<object>().Select(i => i.ToString()))
				: null;

		#endregion

		#region IDisposable
#if WINUI
		new
#endif
		public void Dispose()
		{
#if WINUI
			this.KeyUp -= MultiSelectComboBox_KeyUp;
			this.KeyDown -= MultiSelectComboBox_KeyDown;
#else
			PreviewKeyUp -= MultiSelectComboBox_PreviewKeyUp;
			PreviewKeyDown -= MultiSelectComboBox_PreviewKeyDown;
#endif

			if (MultiSelectComboBoxGrid != null)
			{
#if WINUI
				MultiSelectComboBoxGrid.PointerPressed -= MultiSelectComboBoxOnPointerPressed;
#else
				MultiSelectComboBoxGrid.PreviewMouseDown -= MultiSelectComboBoxOnPreviewMouseDown;
#endif
				MultiSelectComboBoxGrid.GotFocus -= MultiSelectComboBoxGotFocus;
				MultiSelectComboBoxGrid.LostFocus -= MultiSelectComboBoxLostFocus;
				MultiSelectComboBoxGrid.KeyUp -= MultiSelectComboBoxKeyUp;
				MultiSelectComboBoxGrid.SizeChanged -= MultiSelectComboBoxGridSizeChanged;
			}

#if !WINUI
			if (ControlWindow != null)
			{
				ControlWindow.LocationChanged -= ControlWindowLocationChanged;
				ControlWindow.Deactivated -= ControlWindowDeactivated;
			}
#endif

			if (DropdownMenu != null)
			{
				DropdownMenu.Closed -= DropdownMenuClosed;
				DropdownMenu.Opened -= DropdownMenuOpened;
			}

			if (DropdownListBox != null)
			{
				DropdownListBox.SelectionChanged -= DropdownListBoxSelectionChanged;
#if WINUI
				DropdownListBox.ItemClick -= DropdownListBoxItemClick;
				DropdownListBox.KeyDown -= DropdownListBoxKeyDown;
#else
				DropdownListBox.PreviewMouseUp -= DropdownListBoxPreviewMouseUp;
				DropdownListBox.PreviewKeyDown -= DropdownListBoxPreviewKeyDown;
				DropdownListBox.ItemContainerGenerator.StatusChanged -= DropDownListBoxItemContainerGenerator_StatusChanged;
#endif
			}

			if (SelectedItemsControl != null)
			{
#if WINUI
				SelectedItemsControl.PointerPressed -= SelectedItemsControl_OnPointerPressed;
#else
				SelectedItemsControl.Items.CurrentChanged -= SelectedItemsControl_CurrentChanged;
				SelectedItemsControl.PreviewMouseDown -= SelectedItemsControl_OnPreviewMouseDown;
#endif
				SelectedItemsControl.KeyUp -= SelectedItemsControl_OnKeyUp;
			}

			if (SelectedItemsFilterTextBox != null)
			{
#if WINUI
				SelectedItemsFilterTextBox.BeforeTextChanging -= SelectedItemsFilterTextBoxBeforeTextChanging;
#else
				SelectedItemsFilterTextBox.PreviewTextInput -= SelectedItemsFilterTextBoxPreviewTextInput;
#endif
				SelectedItemsFilterTextBox.TextChanged -= SelectedItemsFilterTextBoxTextChanged;
			}
		}

		#endregion
	}

#if !WINUI
	public class MultiSelectComboBoxAutomationPeer : FrameworkElementAutomationPeer, IValueProvider, IExpandCollapseProvider
	{
		public MultiSelectComboBoxAutomationPeer(FrameworkElement owner) : base(owner)
		{ }

		public new MultiSelectComboBox Owner => base.Owner as MultiSelectComboBox;

		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.ComboBox;
		}

		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.Value || patternInterface == PatternInterface.ExpandCollapse)
			{
				return this;
			}

			return base.GetPattern(patternInterface);
		}

		public string Value => Owner.SelectedItemsAsText;

		public bool IsReadOnly => !Owner.IsEditable;

		public ExpandCollapseState ExpandCollapseState =>
			Owner.IsDropDownOpen ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;

		public void SetValue(string value)
		{
			// Currently we support only reading the value through automation.
			throw new NotSupportedException();
		}

		public void Expand()
		{
			// Currently we support only reading the expansion state through automation.
			throw new NotSupportedException();
		}

		public void Collapse()
		{
			// Currently we support only reading the expansion state through automation.
			throw new NotSupportedException();
		}
	}
#endif
}
