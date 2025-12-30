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

		// Ensure dropdown is open, then ensure filter is empty.
		// Typing is the most reliable open path in this app.
		comboBox.OpenDropdown();

		// These 4 codes come from the Example app's LanguageItems.RecentlyUsedFilterService initialization.
		// We intentionally do NOT instantiate LanguageItems here because its item generation depends on image resources.
		var recentCodes = new[] { "en-US", "it-IT", "de-DE", "fr-FR" };
		var recentNames = recentCodes
			.Select(code => System.Globalization.CultureInfo.GetCultureInfo(code).EnglishName)
			.ToHashSet(StringComparer.OrdinalIgnoreCase);

		// Act
		var firstFourSelectable = comboBox.GetFirstSelectableDropdownItems(4);

		// Assert
		await Assert.That(firstFourSelectable.Count).IsEqualTo(4);
		await Assert.That(firstFourSelectable).IsEquivalentTo(recentNames.ToArray());

	}

	[Test]
	[Category("AlternateItems")]
	public async Task AlternateItems_DisabledItemsCannotBeSelected_AndNavigationSkipsDisabled() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		_mainPage.ClearSelectedItems();
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableGrouping, false);
		_mainPage.SetDemoOption(MainWindowPage.DemoOptions.EnableAlternateItems, true);
		comboBox.ClearFilterTextAnyItemsUsingBackspace();
		comboBox.OpenDropdown();
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();

		var got0 = comboBox.TryGetDropdownItemByIndex(0, out var item0Name, out _);
		var got1 = comboBox.TryGetDropdownItemByIndex(1, out var item1Name, out _);
		await Assert.That(got0).IsTrue();
		await Assert.That(got1).IsTrue();
		await Assert.That(item0Name).IsNotNull();
		await Assert.That(item1Name).IsNotNull();

		// Act + Assert: first dropdown item should behave as disabled (not selectable)
		comboBox.ClickDropdownItemByIndex(0);
		await Assert.That(comboBox.SelectedItemsCount).IsEqualTo(0);
		await Assert.That(comboBox.SelectedItems.Contains(item0Name)).IsFalse();

		// Act + Assert: second dropdown item should be selectable
		comboBox.ClickDropdownItemByIndex(1);
		await Assert.That(comboBox.SelectedItemsCount).IsEqualTo(1);
		await Assert.That(comboBox.SelectedItems.Contains(item1Name)).IsTrue();

		// Arrange for keyboard nav assertion
		_mainPage.ClearSelectedItems();
		comboBox.ClearFilterTextAnyItemsUsingBackspace();
		comboBox.OpenDropdown();
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();

		// Act: move focus into list (DOWN)
		comboBox.MoveKeyboardFocusToDropdownList();
		var focused = comboBox.GetFocusedDropdownItem();

		// Assert: focus should skip the first (disabled-behaving) item and land on second
		await Assert.That(focused).IsEqualTo(item1Name);
	}

	[Test]
	[Category("BatchPaste")]
	public async Task BatchPasteSelection_PastesCommaSeparatedListAndSelectsAllItems() {
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
