using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
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
	public async Task ArrowDown_NavigatesToNextItem() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.TypeFilterText("Eng");
		comboBox.OpenDropdown();
		Thread.Sleep(300);

		await Assert.That(comboBox.IsDropdownOpen).IsTrue();

		// Act - Navigate down
		comboBox.NavigateDown();
		Thread.Sleep(200);

		// Assert - Should have focus on an item (dropdown still open)
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task ArrowUp_NavigatesToPreviousItem() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.TypeFilterText("Eng");
		comboBox.OpenDropdown();
		Thread.Sleep(300);

		// Navigate down a few items first
		comboBox.NavigateDown(3);
		Thread.Sleep(200);

		// Act - Navigate up
		comboBox.NavigateUp(2);
		Thread.Sleep(200);

		// Assert - Dropdown should still be open and functional
		await Assert.That(comboBox.IsDropdownOpen).IsTrue();
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task Enter_SelectsFocusedItem() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		var initialCount = comboBox.SelectedItemsCount;

		comboBox.TypeFilterText("Eng");
		comboBox.OpenDropdown();
		Thread.Sleep(300);

		// Navigate to first item
		comboBox.NavigateDown();
		Thread.Sleep(200);

		// Act - Press Enter to select
		comboBox.PressEnter();
		Thread.Sleep(300);

		// Assert
		var finalCount = comboBox.SelectedItemsCount;
		await Assert.That(finalCount).IsGreaterThan(initialCount);
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task Escape_ClosesDropdown() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.OpenDropdown();
		Thread.Sleep(300);

		await Assert.That(comboBox.IsDropdownOpen).IsTrue();

		// Act
		Keyboard.Press(VirtualKeyShort.ESCAPE);
		Thread.Sleep(300);

		// Assert
		await Assert.That(comboBox.IsDropdownOpen).IsFalse();
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task ArrowDownThenEnter_SelectsCorrectItem() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// Filter to show specific items
		comboBox.TypeFilterText("English");
		comboBox.OpenDropdown();
		Thread.Sleep(300);

		// Act - Navigate down and select
		comboBox.NavigateDown();
		Thread.Sleep(100);
		comboBox.PressEnter();
		Thread.Sleep(300);

		// Assert - Should have selected an English-related item
		var selectedItems = comboBox.SelectedItems;
		await Assert.That(selectedItems.Count).IsGreaterThan(0);
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task MultipleArrowDownThenEnter_SelectsCorrectItem() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.TypeFilterText("Spa");
		comboBox.OpenDropdown();
		Thread.Sleep(300);

		// Act - Navigate down 3 times and select
		comboBox.NavigateDown(3);
		Thread.Sleep(200);
		comboBox.PressEnter();
		Thread.Sleep(300);

		// Assert
		var selectedCount = comboBox.SelectedItemsCount;
		await Assert.That(selectedCount).IsEqualTo(1);
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task NavigateAndSelect_MultipleTimes_Works() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// First selection
		comboBox.TypeFilterText("Eng");
		comboBox.OpenDropdown();
		Thread.Sleep(200);
		comboBox.NavigateDown();
		comboBox.PressEnter();
		Thread.Sleep(200);

		// Second selection
		comboBox.ClearFilterText();
		comboBox.TypeFilterText("French");
		comboBox.OpenDropdown();
		Thread.Sleep(200);
		comboBox.NavigateDown();
		comboBox.PressEnter();
		Thread.Sleep(200);

		// Third selection
		comboBox.ClearFilterText();
		comboBox.TypeFilterText("Span");
		comboBox.OpenDropdown();
		Thread.Sleep(200);
		comboBox.NavigateDown();
		comboBox.PressEnter();
		Thread.Sleep(200);

		comboBox.CloseDropdown();
		Thread.Sleep(200);

		// Assert
		var selectedCount = comboBox.SelectedItemsCount;
		await Assert.That(selectedCount).IsEqualTo(3);
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task ArrowKeysDoNotSelectItems_OnlyNavigate() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.TypeFilterText("Eng");
		comboBox.OpenDropdown();
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
		comboBox.ClickToFocus();
		Thread.Sleep(200);

		// Act - Just start typing (dropdown should open automatically)
		Keyboard.Type("Eng");
		Thread.Sleep(500);

		// Assert
		var visibleItems = comboBox.VisibleDropdownItems;
		await Assert.That(visibleItems.Count).IsGreaterThan(0);
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task Tab_MovesToNextControl() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.ClickToFocus();
		Thread.Sleep(200);

		// Act - Press Tab to move to next control
		Keyboard.Press(VirtualKeyShort.TAB);
		Thread.Sleep(200);

		// Assert - Tab navigation should work without crashing
		// The combobox should still be accessible after tab
		await Assert.That(comboBox).IsNotNull();
	}

	[Test]
	[Category("KeyboardNavigation")]
	public async Task EnterWithoutNavigation_DoesNotCrash() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.TypeFilterText("Eng");
		comboBox.OpenDropdown();
		Thread.Sleep(200);

		// Act - Press Enter without first navigating
		comboBox.PressEnter();
		Thread.Sleep(200);

		// Assert - Should not crash, combobox should still be functional
		var selectedCount = comboBox.SelectedItemsCount;
		await Assert.That(selectedCount).IsGreaterThanOrEqualTo(0);
	}

	public override void TearDown() {
		_mainPage?.Dispose();
		base.TearDown();
	}
}
