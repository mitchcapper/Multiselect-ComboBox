using System;
using System.Linq;
using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using Sdl.MultiSelectComboBox.UI.Tests.Helpers;
using Sdl.MultiSelectComboBox.UI.Tests.PageObjects;
using static Sdl.MultiSelectComboBox.UI.Tests.Helpers.DelayHelper;

namespace Sdl.MultiSelectComboBox.UI.Tests.Tests;

/// <summary>
/// Tests covering the Example application's "Demo Settings" options.
/// These validate that toggling demo options results in observable, user-facing behavior changes.
/// </summary>
public class DemoOptionsTests : UITestBase {
	private MainWindowPage _mainPage = null!;

	public override void Setup() {
		base.Setup();
		_mainPage = new MainWindowPage(MainWindow!, Automation!);
	}

	[Test]
	[Category("DemoOptions")]
	public async Task SelectionMode_Single_SelectingSecondItemReplacesFirst() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		var ksFirst = GetKnownSearch(KnownSearch.dut);
		var firstItem = ksFirst.VisibleItemToPosition[0].Key;
		var ksSecond = GetKnownSearch(KnownSearch.CANA);
		var secondItem = ksSecond.VisibleItemToPosition[0].Key;

		_mainPage.ClearSelectedItems();
		_mainPage.SetSelectionMode("Single");
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.ClearSelectionOnFilterChanged, false);

		// Act
		comboBox.SetFilterTextClearItems(ksFirst.Term);
		comboBox.SelectItemByName(firstItem);
		comboBox.TypeFilterText(ksSecond.Term);
		comboBox.SelectItemByName(secondItem);

		// Assert
		await Assert.That(_mainPage.DisplayedSelectedCount).IsEqualTo(1);
		await Assert.That(comboBox.SelectedItems.Single()).IsEqualTo(secondItem);
	}
		[Test]
	[Category("DemoOptions")]
	public async Task SelectMode_Single_AfterClearingFirstSelectionCanTypeNewFilter(){
		var comboBox = _mainPage.MultiSelectComboBox;
		var ksFirst = GetKnownSearch(KnownSearch.dut);
		var ksSecond = GetKnownSearch(KnownSearch.No);
		var firstItem = ksFirst.VisibleItemToPosition[0].Key;
		_mainPage.SetSelectionMode("Single");
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.ClearSelectionOnFilterChanged, false);
		comboBox.TypeFilterText(ksFirst.Term);
		comboBox.SelectItemByName(firstItem);
		_mainPage.ClearSelectedItems();

		var invalid = "InvalidFilter";
		comboBox.TypeFilterText(ksSecond.Term);
		await Assert.That( comboBox.FilterText.Contains(ksSecond.Term)).IsEqualTo(true);
	}
	[Test]
	[Category("DemoOptions")]
	public async Task SelectMode_Single_CanAddFilterTextAfterFirstItem(){
		var comboBox = _mainPage.MultiSelectComboBox;
		var ksFirst = GetKnownSearch(KnownSearch.dut);
		var firstItem = ksFirst.VisibleItemToPosition[0].Key;
		_mainPage.SetSelectionMode("Single");
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.ClearSelectionOnFilterChanged, false);
		comboBox.TypeFilterText(ksFirst.Term);
		comboBox.SelectItemByName(firstItem);
		var invalid = "InvalidFilter";
		comboBox.TypeFilterText(invalid);
		await Assert.That( comboBox.FilterText.Contains(invalid)).IsEqualTo(true);
	}

	[Test]
	[Category("DemoOptions")]
	public async Task ClearFilterOnDropdownClosing_WhenEnabled_ClearsFilterTextAfterClose() {
		// Arrange
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.ClearFilterOnDropdownClosing, true);
		var comboBox = _mainPage.MultiSelectComboBox;
		var ks = GetKnownSearch(KnownSearch.dut);

		// Act
		comboBox.TypeFilterText(ks.Term);
		comboBox.Navigate();
		await Assert.That(comboBox.FilterText).IsEqualTo(ks.Term);
		comboBox.CloseDropdown();

		// Assert
		
		await Assert.That(comboBox.FilterText).IsEqualTo(string.Empty);
	}

	[Test]
	[Category("DemoOptions")]
	public async Task ClearFilterOnDropdownClosing_WhenDisabled_PreservesFilterTextAfterClose() {
		// Arrange
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.ClearFilterOnDropdownClosing, false);
		var comboBox = _mainPage.MultiSelectComboBox;
		var ks = GetKnownSearch(KnownSearch.dut);

		// Act
		comboBox.TypeFilterText(ks.Term);
		comboBox.Navigate();
		await Assert.That(comboBox.FilterText).IsEqualTo(ks.Term);
		comboBox.CloseDropdown();

		// Assert
		await Assert.That(comboBox.FilterText).IsEqualTo(ks.Term);
	}

	[Test]
	[Category("DemoOptions")]
	public async Task IsEditableAndFiltering_WhenDisabled_DoesNotEnterEditMode() {
		// Arrange
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableFiltering, false);
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.IsEditable, false);
		var comboBox = _mainPage.MultiSelectComboBox;

		// Act
		comboBox.ClickToFocus();

		// Assert
		await Assert.That(comboBox.InEditMode).IsFalse();
	}

	[Test]
	[Category("DemoOptions")]
	public async Task EnableFiltering_WhenDisabled_NonExistentTextDoesNotReduceListToZero() {
		// Arrange
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableFiltering, false);
		var comboBox = _mainPage.MultiSelectComboBox;

		// Act
		comboBox.SetFilterTextClearItems("xyznonexistent123");
		comboBox.OpenDropdown();

		// Assert
		var visibleItems = comboBox.VisibleDropdownItems;
		await Assert.That(visibleItems.Count).IsGreaterThan(0);
	}

	[Test]
	[Category("DemoOptions")]
	public async Task UseCustomFilterService_WhenEnabled_CanFilterByCultureId() {
		// Arrange
		// The suggestion provider is name-based; disable it so the filter service itself is what drives results.
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableSuggestionProvider, false);
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableFiltering, true);
		var comboBox = _mainPage.MultiSelectComboBox;
		var ks = GetKnownSearch(KnownSearch.CANA);
		var displayName = ks.VisibleItemToPosition[0].Key;
		var cultureCode = GetCultureCodeForDisplayName(displayName);

		// With the default service, filtering is by Name, so a culture code should not match.
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.UseCustomFilterService, false);
		comboBox.SetFilterTextClearItems(cultureCode);
		var defaultServiceItems = comboBox.VisibleDropdownItems;
		await Assert.That(defaultServiceItems.Any(i => i.Contains(displayName, StringComparison.OrdinalIgnoreCase))).IsFalse();

		// Act
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.UseCustomFilterService, true);
		comboBox.SetFilterTextClearItems(cultureCode);

		// Assert
		var customServiceItems = comboBox.VisibleDropdownItems;
		await Assert.That(customServiceItems.Any(i => i.Contains(displayName, StringComparison.OrdinalIgnoreCase))).IsTrue();
	}

	[Test]
	[Category("DemoOptions")]
	public async Task ClearSelectionOnFilterChanged_WhenEnabledInSingleMode_ClearsSelection() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.ClickToFocus();
		var ksSelected = GetKnownSearch(KnownSearch.dut);
		var selectedItem = ksSelected.VisibleItemToPosition[0].Key;
		var ksTrigger = GetKnownSearch(KnownSearch.CANA);

		_mainPage.ClearSelectedItems();
		_mainPage.SetSelectionMode("Single");
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.ClearSelectionOnFilterChanged, false);
		comboBox.SetFilterTextClearItems(ksSelected.Term);
		comboBox.SelectItemByName(selectedItem);
		comboBox.CloseDropdown();
		await Assert.That(_mainPage.DisplayedSelectedCount).IsEqualTo(1);

		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.ClearSelectionOnFilterChanged, true);
		
		// Act
		comboBox.SetFilterTextClearItems(ksTrigger.Term);

		// Assert
		await Assert.That(_mainPage.DisplayedSelectedCount).IsEqualTo(0);
	}

	[Test]
	[Category("DemoOptions")]
	public async Task ListenToFilterTextChanged_WhenDisabled_DoesNotWriteFilterChangedToLog() {
		// Arrange
		_mainPage.ClearEventLog();
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.ListenToFilterTextChanged, false);
		var comboBox = _mainPage.MultiSelectComboBox;
		var ks = GetKnownSearch(KnownSearch.dut);

		// Act
		comboBox.TypeFilterText(ks.Term);
		SleepLong();

		// Assert
		await Assert.That(_mainPage.EventLogText.Contains("Filter Changed", StringComparison.OrdinalIgnoreCase)).IsFalse();
	}

	[Test]
	[Category("DemoOptions")]
	public async Task ListenToFilterTextChanged_WhenEnabled_WritesFilterChangedToLog() {
		// Arrange
		_mainPage.ClearEventLog();
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.ListenToFilterTextChanged, true);
		var comboBox = _mainPage.MultiSelectComboBox;
		var ks = GetKnownSearch(KnownSearch.dut);

		// Act
		comboBox.TypeFilterText(ks.Term);
		
		// Assert
		await Assert.That(_mainPage.EventLogText.Contains("Filter Changed", StringComparison.OrdinalIgnoreCase)).IsTrue();
	}

	[Test]
	[Category("DemoOptions")]
	public async Task ListenToSelectedItemsChanged_WhenDisabled_DoesNotWriteSelectedChangedToLog() {
		// Arrange
		_mainPage.ClearSelectedItems();
		_mainPage.ClearEventLog();
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.ListenToSelectedItemsChanged, false);
		var comboBox = _mainPage.MultiSelectComboBox;
		var ks = GetKnownSearch(KnownSearch.dut);
		var item = ks.VisibleItemToPosition[0].Key;

		// Act
		comboBox.TypeFilterText(ks.Term);
		comboBox.SelectItemByName(item);
		comboBox.CloseDropdown();
		SleepLong();

		// Assert
		await Assert.That(_mainPage.EventLogText.Contains("Selected Changed", StringComparison.OrdinalIgnoreCase)).IsFalse();
	}

	[Test]
	[Category("DemoOptions")]
	public async Task ListenToSelectedItemsChanged_WhenEnabled_WritesSelectedChangedToLog() {
		// Arrange
		_mainPage.ClearSelectedItems();
		_mainPage.ClearEventLog();
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.ListenToSelectedItemsChanged, true);
		var comboBox = _mainPage.MultiSelectComboBox;
		var ks = GetKnownSearch(KnownSearch.dut);
		var item = ks.VisibleItemToPosition[0].Key;

		// Act
		comboBox.TypeFilterText(ks.Term);
		comboBox.SelectItemByName(item);
		comboBox.CloseDropdown();
		
		// Assert
		await Assert.That(_mainPage.EventLogText.Contains("Selected Changed", StringComparison.OrdinalIgnoreCase)).IsTrue();
	}

	[Test]
	[Category("DemoOptions")]
	public async Task EnableAutoComplete_WhenDisabled_BatchSelectionCheckboxIsDisabled() {
		// Arrange
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableAutoComplete, false);

		// Assert
		await Assert.That(_mainPage.IsDemoOptionEnabled(MainWindowPage.DemoOptions.EnableBatchSelection)).IsFalse();
	}

	[Test]
	[Category("DemoOptions")]
	public async Task EnableGrouping_WhenDisabled_RecentlyUsedCheckboxIsDisabled() {
		// Arrange
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableGrouping, false);

		// Assert
		await Assert.That(_mainPage.IsDemoOptionEnabled(MainWindowPage.DemoOptions.UseRecentlyUsedGroupingService)).IsFalse();
	}

	[Test]
	[Category("DemoOptions")]
	public async Task EnableFiltering_WhenDisabled_CustomFilterCheckboxIsDisabled() {
		// Arrange
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableFiltering, false);

		// Assert
		await Assert.That(_mainPage.IsDemoOptionEnabled(MainWindowPage.DemoOptions.UseCustomFilterService)).IsFalse();
	}

	[Test]
	[Category("DemoOptions")]
	public async Task EnableAlternateItems_WhenEnabled_FirstItemIsDisabledAndCannotBeSelected() {
		// Arrange
		_mainPage.ClearSelectedItems();
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableGrouping, false);
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableAlternateItems, true);

		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.SetFilterTextClearItems(string.Empty);
		comboBox.OpenDropdown();

		// Act
		var found = comboBox.TryGetDropdownItemByIndex(0, out var name, out var enabled);
		await Assert.That(found).IsTrue();
		await Assert.That(enabled).IsFalse();
		await Assert.That(name).IsNotEqualTo(string.Empty);

		comboBox.ClickDropdownItemByIndex(0);

		// Assert
		await Assert.That(comboBox.SelectedItemsCount).IsEqualTo(0);
	}

	[Test]
	[Category("DemoOptions")]
	public async Task EnableSuggestionProvider_ToggleOffAndOn_FilteringStillWorks() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableFiltering, true);
		var ks = GetKnownSearch(KnownSearch.dut);
		var expectedItem = ks.VisibleItemToPosition[0].Key;

		// Act
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableSuggestionProvider, false);
		comboBox.SetFilterTextClearItems(ks.Term);
		var itemsAfterDisable = comboBox.VisibleDropdownItems;
		await Assert.That(itemsAfterDisable.Any(i => i.Contains(expectedItem, StringComparison.OrdinalIgnoreCase))).IsTrue();

		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableSuggestionProvider, true);
		comboBox.SetFilterTextClearItems(ks.Term);

		// Assert
		var itemsAfterEnable = comboBox.VisibleDropdownItems;
		await Assert.That(itemsAfterEnable.Any(i => i.Contains(expectedItem, StringComparison.OrdinalIgnoreCase))).IsTrue();
	}

	public override void TearDown() {
		_mainPage?.Dispose();
		base.TearDown();
	}
}
