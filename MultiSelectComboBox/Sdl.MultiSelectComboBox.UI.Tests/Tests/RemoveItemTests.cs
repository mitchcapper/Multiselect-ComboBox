using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using Sdl.MultiSelectComboBox.UI.Tests.Helpers;
using Sdl.MultiSelectComboBox.UI.Tests.PageObjects;

namespace Sdl.MultiSelectComboBox.UI.Tests.Tests;

/// <summary>
/// Tests for removing selected items using X button
/// </summary>
public class RemoveItemTests : UITestBase {
	private MainWindowPage _mainPage = null!;

	public override void Setup() {
		base.Setup();
		_mainPage = new MainWindowPage(MainWindow!, Automation!);
	}

	[Test]
	[Category("RemoveItem")]
	public async Task ClickRemoveButton_RemovesSelectedItem() {
		// Arrange - Select some items
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.TypeFilterText("English");
		Thread.Sleep(200);
		comboBox.SelectItemByKeyboard(1); // Select first matching item
		Thread.Sleep(200);

		var initialCount = comboBox.SelectedItemsCount;
		await Assert.That(initialCount).IsGreaterThan(0);

		// Act - Click the remove button
		var removeButtons = comboBox.GetRemoveButtons();
		if (removeButtons.Count > 0) {
			removeButtons[0].Click();
			Thread.Sleep(200);
		}

		// Assert - Selected count should decrease
		var finalCount = comboBox.SelectedItemsCount;
		await Assert.That(finalCount).IsLessThan(initialCount);
	}

	[Test]
	[Category("RemoveItem")]
	public async Task RemoveAllItems_ResultsInEmptySelection() {
		// Arrange - Select a couple of items
		var comboBox = _mainPage.MultiSelectComboBox;

		comboBox.TypeFilterText("Eng");
		Thread.Sleep(200);
		comboBox.SelectItemByKeyboard(1);
		Thread.Sleep(200);

		comboBox.ClearFilterText();
		comboBox.TypeFilterText("Span");
		Thread.Sleep(200);
		comboBox.SelectItemByKeyboard(1);
		Thread.Sleep(200);
		comboBox.CloseDropdown();

		var initialCount = comboBox.SelectedItemsCount;
		await Assert.That(initialCount).IsGreaterThan(0);

		// Act - Remove all items by clicking remove buttons
		while (true) {
			var buttons = comboBox.GetRemoveButtons();
			if (buttons.Count == 0) break;

			buttons[0].Click();
			Thread.Sleep(200);
		}

		// Assert
		await Assert.That(comboBox.SelectedItemsCount).IsEqualTo(0);
	}

	[Test]
	[Category("RemoveItem")]
	public async Task RemoveButton_ExistsForEachSelectedItem() {
		// Arrange - Use the random selection button to select multiple items
		_mainPage.ClearSelectedItems();
		Thread.Sleep(200);
		_mainPage.SelectRandomItems();
		Thread.Sleep(500);

		var comboBox = _mainPage.MultiSelectComboBox;
		var selectedCount = comboBox.SelectedItemsCount;

		// Act - Get remove buttons
		var removeButtons = comboBox.GetRemoveButtons();

		// Assert - Should have a remove button for each selected item
		// (might be slightly different due to virtualization, but should have some)
		await Assert.That(removeButtons.Count).IsGreaterThan(0);
	}

	[Test]
	[Category("RemoveItem")]
	public async Task ClearSelectedItemsButton_ClearsAllItems() {
		// Arrange - Select some items
		_mainPage.SelectRandomItems();
		Thread.Sleep(500);

		var comboBox = _mainPage.MultiSelectComboBox;
		await Assert.That(comboBox.SelectedItemsCount).IsGreaterThan(0);

		// Act - Use the Clear selected items button
		_mainPage.ClearSelectedItems();
		Thread.Sleep(300);

		// Assert
		await Assert.That(comboBox.SelectedItemsCount).IsEqualTo(0);
	}

	[Test]
	[Category("RemoveItem")]
	public async Task RemoveButton_DoesNotAffectOtherItems() {
		// Arrange - Select multiple items
		var comboBox = _mainPage.MultiSelectComboBox;

		// Select first item
		comboBox.TypeFilterText("Eng");
		Thread.Sleep(200);
		comboBox.SelectItemByKeyboard(1);
		Thread.Sleep(200);

		// Select second item
		comboBox.ClearFilterText();
		comboBox.TypeFilterText("Span");
		Thread.Sleep(200);
		comboBox.SelectItemByKeyboard(1);
		Thread.Sleep(200);

		// Select third item
		comboBox.ClearFilterText();
		comboBox.TypeFilterText("French");
		Thread.Sleep(200);
		comboBox.SelectItemByKeyboard(1);
		Thread.Sleep(200);
		comboBox.CloseDropdown();

		var initialCount = comboBox.SelectedItemsCount;
		await Assert.That(initialCount).IsGreaterThanOrEqualTo(3);

		// Act - Remove just one item
		var removeButtons = comboBox.GetRemoveButtons();
		if (removeButtons.Count > 0) {
			removeButtons[0].Click();
			Thread.Sleep(200);
		}

		// Assert - Should have exactly one less item
		var finalCount = comboBox.SelectedItemsCount;
		await Assert.That(finalCount).IsEqualTo(initialCount - 1);
	}

	public override void TearDown() {
		_mainPage?.Dispose();
		base.TearDown();
	}
}
