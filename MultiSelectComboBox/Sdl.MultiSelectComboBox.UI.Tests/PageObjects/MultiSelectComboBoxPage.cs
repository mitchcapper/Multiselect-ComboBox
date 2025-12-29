using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Identifiers;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;

using ControlConsts = Sdl.MultiSelectComboBox.Themes.Generic.MultiSelectComboBox;

namespace Sdl.MultiSelectComboBox.UI.Tests.PageObjects;

/// <summary>
/// Page Object for interacting with the MultiSelectComboBox control
/// </summary>
public class MultiSelectComboBoxPage : IDisposable {
	private readonly Window _window;
	private readonly AutomationBase _automation;
	private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(3);
	private bool _disposed;

	public MultiSelectComboBoxPage(Window window, AutomationBase automation) {
		_window = window ?? throw new ArgumentNullException(nameof(window));
		_automation = automation ?? throw new ArgumentNullException(nameof(automation));
	}

	#region Element Properties

	/// <summary>
	/// Gets the main MultiSelectComboBox control (the first one with default style visible)
	/// </summary>
	private AutomationElement MultiSelectComboBoxControl =>
		WaitForElement(cf => cf.ByAutomationId("mainMultiSelectComboxBox"));

	private AutomationElement CustomThemeMultiSelectComboBoxControl =>
		WaitForElement(cf => cf.ByAutomationId("mainMultiSelectComboxBoxCustom"));

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
			item.Patterns.VirtualizedItem.Pattern.Realize();
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
					System.Threading.Thread.Sleep(100);
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
		Thread.Sleep(200);
	}

	/// <summary>
	/// Types text into the filter textbox to filter dropdown items
	/// </summary>
	public void TypeFilterText(string text) {
		ClickToFocus();
		Thread.Sleep(100);

		// Type using keyboard to simulate real user input
		Keyboard.Type(text);
		Thread.Sleep(300); // Wait for filtering to apply
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
			Thread.Sleep(50);
		}
		Thread.Sleep(200);
	}

	/// <summary>
	/// Presses backspace key to delete characters
	/// </summary>
	public void PressBackspace(int times = 1) {
		for (int i = 0; i < times; i++) {
			Keyboard.Type(VirtualKeyShort.BACK);
			Thread.Sleep(100);
		}
	}

	#endregion

	#region Actions - Dropdown

	/// <summary>
	/// Opens the dropdown by clicking the dropdown button
	/// </summary>
	public void OpenDropdown() {
		if (!IsDropdownOpen) {
			ClickToFocus();//must be in it fro the dorpdown to open
			Thread.Sleep(100);
			var button = DropdownButton;
			button!.Click();
			WaitForDropdownOpen();
		}
	}

	/// <summary>
	/// Closes the dropdown by pressing Escape
	/// </summary>
	public void CloseDropdown() {
		if (IsDropdownOpen) {
			Keyboard.Type(VirtualKeyShort.ESCAPE);
			Thread.Sleep(200);
		}
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
		Thread.Sleep(200);

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
		Thread.Sleep(200);
	}

	/// <summary>
	/// Selects an item using keyboard navigation (arrow down then enter)
	/// </summary>
	public void SelectItemByKeyboard(int arrowDownCount, bool leaveOpen = false) {
		OpenDropdown();
		Thread.Sleep(200);

		for (int i = 0; i < arrowDownCount; i++) {
			Keyboard.Type(VirtualKeyShort.DOWN);
			Thread.Sleep(100);
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
			Thread.Sleep(200);
			SelectItemByKeyboard(1);
			Thread.Sleep(200);
		}
	}

	/// <summary>
	/// Selects items directly by their display names
	/// </summary>
	public void SelectItemsByName(params string[] itemNames) {
		foreach (var name in itemNames) {
			SelectItemByName(name);
			Thread.Sleep(200);
		}
	}
	public void Navigate(int offset) {
		if (offset > 0)
			NavigateDown(offset);
		else if (offset < 0)
			NavigateUp(-offset);
	}
	/// <summary>
	/// Navigates down in the dropdown using arrow key
	/// </summary>
	public void NavigateDown(int times = 1) {
		for (int i = 0; i < times; i++) {
			Keyboard.Type(VirtualKeyShort.DOWN);
			Thread.Sleep(100);
		}
	}

	/// <summary>
	/// Navigates up in the dropdown using arrow key
	/// </summary>
	public void NavigateUp(int times = 1) {
		for (int i = 0; i < times; i++) {
			Keyboard.Type(VirtualKeyShort.UP);
			Thread.Sleep(100);
		}
	}

	/// <summary>
	/// Presses Enter to select the currently focused item
	/// </summary>
	public void PressEnter() {
		Keyboard.Type(VirtualKeyShort.ENTER);
		Thread.Sleep(200);
	}
	/// <summary>
	/// Presses Space to select the currently focused item and leave open
	/// </summary>
	public void PressSpace() {
		Keyboard.Type(VirtualKeyShort.SPACE);
		Thread.Sleep(200);
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
					Thread.Sleep(200);
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
			Thread.Sleep(50);
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
