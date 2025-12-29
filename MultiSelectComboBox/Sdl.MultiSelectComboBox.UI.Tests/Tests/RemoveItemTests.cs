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
		_mainPage.ClearSelectedItems();
		Thread.Sleep(200);
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.SelectItemsByFilter("English");

		var initialCount = comboBox.SelectedItemsCount;
		await Assert.That(initialCount).IsEqualTo(1);
		var removeButtons = comboBox.GetRemoveButtons();
		await Assert.That(removeButtons.Count).IsEqualTo(1);

		// Act - Click the remove button
		removeButtons[0].Click();
		Thread.Sleep(200);

		// Assert - Selected count should decrease
		var finalCount = comboBox.SelectedItemsCount;
		await Assert.That(finalCount).IsEqualTo(0);
	}

	[Test]
	[Category("RemoveItem")]
	public async Task RemoveAllItems_ResultsInEmptySelection() {
		// Arrange - Select a couple of items
		_mainPage.ClearSelectedItems();
		Thread.Sleep(200);
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.SelectItemsByFilter("English", "Spanish");
		comboBox.CloseDropdown();

		var initialCount = comboBox.SelectedItemsCount;
		await Assert.That(initialCount).IsEqualTo(2);
		var removeButtons = comboBox.GetRemoveButtons();
		await Assert.That(removeButtons.Count).IsEqualTo(2);

		// Act - Remove all items by clicking remove buttons
		while (comboBox.SelectedItemsCount > 0) {
			var buttons = comboBox.GetRemoveButtons();
			buttons[0].Click();
			Thread.Sleep(200);
		}

		// Assert
		await Assert.That(comboBox.SelectedItemsCount).IsEqualTo(0);
	}

	[Test]
	[Category("RemoveItem")]
	public async Task RemoveButton_ExistsForEachSelectedItem() {
		// Arrange - Select a known set of items
		_mainPage.ClearSelectedItems();
		Thread.Sleep(200);
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.SelectItemsByFilter("English", "Spanish", "French");
		var selectedCount = comboBox.SelectedItemsCount;
		await Assert.That(selectedCount).IsEqualTo(3);

		// Act - Get remove buttons
		var removeButtons = comboBox.GetRemoveButtons();

		// Assert - Should have a remove button for each selected item
		await Assert.That(removeButtons.Count).IsEqualTo(selectedCount);
	}

	[Test]
	[Category("RemoveItem")]
	public async Task ClearSelectedItemsButton_ClearsAllItems() {
		// Arrange - Select some items
		_mainPage.ClearSelectedItems();
		Thread.Sleep(200);
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.SelectItemsByFilter("English", "Spanish");
		await Assert.That(comboBox.SelectedItemsCount).IsEqualTo(2);

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
		_mainPage.ClearSelectedItems();
		Thread.Sleep(200);

		// Select first item
		comboBox.SelectItemsByFilter("English", "Spanish", "French");
		comboBox.CloseDropdown();

		var initialCount = comboBox.SelectedItemsCount;
		await Assert.That(initialCount).IsEqualTo(3);

		// Act - Remove just one item
		var removeButtons = comboBox.GetRemoveButtons();
		await Assert.That(removeButtons.Count).IsEqualTo(3);
		removeButtons[0].Click();
		Thread.Sleep(200);

		// Assert - Should have exactly one less item
		var finalCount = comboBox.SelectedItemsCount;
		await Assert.That(finalCount).IsEqualTo(initialCount - 1);
	}

	public override void TearDown() {
		_mainPage?.Dispose();
		base.TearDown();
	}
}
