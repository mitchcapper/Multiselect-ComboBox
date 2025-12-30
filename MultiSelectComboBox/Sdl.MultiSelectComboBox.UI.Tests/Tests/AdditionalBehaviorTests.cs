using System;
using System.Collections.Generic;
using System.Linq;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using Sdl.MultiSelectComboBox.Example.Models;
using Sdl.MultiSelectComboBox.UI.Tests.Helpers;
using Sdl.MultiSelectComboBox.UI.Tests.PageObjects;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;
using static Sdl.MultiSelectComboBox.UI.Tests.Helpers.DelayHelper;

namespace Sdl.MultiSelectComboBox.UI.Tests.Tests;

public class AdditionalBehaviorTests : UITestBase {
	private MainWindowPage _mainPage = null!;

	public override void Setup() {
		base.Setup();
		_mainPage = new MainWindowPage(MainWindow!, Automation!);
	}

	[Test]
	[Category("Autocomplete")]
	public async Task AutoCompleteToggle_ChangesOverlayText() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		_mainPage.ClearSelectedItems();

		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableAutoComplete, true);
		comboBox.SetFilterTextClearItems("eng");

		// Assert
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();
		await Assert.That(comboBox.FilterText).IsEqualTo("eng");
		await Assert.That(comboBox.AutoCompleteText).IsNotNull();
		await Assert.That(comboBox.AutoCompleteText.Length).IsGreaterThan(0);

		// Act
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableAutoComplete, false);
		comboBox.SetFilterTextClearItems("eng");

		// Assert
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();
		await Assert.That(comboBox.FilterText).IsEqualTo("eng");
		await Assert.That(comboBox.AutoCompleteText).IsEqualTo(string.Empty);
	}

	[Test]
	[Category("Clipboard")]
	public async Task CtrlC_CopiesSelectedItemsToClipboard() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		_mainPage.ClearSelectedItems();
		ClipboardHelper.SetText(string.Empty);

		var ks = GetKnownSearch(KnownSearch.CANA);
		comboBox.TypeFilterText(ks.Term);
		comboBox.PressEnter();

		comboBox.TypeFilterText(ks.Term);
		comboBox.Navigate();
		comboBox.Navigate(1);
		comboBox.PressEnter();

		var expectedSelectedItems = comboBox.SelectedItems;
		await Assert.That(expectedSelectedItems.Count).IsEqualTo(2);

		comboBox.ClickToFocus();
		SleepShort();

		// Act
		Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_C);
		SleepLong();

		// Assert
		var text = ClipboardHelper.GetText();
		var expectedClipboard = string.Join(", ", expectedSelectedItems);
		await Assert.That(text).IsEqualTo(expectedClipboard);
	}

	[Test]
	[Category("Grouping")]
	public async Task RecentsGrouping_PutsRecentItemsFirst_WhenFilterIsEmpty() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		_mainPage.ClearSelectedItems();
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableGrouping, true);
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.UseRecentlyUsedGroupingService, true);

		comboBox.ClearFilterTextAnyItemsUsingBackspace();
		comboBox.OpenDropdown();
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();

		var languageItems = new LanguageItems();
		var recentCodes = new HashSet<string>(languageItems.RecentlyUsedFilterService.GetItems(), StringComparer.OrdinalIgnoreCase);
		var recentNames = (languageItems._allItems ?? new List<LanguageItem>())
			.Where(i => i.CultureInfo != null && recentCodes.Contains(i.CultureInfo.Name))
			.Select(i => i.Name ?? string.Empty)
			.Where(n => !string.IsNullOrWhiteSpace(n))
			.ToHashSet(StringComparer.OrdinalIgnoreCase);

		// Act
		var found = comboBox.TryGetFirstSelectableDropdownItem(out var firstSelectableName, out _);

		// Assert
		await Assert.That(found).IsTrue();
		if (!recentNames.Contains(firstSelectableName)) {
			var output = TestContext.Current?.Output?.StandardOutput;
			output?.WriteLine($"First selectable item: '{firstSelectableName}'");
			output?.WriteLine($"Recent names: {string.Join(", ", recentNames.OrderBy(a => a))}");
			output?.WriteLine($"Visible dropdown items: {string.Join(" | ", comboBox.VisibleDropdownItems)}");
		}
		await Assert.That(recentNames.Contains(firstSelectableName)).IsTrue();
	}

	[Test]
	[Category("AlternateItems")]
	public async Task AlternateItems_DisabledItemsCannotBeSelected_AndNavigationSkipsDisabled() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		_mainPage.ClearSelectedItems();
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableAlternateItems, true);

		var ks = GetKnownSearch(KnownSearch.No);
		comboBox.TypeFilterText(ks.Term);
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();

		// Find a disabled item in the visible list
		var disabledIndex = -1;
		string? disabledName = null;
		for (var i = 0; i < 20; i++) {
			if (comboBox.TryGetDropdownItemByIndex(i, out var name, out var enabled) && !enabled) {
				disabledIndex = i;
				disabledName = name;
				break;
			}
		}

		await Assert.That(disabledIndex).IsGreaterThanOrEqualTo(0);
		await Assert.That(disabledName).IsNotNull();

		// Act - try selecting the disabled item
		var before = comboBox.SelectedItemsCount;
		try {
			comboBox.ClickDropdownItemByIndex(disabledIndex);
		} catch {
			// If UIA prevents clicking disabled controls, that's fine; the net effect should still be "not selected".
		}
		var after = comboBox.SelectedItemsCount;

		// Assert
		await Assert.That(after).IsEqualTo(before);
		await Assert.That(comboBox.SelectedItems.Contains(disabledName!)).IsFalse();

		// Act - keyboard navigation should skip disabled items
		comboBox.MoveKeyboardFocusToDropdownList();
		for (var step = 0; step < 8; step++) {
			var ok = comboBox.TryGetFocusedDropdownItem(out var focusedName, out var focusedEnabled);
			await Assert.That(ok).IsTrue();
			await Assert.That(focusedName).IsNotNull();
			await Assert.That(focusedEnabled).IsTrue();
			comboBox.Navigate(1);
		}
	}

	[Test]
	[Category("KnownBroken")]
	public async Task BatchPasteSelection_PastesCommaSeparatedListAndSelectsAllItems() {
		// NOTE: This is intentionally written to assert the intended behaviour.
		// It may fail today if paste/batch selection is broken.

		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		_mainPage.ClearSelectedItems();
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableAutoComplete, true);
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableBatchSelection, true);
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableSuggestionProvider, true);

		comboBox.OpenDropdown();
		comboBox.ClickToFocus();
		SleepShort();

		var list = "Asu (Tanzania), English (United States), Asturian (Spain)";
		ClipboardHelper.SetText(list);

		// Act
		Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_V);
		SleepLong();

		// Assert
		await Assert.That(comboBox.SelectedItems).IsEquivalentTo([
			"Asu (Tanzania)",
			"English (United States)",
			"Asturian (Spain)",
		]);
	}
}
