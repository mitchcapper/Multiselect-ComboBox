using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using Sdl.MultiSelectComboBox.UI.Tests.Helpers;
using Sdl.MultiSelectComboBox.UI.Tests.PageObjects;

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
		comboBox.TypeFilterText("Spa");
		Thread.Sleep(300);
		comboBox.NavigateDown(3);
		comboBox.PressSpace();
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();
		comboBox.NavigateDown(1);
		comboBox.PressSpace();
		comboBox.NavigateDown(2);
		comboBox.PressEnter();
		await Assert.That(comboBox.IsDropdownOpen).IsFalse();
		var selectedItems = comboBox.SelectedItems;
		await Assert.That(selectedItems).IsEquivalentTo(["Catalan (Spain)", "Galician (Spain)", "Spanish (Bolivia)"]);
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task ArrowDown_NavigatesToNextItemAfterClickingAnItem() {
		// Arrange
		var itm1 = "Spanish (Chile)";
		var itm2 = "Spanish (Costa Rica)";
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.TypeFilterText("Spa");
		Thread.Sleep(300);
		comboBox.SelectItemByName(itm1);
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();

		// Act - Navigate down
		comboBox.NavigateDown(2);
		
		Thread.Sleep(100);
		var focusedAfterDown = comboBox.GetFocusedDropdownItem();
		await Assert.That(focusedAfterDown).IsEqualTo(itm2);
		// Assert - Should have focus on an item (dropdown still open)
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();
		comboBox.PressEnter();
		Thread.Sleep(200);
		var selectedItems = comboBox.SelectedItems;
		await Assert.That(selectedItems).IsEquivalentTo([itm1,itm2]);
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task ArrowUp_NavigatesToPreviousItem() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.TypeFilterText("Eng");
		Thread.Sleep(300);

		// Navigate down a few items first
		comboBox.NavigateDown(3);
		Thread.Sleep(200);

		// Act - Navigate up
		comboBox.NavigateUp(2);
		Thread.Sleep(200);
		var focusedAfterUp = comboBox.GetFocusedDropdownItem();

		await Assert.That(focusedAfterUp).IsEqualTo("English (United States)");

		// Assert - Dropdown should still be open and functional
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task Enter_SelectsFocusedItem() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		comboBox.TypeFilterText("Eng");
		Thread.Sleep(300);

		// Navigate to first item
		comboBox.NavigateDown(6);
		Thread.Sleep(200);
		var focusedItem = comboBox.GetFocusedDropdownItem();
		// Act - Press Enter to select
		comboBox.PressEnter();
		Thread.Sleep(300);

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
		Thread.Sleep(200);

		await Assert.That(comboBox.IsDropdownOpen).IsTrue();

		// Act
		Keyboard.Type(VirtualKeyShort.ESCAPE);
		Thread.Sleep(200);

		// Assert
		await Assert.That(comboBox.IsDropdownOpen).IsFalse();

		comboBox.TypeFilterText("E");
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();
		Keyboard.Type(VirtualKeyShort.ESCAPE);
		Thread.Sleep(200);
		await Assert.That(comboBox.IsDropdownOpen).IsFalse();
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task HittingEnter_SelectsFirstItem() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// Filter to show specific items
		comboBox.TypeFilterText("Z");
		comboBox.PressEnter();
		Thread.Sleep(300);

		// Assert - Should have selected an English-related item
		var selectedItems = comboBox.SelectedItems;
		await Assert.That(selectedItems.Count).IsEqualTo(1);
		await Assert.That(selectedItems.First()).IsEqualTo("Asu (Tanzania)");
	}
	
	[Test]
	[Category("KeyboardNavigation")]
	public async Task ArrowDownTwiceThenEnter_SelectsCorrectItem() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// Filter to show specific items
		comboBox.TypeFilterText("Tan");
		Thread.Sleep(300);

		// Act - Navigate down and select
		comboBox.NavigateDown(2); //down two should select Asu (Tanzania)
		Thread.Sleep(100);
		comboBox.PressEnter();
		Thread.Sleep(300);

		// Assert - Should have selected an English-related item
		var selectedItems = comboBox.SelectedItems;
		await Assert.That(selectedItems.Count).IsEqualTo(1);
		await Assert.That(selectedItems).IsEquivalentTo(["Asu (Tanzania)"]);

	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task NavigateAndSelect_MultipleTimes_Works() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// First selection
		comboBox.TypeFilterText("Eng");
		Thread.Sleep(200);
		comboBox.PressEnter();
		Thread.Sleep(200);

		// Second selection
		comboBox.TypeFilterText("French");
		Thread.Sleep(200);
		comboBox.NavigateDown(3);
		comboBox.PressEnter();
		Thread.Sleep(200);

		// Third selection
		comboBox.TypeFilterText("Span");
		Thread.Sleep(200);
		comboBox.NavigateDown(2);
		comboBox.PressEnter();
		Thread.Sleep(200);


		// Assert
		var selectedItems = comboBox.SelectedItems;
		await Assert.That(selectedItems).IsEquivalentTo(["English (United States)", "French (Belgium)", "Spanish (Bolivia)"]);

	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task ArrowKeysDoNotSelectItems_OnlyNavigate() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.TypeFilterText("Eng");
		Thread.Sleep(200);

		var initialCount = comboBox.SelectedItemsCount;

		// Act - Just navigate without pressing Enter
		comboBox.NavigateDown(5);
		Thread.Sleep(200);
		comboBox.NavigateUp(2);
		Thread.Sleep(200);

		// Assert - Selection should not change
		var finalCount = comboBox.SelectedItemsCount;
		await Assert.That(finalCount).IsEqualTo(initialCount);
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task DropdownOpensOnType_WithoutExplicitOpen() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// Act - Just start typing (dropdown should open automatically)
		comboBox.TypeFilterText("Eng");
		Thread.Sleep(300);
		// Assert
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task Tab_MovesToNextControl() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.ClickToFocus();
		Thread.Sleep(200);

		// Act - Press Tab to move to next control
		Keyboard.Type(VirtualKeyShort.TAB);
		Thread.Sleep(200);
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
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.TypeFilterText("Eng");
		Thread.Sleep(200);

		// Act - Press Enter without first navigating
		comboBox.PressEnter();
		Thread.Sleep(200);

		// Assert - Should not crash, combobox should still be functional
		var selectedItems = comboBox.SelectedItems;
		await Assert.That(selectedItems).IsEquivalentTo(["English (United States)"]);
	}

	public override void TearDown() {
		_mainPage?.Dispose();
		base.TearDown();
	}

}
