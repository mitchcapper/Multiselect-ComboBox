using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Identifiers;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using static Sdl.MultiSelectComboBox.UI.Tests.Helpers.DelayHelper;

using ControlConsts = Sdl.MultiSelectComboBox.Themes.Generic.MultiSelectComboBox;

namespace Sdl.MultiSelectComboBox.UI.Tests.PageObjects;

/// <summary>
/// Page Object for interacting with the MultiSelectComboBox control
/// </summary>
public class MultiSelectComboBoxPage : IDisposable {
	private readonly Window _window;
	private readonly AutomationBase _automation;
	private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(5);
	private bool _disposed;

	public MultiSelectComboBoxPage(Window window, AutomationBase automation) {
		_window = window ?? throw new ArgumentNullException(nameof(window));
		_automation = automation ?? throw new ArgumentNullException(nameof(automation));
	}

	#region Element Properties

	/// <summary>
	/// Gets the main MultiSelectComboBox control (the first one with default style visible)
	/// </summary>
	private AutomationElement MultiSelectComboBoxControl => WaitForMultiSelectComboBoxControl();

	private AutomationElement WaitForMultiSelectComboBoxControl(TimeSpan? timeout = null) {
		timeout ??= _defaultTimeout;
		return Retry.WhileNull(
			() => {
				var defaultCtrl = _window.FindFirstDescendant(cf => cf.ByAutomationId("mainMultiSelectComboxBox"));
				if (defaultCtrl != null) {
					return defaultCtrl;
				}

				return _window.FindFirstDescendant(cf => cf.ByAutomationId("mainMultiSelectComboxBoxCustom"));
			},
			timeout: timeout.Value,
			throwOnTimeout: true,
			timeoutMessage: "Element not found: MultiSelectComboBox"
		).Result;
	}

	/// <summary>
	/// Gets the filter textbox inside the selected items panel
	/// </summary>
	private TextBox? FilterTextBox {
		get {
			var control = MultiSelectComboBoxControl;
			return control?.FindFirstDescendant(cf => cf.ByAutomationId(ControlConsts.PART_MultiSelectComboBox_SelectedItemsPanel_Filter_TextBox))?.AsTextBox();
		}
	}
	/// <summary>
	/// True when in edit mode, IE the pencil icon is gone
	/// </summary>
	public bool InEditMode => FilterTextBox?.IsEnabled == true; // FilterTextBox?.Patterns?.Value?.Pattern?.IsReadOnly == false works EXCEPt for first run,  for whatever reason it doesnt get set to readonly even though its not enabled in terms of UIA props 


	/// <summary>
	/// Gets the dropdown button
	/// </summary>
	private Button? DropdownButton {
		get {
			var control = MultiSelectComboBoxControl;
			return control?.FindFirstDescendant(cf => cf.ByAutomationId(ControlConsts.PART_MultiSelectComboBox_ToggleButton))?.AsButton();
		}
	}

	private Button? ArrowButton {
		get {
			var control = MultiSelectComboBoxControl;
			return WaitForElement<Button>(cf => cf.ByAutomationId(ControlConsts.PART_MultiSelectComboBox_Dropdown_Button));
		}
	}

	/// <summary>
	/// Gets the dropdown listbox containing items
	/// </summary>
	private ListBox? DropdownListBox {
		get {
			return _window.FindFirstDescendant(cf => cf.ByAutomationId(ControlConsts.PART_MultiSelectComboBox_Dropdown_ListBox))?.AsListBox();
		}
	}

	/// <summary>
	/// Gets the selected items panel (ItemsControl containing selected item tags)
	/// </summary>
	private AutomationElement? SelectedItemsPanel =>
		MultiSelectComboBoxControl?.FindFirstDescendant(cf => cf.ByAutomationId(ControlConsts.PART_MultiSelectComboBox_SelectedItemsPanel_ItemsControl));

	#endregion

	#region Read-Only Properties

	/// <summary>
	/// Gets the current filter text value
	/// </summary>
	public string FilterText => FilterTextBox?.Text ?? string.Empty;

	/// <summary>
	/// Returns true if the dropdown is currently open
	/// </summary>
	public bool IsDropdownOpen {
		get {

			var expectedState = DropdownButton!.Patterns.Toggle.Pattern.ToggleState.Value == FlaUI.Core.Definitions.ToggleState.On;
			var listBox = DropdownListBox;
			var listState = listBox?.IsOffscreen == false;
			if (expectedState != listState)
				throw new DataMisalignedException($"Dropdown open state mismatch: between ToggleButton is: {expectedState} and listbox Onscreen: {listState}");
			return listState;
		}
	}

	/// <summary>
	/// Gets the visible items in the dropdown (when open)
	/// </summary>
	public IReadOnlyList<string> VisibleDropdownItems => GetVisibleDropdownItems(6);

	public bool TryGetFirstVisibleDropdownItem(out string name, out bool isEnabled) {
		name = string.Empty;
		isEnabled = false;
		var items = GetVisibleDropdownItemElements(maxToDevirtualize: 6);
		var first = items.FirstOrDefault();
		if (first == null) {
			return false;
		}

		name = first.Name ?? string.Empty;
		isEnabled = first.IsEnabled;
		return !string.IsNullOrWhiteSpace(name);
	}

	public bool TryGetDropdownItemByIndex(int index, out string name, out bool isEnabled) {
		name = string.Empty;
		isEnabled = false;
		if (!IsDropdownOpen) {
			return false;
		}

		var listBox = DropdownListBox;
		if (listBox == null) {
			return false;
		}

		var items = listBox.Items;
		if (index < 0 || index >= items.Length) {
			return false;
		}

		var item = items[index];
		name = item.Name ?? string.Empty;
		isEnabled = item.IsEnabled;
		return !string.IsNullOrWhiteSpace(name);
	}

	public void ClickDropdownItemByIndex(int index) {
		OpenDropdown();
		var listBox = DropdownListBox;
		if (listBox == null) {
			throw new InvalidOperationException("Could not find dropdown listbox");
		}

		var items = listBox.Items;
		if (index < 0 || index >= items.Length) {
			throw new ArgumentOutOfRangeException(nameof(index));
		}

		items[index].Click();
		SleepLong();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="maxToDevirtualize">if 0 then no devirutalization otherwise up to this many</param>
	/// <returns></returns>
	public IReadOnlyList<string> GetVisibleDropdownItems(int maxToDevirtualize = 0) {

		if (!IsDropdownOpen) return Array.Empty<string>();
		var listBox = DropdownListBox;
		if (listBox == null) return Array.Empty<string>();

		var items = listBox.Items;
		return items.Where(i => ItemIsVisible(i, ref maxToDevirtualize)).Select(a => a.Name).ToList();

	}

	private IReadOnlyList<ListBoxItem> GetVisibleDropdownItemElements(int maxToDevirtualize = 0) {
		if (!IsDropdownOpen) return Array.Empty<ListBoxItem>();
		var listBox = DropdownListBox;
		if (listBox == null) return Array.Empty<ListBoxItem>();

		var items = listBox.Items;
		return items.Where(i => ItemIsVisible(i, ref maxToDevirtualize)).ToList();
	}

	private string CollToString<T>(IEnumerable<T> items) {
		return String.Join(", ", items);
	}
	private bool ItemIsVisible(ListBoxItem item, ref int maxToDevirtualize) {
		if (String.IsNullOrWhiteSpace(item?.Name))
			return false;
		//Console.WriteLine($"Inspecting: {item?.Name} virt: {item.Patterns.VirtualizedItem.IsSupported}");
		// so IsOffscreen can throw an error for virtualized items we cant even check if its supported.  But the item only supports the virtualized pattern if it hasnt been realized yet so we know its not visibile if it supports the Virtualized Pattern.

		//var supportedPatterns = CollToString(item.GetSupportedPatterns());
		if (item.Patterns.VirtualizedItem.IsSupported) {
			if (maxToDevirtualize == 0)
				return false;
			if (item.Patterns.VirtualizedItem.TryGetPattern(out var pat)) // incase it resolved inbetween
				pat.Realize();
			maxToDevirtualize--;
		}
		return !item.IsOffscreen;

	}

	/// <summary>
	/// Gets all items in the dropdown (visible or not)
	/// </summary>
	public IReadOnlyList<string> AllDropdownItems {
		get {
			if (!IsDropdownOpen) return Array.Empty<string>();
			var listBox = DropdownListBox;
			if (listBox == null) return Array.Empty<string>();

			return listBox.Items
				.Select(item => item.Name ?? string.Empty)
				.Where(name => !string.IsNullOrWhiteSpace(name))
				.ToList();
		}
	}

	/// <summary>
	/// Gets the names of currently selected items (shown as tags in the control)
	/// </summary>
	public IReadOnlyList<string> SelectedItems {
		get {
			var panel = SelectedItemsPanel;
			if (panel == null) return Array.Empty<string>();

			// Find all text elements representing selected items
			var textElements = panel.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text));
			return textElements
				.Select(t => t.Name ?? string.Empty)
				.Where(name => !string.IsNullOrWhiteSpace(name))
				.ToList();
		}
	}

	/// <summary>
	/// Gets the count of selected items
	/// </summary>
	public int SelectedItemsCount => SelectedItems.Count;

	/// <summary>
	/// Gets the displayed selected count from the UI (the "X Selected" text)
	/// </summary>
	public int DisplayedSelectedCount {
		get {
			// Find the TextBox showing "X Selected" at the top of the window
			var countTextBox = _window.FindFirstDescendant(cf => cf.ByAutomationId("SelectedItemsCountTextBox"));

			if (countTextBox != null) {
				// Retry reading a few times in case of binding delay
				for (int i = 0; i < 10; i++) {
					var text = countTextBox.AsTextBox().Text;
					if (int.TryParse(text, out var count)) {
						if (count > 0) return count;
					}
					SleepShort();
				}

				var finalText = countTextBox.AsTextBox().Text;
				if (int.TryParse(finalText, out var finalCount)) return finalCount;
			}
			return 0;
		}
	}

	#endregion

	#region Actions - Filtering

	/// <summary>
	/// Click on the control to focus it and enable editing
	/// </summary>
	public void ClickToFocus() {
		var control = MultiSelectComboBoxControl;
		control?.Click();
		SleepLong();
	}

	/// <summary>
	/// Types text into the filter textbox to filter dropdown items
	/// </summary>
	public void TypeFilterText(string text) {
		ClickToFocus();
		SleepShort();

		// Type using keyboard to simulate real user input
		Keyboard.Type(text);
		for (var x = 0; x < 3; x++) //yeah 3x sleep for reliability
			SleepLong();

	}

	/// <summary>
	/// Sets filter text directly (clears existing and types new)
	/// </summary>
	public void SetFilterTextClearItems(string text) {
		ClearFilterTextAnyItemsUsingBackspace();
		TypeFilterText(text);
	}

	/// <summary>
	/// Clears the filter text using backspace
	/// </summary>
	public void ClearFilterTextAnyItemsUsingBackspace() {
		ClickToFocus();
		var currentText = FilterText;
		for (int i = 0; i < currentText.Length + 5; i++) {
			Keyboard.Type(VirtualKeyShort.BACK);
			SleepShort();
		}
		SleepLong();
	}

	/// <summary>
	/// Presses backspace key to delete characters
	/// </summary>
	public void PressBackspace(int times = 1) {
		for (int i = 0; i < times; i++) {
			Keyboard.Type(VirtualKeyShort.BACK);
			SleepShort();
		}
	}

	#endregion

	#region Actions - Dropdown

	/// <summary>
	/// Opens the dropdown by clicking the dropdown button
	/// </summary>
	public void OpenDropdown() {
		if (IsDropdownOpen) {
			return;
		}

		// Clicking the control can sometimes open the dropdown implicitly.
		ClickToFocus();
		SleepShort();
		if (IsDropdownOpen) {
			return;
		}

		ArrowButton!.Click();
		SleepShort();
		WaitForDropdownOpen();
	}

	public void MoveKeyboardFocusToDropdownList() {
		OpenDropdown();
		Keyboard.Type(VirtualKeyShort.DOWN);
		SleepShort();
	}

	/// <summary>
	/// Closes the dropdown by pressing Escape
	/// </summary>
	public void CloseDropdown() {
		try {
			if (!IsDropdownOpen) {
				return;
			}
		} catch {
			// If we can't reliably query state, still try to close; Escape is safe.
		}

		Keyboard.Type(VirtualKeyShort.ESCAPE);
		SleepLong();
	}

	/// <summary>
	/// Waits for the dropdown to open
	/// </summary>
	public void WaitForDropdownOpen(TimeSpan? timeout = null) {
		timeout ??= _defaultTimeout;
		Retry.WhileTrue(
			() => !IsDropdownOpen,
			timeout: timeout.Value,
			throwOnTimeout: true,
			timeoutMessage: "Dropdown did not open in time"
		);
	}

	#endregion

	#region Actions - Selection

	/// <summary>
	/// Selects an item from the dropdown by clicking on it
	/// </summary>
	public void SelectItemByName(string itemName) {
		OpenDropdown();

		var listBox = DropdownListBox;
		if (listBox == null) {
			throw new InvalidOperationException("Could not find dropdown listbox");
		}

		var item = listBox.Items.FirstOrDefault(i =>
			(i.Name ?? string.Empty).Contains(itemName, StringComparison.OrdinalIgnoreCase));

		if (item == null) {

			throw new InvalidOperationException($"Item containing '{itemName}' not found in dropdown");
		}

		item.Click();
		SleepLong();
	}

	/// <summary>
	/// Selects an item using keyboard navigation (arrow down then enter)
	/// </summary>
	public void SelectItemByKeyboard(int arrowDownCount, bool leaveOpen = false) {
		OpenDropdown();
		SleepLong();

		for (int i = 0; i < arrowDownCount; i++) {
			Keyboard.Type(VirtualKeyShort.DOWN);
			SleepShort();
		}
		if (leaveOpen)
			this.PressSpace();
		else
			this.PressEnter();

	}

	/// <summary>
	/// Selects one or more items by typing filter text and confirming with keyboard
	/// </summary>
	public void SelectItemsByFilter(params string[] filterTexts) {
		foreach (var filter in filterTexts) {
			TypeFilterText(filter);
			SleepLong();
			SelectItemByKeyboard(1);
			SleepLong();
		}
	}

	/// <summary>
	/// Selects items directly by their display names
	/// </summary>
	public void SelectItemsByName(params string[] itemNames) {
		foreach (var name in itemNames) {
			SelectItemByName(name);
			SleepLong();
		}
	}
	/// <summary>
	/// Navigates using arrow keys up (negative numbers) or down (positive numbers) in the dropdown list
	/// </summary>
	/// <param name="offset"></param>
	public void Navigate(int offset=1) {
		if (offset > 0)
			NavigateDown(offset);
		else if (offset < 0)
			NavigateUp(-offset);
		SleepLong();
	}
	/// <summary>
	/// Navigates down in the dropdown using arrow key
	/// </summary>
	private void NavigateDown(int times = 1) {
		for (int i = 0; i < times; i++) {
			Keyboard.Type(VirtualKeyShort.DOWN);
			SleepShort();
		}
	}

	/// <summary>
	/// Navigates up in the dropdown using arrow key
	/// </summary>
	private void NavigateUp(int times = 1) {
		for (int i = 0; i < times; i++) {
			Keyboard.Type(VirtualKeyShort.UP);
			SleepShort();
		}
	}

	/// <summary>
	/// Presses Enter to select the currently focused item
	/// </summary>
	public void PressEnter() {
		Keyboard.Type(VirtualKeyShort.ENTER);
		SleepLong();
	}
	/// <summary>
	/// Presses Space to select the currently focused item and leave open
	/// </summary>
	public void PressSpace() {
		Keyboard.Type(VirtualKeyShort.SPACE);
		SleepLong();
	}

	/// <summary>
	/// Gets the currently focused/highlighted item in the dropdown
	/// </summary>
	public string? GetFocusedDropdownItem() {
		var listBox = DropdownListBox;
		if (listBox == null) return null;

		// Look for the selected/focused item
		var focused = listBox.Items.FirstOrDefault(item =>
			item.Patterns.SelectionItem?.Pattern?.IsSelected.Value == true);

		return focused?.Name;
	}

	#endregion

	#region Actions - Remove Items

	/// <summary>
	/// Clicks the X button to remove a selected item by its name
	/// </summary>
	public void RemoveSelectedItemByName(string itemName) {
		var panel = SelectedItemsPanel;
		if (panel == null) {
			throw new InvalidOperationException("Could not find selected items panel");
		}

		// Find all item containers with remove buttons
		var buttons = panel.FindAllDescendants(cf => cf.ByAutomationId(ControlConsts.PART_MultiSelectComboBox_SelectedItemsPanel_RemoveItem_Button));

		foreach (var button in buttons) {
			// Check if this button's parent container contains the item name
			var parent = button.Parent;
			if (parent != null) {
				var texts = parent.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text));
				if (texts.Any(t => (t.Name ?? string.Empty).Contains(itemName, StringComparison.OrdinalIgnoreCase))) {
					button.AsButton().Click();
					SleepLong();
					return;
				}
			}
		}

		throw new InvalidOperationException($"Could not find remove button for item '{itemName}'");
	}
	public class OurRemovalButton {
		private Button Button;

		public OurRemovalButton(Button button) {
			this.Button = button;
		}
		public void Click() {
			Button.Invoke(); //does nothing but does scroll us into view:)
			SleepShort();
			Button.Click();//perform actual mouse event now that position is correct
		}
	}
	/// <summary>
	/// Gets all remove buttons (X buttons) for selected items
	/// </summary>
	public IReadOnlyList<OurRemovalButton> GetRemoveButtons() {
		var panel = SelectedItemsPanel;
		if (panel == null) return [];

		return panel.FindAllDescendants(cf => cf.ByAutomationId(ControlConsts.PART_MultiSelectComboBox_SelectedItemsPanel_RemoveItem_Button))
			.Select(e => e.AsButton())
			.Where(b => b.IsEnabled).Select(b => new OurRemovalButton(b))
			.ToList();
	}

	#endregion

	#region Helpers

	private T WaitForElement<T>(Func<ConditionFactory, ConditionBase> condition,
		TimeSpan? timeout = null) where T : AutomationElement {
		timeout ??= _defaultTimeout;

		var element = Retry.WhileNull(() =>
			_window.FindFirstDescendant(condition),
			timeout: timeout.Value,
			throwOnTimeout: true,
			timeoutMessage: $"Element not found: {typeof(T).Name}"
		).Result;

		// Use reflection to get the appropriate As* method or just cast
		if (typeof(T) == typeof(TextBox))
			return (T)(object)element.AsTextBox();
		if (typeof(T) == typeof(Button))
			return (T)(object)element.AsButton();
		if (typeof(T) == typeof(ListBox))
			return (T)(object)element.AsListBox();

		return (T)element;
	}

	private AutomationElement WaitForElement(Func<ConditionFactory, ConditionBase> condition,
		TimeSpan? timeout = null) {
		timeout ??= _defaultTimeout;

		return Retry.WhileNull(() =>
			_window.FindFirstDescendant(condition),
			timeout: timeout.Value,
			throwOnTimeout: true,
			timeoutMessage: "Element not found"
		).Result;
	}

	private void WindowDumpAll() {
		var all = _window.FindAllDescendants();
		foreach (var child in all) {
			var classNameAdd = "";
			if (child.Properties.ClassName.IsSupported) {
				classNameAdd = $" {child.ClassName} - ";
			}
			Console.WriteLine($"Element: {classNameAdd}{child.Name} - {child.ControlType} (ID: {child.AutomationId})");
		}
	}

	#endregion

	#region IDisposable

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing) {
		if (_disposed) return;
		_disposed = true;
	}

	#endregion
}
