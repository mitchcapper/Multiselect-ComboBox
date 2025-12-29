using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using Sdl.MultiSelectComboBox.UI.Tests.Helpers;
using Sdl.MultiSelectComboBox.UI.Tests.PageObjects;
using FlaUI.Core.Input;

namespace Sdl.MultiSelectComboBox.UI.Tests.Tests;

/// <summary>
/// Tests to verify selected count stays in sync when toggling items
/// </summary>
public class SelectedCountSyncTests : UITestBase {
	private MainWindowPage _mainPage = null!;

	public override void Setup() {
		base.Setup();
		_mainPage = new MainWindowPage(MainWindow!, Automation!);
	}

	[Test]
	[Category("SelectedCount")]
	public async Task SelectItem_CountIncreases() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		_mainPage.ClearSelectedItems();
		Thread.Sleep(200);
		var initialCount = comboBox.SelectedItemsCount;
		await Assert.That(initialCount).IsEqualTo(0);

		// Act - Select an item
		comboBox.SelectItemsByFilter("English");
		Thread.Sleep(300);

		// Assert
		var newCount = comboBox.SelectedItemsCount;
		await Assert.That(newCount).IsEqualTo(initialCount + 1);
	}

	[Test]
	[Category("SelectedCount")]
	public async Task DeselectItem_CountDecreases() {
		// Arrange - First select an item
		var comboBox = _mainPage.MultiSelectComboBox;
		_mainPage.ClearSelectedItems();
		Thread.Sleep(200);

		comboBox.SelectItemsByName("English (United States)");
		Thread.Sleep(200);
		await Assert.That(comboBox.SelectedItemsCount).IsEqualTo(1);

		// Act - Toggle the same item off
		comboBox.SelectItemsByName("English (United States)");
		Thread.Sleep(200);

		// Assert
		var countAfterDeselect = comboBox.SelectedItemsCount;
		await Assert.That(countAfterDeselect).IsEqualTo(0);

	}

	[Test]
	[Category("SelectedCount")]
	public async Task SelectMultipleItems_CountMatchesActualSelection() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;
		var ks2 = GetKnownSearch(KnownSearch.CANA);
		comboBox.SetFilterTextClearItems(ks2.Term);
		Thread.Sleep(200);
		var toSel = ks2.VisibleItemToPosition.Take(3).Select(a=>a.Key).ToArray();
		comboBox.SelectItemsByName(toSel);

		comboBox.CloseDropdown();
		Thread.Sleep(200);

		// Assert
		var actualCount = comboBox.SelectedItemsCount;
		var selectedItems = comboBox.SelectedItems;

		await Assert.That(actualCount).IsEqualTo(selectedItems.Count);
		await Assert.That(toSel).IsEquivalentTo(selectedItems);
	}

	[Test]
	[Category("SelectedCount")]
	public async Task DisplayedCount_MatchesActualSelectedItems() {
		// Arrange
		_mainPage.ClearSelectedItems();
		Thread.Sleep(200);
		var comboBox = _mainPage.MultiSelectComboBox;
		var ks2 = GetKnownSearch(KnownSearch.CANA);
		comboBox.SetFilterTextClearItems(ks2.Term);
		Thread.Sleep(200);
		var toSel = ks2.VisibleItemToPosition.Take(3).Select(a=>a.Key).ToArray();
		comboBox.SelectItemsByName(toSel);
		var actualSelectedItems = comboBox.SelectedItems;

		var displayedCount = _mainPage.DisplayedSelectedCount;

		// The displayed count should match the actual number of selected items
		await Assert.That(displayedCount).IsEqualTo(actualSelectedItems.Count);
	}

	[Test]
	[Category("SelectedCount")]
	public async Task ClearAllItems_CountBecomesZero() {
		// Arrange - Select some items
		_mainPage.ClearSelectedItems();
		Thread.Sleep(200);
		var comboBox = _mainPage.MultiSelectComboBox;
		comboBox.TypeFilterText("Spa");
		comboBox.SelectItemsByName("Catalan (Spain)", "Galician (Spain)", "Spanish (Bolivia)");
		await Assert.That(comboBox.SelectedItemsCount).IsEqualTo(3);
		comboBox.CloseDropdown();
		// Act
		_mainPage.ClearSelectedItems();
		Thread.Sleep(300);

		// Assert
		await Assert.That(comboBox.SelectedItemsCount).IsEqualTo(0);
	}

	[Test]
	[Category("SelectedCount")]
	public async Task ToggleItemOnAndOff_CountReturnsToOriginal() {
		// Arrange
		var ks1 = GetKnownSearch(KnownSearch.ata);
		var ks2 = GetKnownSearch(KnownSearch.CANA);
		var ks3 = GetKnownSearch(KnownSearch.No);
		var comboBox = _mainPage.MultiSelectComboBox;

		// Select two items first
		comboBox.TypeFilterText(ks1.Term);
		Thread.Sleep(200);
		comboBox.SelectItemByKeyboard(1);
		Thread.Sleep(200);
		comboBox.TypeFilterText(ks2.Term);
		Thread.Sleep(200);
		comboBox.SelectItemByKeyboard(1);
		Thread.Sleep(200);
		comboBox.CloseDropdown();

		var originalCount = comboBox.SelectedItemsCount;
		await Assert.That(originalCount).IsEqualTo(2);

		comboBox.TypeFilterText(ks3.Term);
		Thread.Sleep(200);
		comboBox.SelectItemByKeyboard(1); // Select (count becomes 3)
		Thread.Sleep(300);

		var afterSelect = comboBox.SelectedItemsCount;
		await Assert.That(afterSelect).IsEqualTo(3);

		comboBox.TypeFilterText(ks3.Term);
		Thread.Sleep(200);
		comboBox.SelectItemByKeyboard(1); // Deselect (count becomes 2)
		Thread.Sleep(300);

		// Assert
		var finalCount = comboBox.SelectedItemsCount;
		await Assert.That(finalCount).IsEqualTo(originalCount);
	}

	[Test]
	[Category("SelectedCount")]
	public async Task RemoveItem_CountDecreasesByOne() {
		// Arrange - Select multiple items
		_mainPage.SelectRandomItems();
		Thread.Sleep(500);

		var comboBox = _mainPage.MultiSelectComboBox;
		var initialCount = comboBox.SelectedItemsCount;
		await Assert.That(initialCount).IsEqualTo(20);

		comboBox.ClickToFocus();//must be focused to show remove buttons
		Thread.Sleep(200);
		// Act - Remove one item using the X button
		var removeButtons = comboBox.GetRemoveButtons();
		await Assert.That(removeButtons.Count).IsEqualTo(20);
		var btn = removeButtons[0];
		btn.Click();
		Thread.Sleep(200);


		// Assert
		var finalCount = comboBox.SelectedItemsCount;
		await Assert.That(finalCount).IsEqualTo(initialCount - 1);
	}

	public override void TearDown() {
		_mainPage?.Dispose();
		base.TearDown();
	}
}
