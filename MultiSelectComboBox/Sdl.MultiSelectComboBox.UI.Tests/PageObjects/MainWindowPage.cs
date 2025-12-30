using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using FlaUI.Core.Definitions;
using ControlConsts = Sdl.MultiSelectComboBox.Themes.Generic.MultiSelectComboBox;
using static Sdl.MultiSelectComboBox.UI.Tests.Helpers.DelayHelper;

namespace Sdl.MultiSelectComboBox.UI.Tests.PageObjects;

/// <summary>
/// Page Object for the main example window
/// </summary>
public class MainWindowPage : IDisposable {
	private readonly Window _window;
	private readonly AutomationBase _automation;
	private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(5);
	private bool _disposed;

	public static class DemoOptions {
		public const string Select20RandomItemsButton = "demoSelect20Random";
		public const string SelectionModeComboBox = "demoSelectionMode";
		public const string ClearFilterOnDropdownClosing = "demoClearFilterOnDropdownClosing";
		public const string IsEditable = "demoIsEditable";
		public const string EnableAlternateItems = "demoEnableAlternateItems";
		public const string ListenToFilterTextChanged = "demoListenToFilterTextChanged";
		public const string ListenToSelectedItemsChanged = "demoListenToSelectedItemsChanged";
		public const string EnableAutoComplete = "demoEnableAutoComplete";
		public const string EnableBatchSelection = "demoEnableBatchSelection";
		public const string EnableGrouping = "demoEnableGrouping";
		public const string UseRecentlyUsedGroupingService = "demoUseRecentlyUsedGroupingService";
		public const string EnableFiltering = "demoEnableFiltering";
		public const string UseCustomFilterService = "demoUseCustomFilterService";
		public const string ClearSelectionOnFilterChanged = "demoClearSelectionOnFilterChanged";
		public const string EnableSuggestionProvider = "demoEnableSuggestionProvider";
	}

	private static class MainWindowAutomationIds {
		public const string ClearSelectedItemsButton = "btnClearSelectedItems";
		public const string ClearLogButton = "btnClearLog";
		public const string EventLogTextBox = "txtEventLog";
	}

	public MainWindowPage(Window window, AutomationBase automation) {
		_window = window ?? throw new ArgumentNullException(nameof(window));
		_automation = automation ?? throw new ArgumentNullException(nameof(automation));
		MultiSelectComboBox = new MultiSelectComboBoxPage(window, automation);
	}

	/// <summary>
	/// The MultiSelectComboBox control page object
	/// </summary>
	public MultiSelectComboBoxPage MultiSelectComboBox { get; }

	/// <summary>
	/// Gets the displayed selected items count from the header
	/// </summary>
	public int DisplayedSelectedCount {
		get {
			var countTextBox = _window.FindFirstDescendant(cf => cf.ByAutomationId("SelectedItemsCountTextBox"));
			if (countTextBox != null) {
				var text = countTextBox.AsTextBox().Text;
				if (int.TryParse(text, out var count)) {
					return count;
				}
			}
			return 0;
		}
	}

	/// <summary>
	/// Clicks the "Clear selected items" button
	/// </summary>
	public void ClearSelectedItems() {
		var clearButton = Retry.WhileNull(
			() => _window.FindFirstDescendant(cf => cf.ByAutomationId(MainWindowAutomationIds.ClearSelectedItemsButton))?.AsButton(),
			timeout: _defaultTimeout,
			throwOnTimeout: true,
			timeoutMessage: $"Button not found: {MainWindowAutomationIds.ClearSelectedItemsButton}"
		).Result;

		clearButton.Click();
		SleepLong();
	}

	/// <summary>
	/// Clicks the "Select 20 random items" button
	/// </summary>
	public void SelectRandomItems() {
		var button = Retry.WhileNull(
			() => _window.FindFirstDescendant(cf => cf.ByAutomationId(DemoOptions.Select20RandomItemsButton))?.AsButton(),
			timeout: _defaultTimeout,
			throwOnTimeout: true,
			timeoutMessage: $"Button not found: {DemoOptions.Select20RandomItemsButton}"
		).Result;

		button.Click();
		SleepLong(); // Wait for items to be selected
	}

	private CheckBox FindDemoOptionCheckboxById(string automationId) {
		if (string.IsNullOrWhiteSpace(automationId)) {
			throw new ArgumentException("AutomationId must be provided", nameof(automationId));
		}

		return Retry.WhileNull(
			() => _window.FindFirstDescendant(cf => cf.ByAutomationId(automationId))?.AsCheckBox(),
			timeout: _defaultTimeout,
			throwOnTimeout: true,
			timeoutMessage: $"Checkbox not found: {automationId}"
		).Result;
	}

	public void SetDemoOption(string optionCheckboxAutomationId, bool isChecked) {
		var checkbox = FindDemoOptionCheckboxById(optionCheckboxAutomationId);
		if (checkbox.IsChecked != isChecked) {
			if (checkbox.Patterns.Toggle.IsSupported) {
				checkbox.Patterns.Toggle.Pattern.Toggle();
			} else {
				checkbox.Click();
			}
			WaitFor(() => checkbox.IsChecked == isChecked, message: $"Checkbox did not update: {optionCheckboxAutomationId}");
		}
	}

	public bool IsDemoOptionEnabled(string optionCheckboxAutomationId) {
		var checkbox = FindDemoOptionCheckboxById(optionCheckboxAutomationId);
		return checkbox.IsEnabled;
	}

	public bool IsDemoOptionChecked(string optionCheckboxAutomationId) {
		var checkbox = FindDemoOptionCheckboxById(optionCheckboxAutomationId);
		return checkbox.IsChecked == true;
	}

	/// <summary>
	/// Gets the event log text
	/// </summary>
	public string EventLogText {
		get {
			var textbox = _window.FindFirstDescendant(cf => cf.ByAutomationId(MainWindowAutomationIds.EventLogTextBox))?.AsTextBox();
			if (textbox == null) {
				return string.Empty;
			}

			return textbox.Text ?? string.Empty;
		}
	}

	/// <summary>
	/// Clears the event log
	/// </summary>
	public void ClearEventLog() {
		var clearLogButton = Retry.WhileNull(
			() => _window.FindFirstDescendant(cf => cf.ByAutomationId(MainWindowAutomationIds.ClearLogButton))?.AsButton(),
			timeout: _defaultTimeout,
			throwOnTimeout: true,
			timeoutMessage: $"Button not found: {MainWindowAutomationIds.ClearLogButton}"
		).Result;

		clearLogButton.Click();
		SleepShort();
	}

	/// <summary>
	/// Sets the selection mode (Multiple or Single)
	/// </summary>
	public void SetSelectionMode(string mode) {
		var selectionModeCombo = FindSelectionModeComboBoxById();
		selectionModeCombo.Select(mode);
		SleepLong();
	}

	private ComboBox FindSelectionModeComboBoxById() {
		var element = Retry.WhileNull(
			() => _window.FindFirstDescendant(cf => cf.ByAutomationId(DemoOptions.SelectionModeComboBox))?.AsComboBox(),
			timeout: _defaultTimeout,
			throwOnTimeout: true,
			timeoutMessage: $"ComboBox not found: {DemoOptions.SelectionModeComboBox}"
		).Result;

		return element;
	}

	/// <summary>
	/// Waits for a condition to be true
	/// </summary>
	public void WaitFor(Func<bool> condition, TimeSpan? timeout = null, string? message = null) {
		timeout ??= _defaultTimeout;
		Retry.WhileTrue(
			() => !condition(),
			timeout: timeout.Value,
			throwOnTimeout: true,
			timeoutMessage: message ?? "Condition not met in time"
		);
	}

	#region IDisposable

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing) {
		if (_disposed) return;

		if (disposing) {
			MultiSelectComboBox?.Dispose();
		}

		_disposed = true;
	}

	#endregion
}
