using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using Sdl.MultiSelectComboBox.UI.Tests.Helpers;
using Sdl.MultiSelectComboBox.UI.Tests.PageObjects;
using static Sdl.MultiSelectComboBox.UI.Tests.Helpers.DelayHelper;

namespace Sdl.MultiSelectComboBox.UI.Tests.Tests;

/// <summary>
/// Tests for keyboard navigation (arrow keys, Enter, Escape)
/// </summary>
public class KeyboardNavigationTests : UITestBase {
	private MainWindowPage _mainPage = null!;

	public override void Setup() {
		base.Setup();
		_mainPage = new MainWindowPage(MainWindow!, Automation!);
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task SpaceKeySelectsItemAndLeavesDropDownOpen() {
		var comboBox = _mainPage.MultiSelectComboBox;
		var ks = GetKnownSearch(KnownSearch.No);
		comboBox.TypeFilterText(ks.Term);
		int[] doItems = [2, 3, 5];
		int CurPosition = 0;
		comboBox.Navigate();//sets us to the first item
		List<string> expected = new();
		foreach (var pos in doItems) {
			var itemPos = ks.VisibleItemToPosition[pos];
			expected.Add(itemPos.Key);
			comboBox.Navigate(itemPos.Value - CurPosition);
			CurPosition = itemPos.Value;
			if (pos != doItems.Last()) {
				comboBox.PressSpace();
				await Assert.That(comboBox.IsDropdownOpen).IsTrue();
			} else {
				comboBox.PressEnter();
				await Assert.That(comboBox.IsDropdownOpen).IsFalse();
			}

		}
		await Assert.That(comboBox.IsDropdownOpen).IsFalse();
		var selectedItems = comboBox.SelectedItems;
		await Assert.That(selectedItems).IsEquivalentTo(expected);
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task ArrowDown_NavigatesToNextItemAfterClickingAnItem() {
		// Arrange
		var ks = GetKnownSearch(KnownSearch.CANA);
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.TypeFilterText(ks.Term);
		var first = ks.VisibleItemToPosition.First();
		var last = ks.VisibleItemToPosition.Last();
		comboBox.SelectItemByName(first.Key);
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();

		// Act - Navigate down
		comboBox.Navigate(last.Value - first.Value);
		var focusedAfterDown = comboBox.GetFocusedDropdownItem();
		await Assert.That(focusedAfterDown).IsEqualTo(last.Key);
		// Assert - Should have focus on an item (dropdown still open)
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();
		comboBox.PressEnter();
		var selectedItems = comboBox.SelectedItems;
		await Assert.That(selectedItems).IsEquivalentTo([first.Key, last.Key]);
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task ArrowUp_NavigatesToPreviousItem() {
		var ks = GetKnownSearch(KnownSearch.dut);
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.TypeFilterText(ks.Term);
		comboBox.Navigate();//set to first item

		var firstItem = ks.VisibleItemToPosition.Skip(3).First();
		var nextItem = ks.VisibleItemToPosition.Skip(1).First();
		// Navigate down a few items first
		comboBox.Navigate(firstItem.Value);
		var focused = comboBox.GetFocusedDropdownItem();
		var dbgFocused = focused;
		await Assert.That(focused).IsEqualTo(firstItem.Key);
		// Act - Navigate up
		comboBox.Navigate(nextItem.Value - firstItem.Value);
		focused = comboBox.GetFocusedDropdownItem();
		await Assert.That(focused).IsEqualTo(nextItem.Key);

		// Assert - Dropdown should still be open and functional
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task Enter_SelectsFocusedItem() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		var ks = GetKnownSearch(KnownSearch.No);
		comboBox.TypeFilterText(ks.Term);

		comboBox.Navigate();
		var firstItem = ks.VisibleItemToPosition.Skip(1).First();
		// Navigate to first item
		comboBox.Navigate(firstItem.Value);
		var focusedItem = comboBox.GetFocusedDropdownItem();
		await Assert.That(focusedItem).IsEqualTo(firstItem.Key);
		// Act - Press Enter to select
		comboBox.PressEnter();

		// Assert
		var finalCount = comboBox.SelectedItemsCount;
		await Assert.That(finalCount).IsEquivalentTo(1);
		var selectedItems = comboBox.SelectedItems;
		await Assert.That(selectedItems).IsEquivalentTo([focusedItem]);
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task Escape_ClosesDropdown() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.OpenDropdown();

		await Assert.That(comboBox.IsDropdownOpen).IsTrue();

		// Act
		Keyboard.Type(VirtualKeyShort.ESCAPE);
		SleepLong();

		// Assert
		await Assert.That(comboBox.IsDropdownOpen).IsFalse();

		comboBox.TypeFilterText("E");
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();
		Keyboard.Type(VirtualKeyShort.ESCAPE);
		SleepLong();
		await Assert.That(comboBox.IsDropdownOpen).IsFalse();
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task HittingEnter_SelectsFirstItem() {
		var ks = GetKnownSearch(KnownSearch.dut);
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// Filter to show specific items
		comboBox.TypeFilterText(ks.Term);
		comboBox.PressEnter();
	

		// Assert - Should have selected an English-related item
		var selectedItems = comboBox.SelectedItems;
		await Assert.That(selectedItems.Count).IsEqualTo(1);
		await Assert.That(selectedItems.First()).IsEqualTo(ks.VisibleItemToPosition.First().Key);
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task ArrowDownTwiceThenEnter_SelectsCorrectItem() {
		// Arrange
		var ks = GetKnownSearch(KnownSearch.CANA);
		var comboBox = _mainPage.MultiSelectComboBox;
		var targetItem = ks.VisibleItemToPosition.Skip(1).First(); // Second item (position 1)

		// Filter to show specific items
		comboBox.TypeFilterText(ks.Term);


		// Act - Navigate down and select
		comboBox.Navigate(); // Gets us onto the first item
		comboBox.Navigate(targetItem.Value); // Navigate to position 1 (second item)
		comboBox.PressEnter();

		// Assert - Should have selected the second item
		var selectedItems = comboBox.SelectedItems;
		await Assert.That(selectedItems.Count).IsEqualTo(1);
		await Assert.That(selectedItems).IsEquivalentTo([targetItem.Key]);
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task NavigateAndSelect_MultipleTimes_Works() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		var ksAta = GetKnownSearch(KnownSearch.dut);
		var ksSpa = GetKnownSearch(KnownSearch.No);
		var ksZim = GetKnownSearch(KnownSearch.CANA);

		// First selection - select first item
		var firstItem = ksAta.VisibleItemToPosition.First();
		comboBox.TypeFilterText(ksAta.Term);
		comboBox.PressEnter();

		// Second selection - navigate to position 2
		var secondItem = ksSpa.VisibleItemToPosition.Skip(2).First();
		comboBox.TypeFilterText(ksSpa.Term);
		comboBox.Navigate(); // Gets us onto the first item
		comboBox.Navigate(secondItem.Value);
		comboBox.PressEnter();

		// Third selection - navigate to position 1
		var thirdItem = ksZim.VisibleItemToPosition.Skip(1).First();
		comboBox.TypeFilterText(ksZim.Term);
		comboBox.Navigate(); // Gets us onto the first item
		comboBox.Navigate(thirdItem.Value);
		comboBox.PressEnter();

		// Assert
		var selectedItems = comboBox.SelectedItems;
		await Assert.That(selectedItems).IsEquivalentTo([firstItem.Key, secondItem.Key, thirdItem.Key]);
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task ArrowKeysDoNotSelectItems_OnlyNavigate() {
		// Arrange
		var ks = GetKnownSearch(KnownSearch.No)!; // Use Spa with 26 items for plenty of room to navigate
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.TypeFilterText(ks.Term);

		var initialCount = comboBox.SelectedItemsCount;

		// Act - Just navigate without pressing Enter
		comboBox.Navigate(); // Gets us onto the first item
		comboBox.Navigate(5);
		SleepLong();
		comboBox.Navigate(-2);

		// Assert - Selection should not change
		var finalCount = comboBox.SelectedItemsCount;
		await Assert.That(finalCount).IsEqualTo(initialCount);
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task DropdownOpensOnType_WithoutExplicitOpen() {
		// Arrange
		var ks = GetKnownSearch(KnownSearch.CANA);
		var comboBox = _mainPage.MultiSelectComboBox;

		// Act - Just start typing (dropdown should open automatically)
		comboBox.TypeFilterText(ks.Term);
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task Tab_MovesToNextControl() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.ClickToFocus();

		// Act - Press Tab to move to next control
		Keyboard.Type(VirtualKeyShort.TAB);
		SleepLong();
		var focusedElement = Automation?.FocusedElement();

		await Assert.That(focusedElement).IsNotNull();
		await Assert.That(focusedElement!.ControlType).IsEqualTo(ControlType.Button);
		await Assert.That(focusedElement.Name).IsEqualTo("System.Windows.Documents.Underline");


		await Assert.That(comboBox.IsDropdownOpen).IsFalse();
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task EnterWithoutNavigation_DoesNotCrash() {
		// Arrange
		var ks = GetKnownSearch(KnownSearch.dut)!;
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.TypeFilterText(ks.Term);

		// Act - Press Enter without first navigating
		comboBox.PressEnter();

		// Assert - Should not crash, combobox should still be functional
		var selectedItems = comboBox.SelectedItems;
		await Assert.That(selectedItems.Single()).IsEqualTo(ks.VisibleItemToPosition.First().Key);

	}

	public override void TearDown() {
		_mainPage?.Dispose();
		base.TearDown();
	}

}
