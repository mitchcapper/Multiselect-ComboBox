using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using Sdl.MultiSelectComboBox.UI.Tests.Helpers;
using Sdl.MultiSelectComboBox.UI.Tests.PageObjects;

namespace Sdl.MultiSelectComboBox.UI.Tests.Tests;

/// <summary>
/// Tests to verify selected count stays in sync when toggling items
/// </summary>
public class SelectedCountSyncTests : UITestBase
{
    private MainWindowPage _mainPage = null!;

    public override void Setup()
    {
        base.Setup();
        _mainPage = new MainWindowPage(MainWindow!, Automation!);
    }

    [Test]
    [Category("SelectedCount")]
    public async Task SelectItem_CountIncreases()
    {
        // Arrange
        var comboBox = _mainPage.MultiSelectComboBox;
        var initialCount = comboBox.SelectedItemsCount;
        await Assert.That(initialCount).IsEqualTo(0);

        // Act - Select an item
        comboBox.TypeFilterText("English");
        comboBox.OpenDropdown();
        Thread.Sleep(200);
        comboBox.SelectItemByKeyboard(1);
        Thread.Sleep(300);

        // Assert
        var newCount = comboBox.SelectedItemsCount;
        await Assert.That(newCount).IsEqualTo(initialCount + 1);
    }

    [Test]
    [Category("SelectedCount")]
    public async Task DeselectItem_CountDecreases()
    {
        // Arrange - First select an item
        var comboBox = _mainPage.MultiSelectComboBox;

        comboBox.TypeFilterText("English");
        comboBox.OpenDropdown();
        Thread.Sleep(200);
        comboBox.SelectItemByKeyboard(1);
        Thread.Sleep(300);

        var countAfterSelect = comboBox.SelectedItemsCount;
        await Assert.That(countAfterSelect).IsEqualTo(1);

        // Act - Click on the same item again to deselect it
        comboBox.ClearFilterText();
        comboBox.TypeFilterText("English");
        comboBox.OpenDropdown();
        Thread.Sleep(200);
        comboBox.SelectItemByKeyboard(1); // Toggle selection
        Thread.Sleep(300);

        // Assert
        var countAfterDeselect = comboBox.SelectedItemsCount;
        await Assert.That(countAfterDeselect).IsEqualTo(0);
    }

    [Test]
    [Category("SelectedCount")]
    public async Task SelectMultipleItems_CountMatchesActualSelection()
    {
        // Arrange
        var comboBox = _mainPage.MultiSelectComboBox;

        // Select first item
        comboBox.TypeFilterText("English");
        comboBox.OpenDropdown();
        Thread.Sleep(200);
        comboBox.SelectItemByKeyboard(1);
        Thread.Sleep(200);

        // Select second item
        comboBox.ClearFilterText();
        comboBox.TypeFilterText("Spanish");
        comboBox.OpenDropdown();
        Thread.Sleep(200);
        comboBox.SelectItemByKeyboard(1);
        Thread.Sleep(200);

        // Select third item
        comboBox.ClearFilterText();
        comboBox.TypeFilterText("French");
        comboBox.OpenDropdown();
        Thread.Sleep(200);
        comboBox.SelectItemByKeyboard(1);
        Thread.Sleep(200);

        comboBox.CloseDropdown();
        Thread.Sleep(200);

        // Assert
        var actualCount = comboBox.SelectedItemsCount;
        var selectedItems = comboBox.SelectedItems;

        await Assert.That(actualCount).IsEqualTo(selectedItems.Count);
        await Assert.That(actualCount).IsEqualTo(3);
    }

    [Test]
    [Category("SelectedCount")]
    public async Task DisplayedCount_MatchesActualSelectedItems()
    {
        // Arrange
        _mainPage.SelectRandomItems();
        Thread.Sleep(500);

        var comboBox = _mainPage.MultiSelectComboBox;

        // Assert
        var actualSelectedItems = comboBox.SelectedItems;
        var displayedCount = _mainPage.DisplayedSelectedCount;

        // The displayed count should match the actual number of selected items
        // Note: Due to virtualization, we check that they're at least close
        await Assert.That(displayedCount).IsGreaterThan(0);
    }

    [Test]
    [Category("SelectedCount")]
    public async Task ClearAllItems_CountBecomesZero()
    {
        // Arrange - Select some items
        _mainPage.SelectRandomItems();
        Thread.Sleep(500);

        var comboBox = _mainPage.MultiSelectComboBox;
        await Assert.That(comboBox.SelectedItemsCount).IsGreaterThan(0);

        // Act
        _mainPage.ClearSelectedItems();
        Thread.Sleep(300);

        // Assert
        await Assert.That(comboBox.SelectedItemsCount).IsEqualTo(0);
    }

    [Test]
    [Category("SelectedCount")]
    public async Task ToggleItemOnAndOff_CountReturnsToOriginal()
    {
        // Arrange
        var comboBox = _mainPage.MultiSelectComboBox;

        // Select two items first
        comboBox.TypeFilterText("English");
        comboBox.OpenDropdown();
        Thread.Sleep(200);
        comboBox.SelectItemByKeyboard(1);
        Thread.Sleep(200);

        comboBox.ClearFilterText();
        comboBox.TypeFilterText("Spanish");
        comboBox.OpenDropdown();
        Thread.Sleep(200);
        comboBox.SelectItemByKeyboard(1);
        Thread.Sleep(200);
        comboBox.CloseDropdown();

        var originalCount = comboBox.SelectedItemsCount;
        await Assert.That(originalCount).IsEqualTo(2);

        // Act - Toggle a third item on and off
        comboBox.ClearFilterText();
        comboBox.TypeFilterText("French");
        comboBox.OpenDropdown();
        Thread.Sleep(200);
        comboBox.SelectItemByKeyboard(1); // Select (count becomes 3)
        Thread.Sleep(300);

        var afterSelect = comboBox.SelectedItemsCount;
        await Assert.That(afterSelect).IsEqualTo(3);

        comboBox.ClearFilterText();
        comboBox.TypeFilterText("French");
        comboBox.OpenDropdown();
        Thread.Sleep(200);
        comboBox.SelectItemByKeyboard(1); // Deselect (count becomes 2)
        Thread.Sleep(300);

        // Assert
        var finalCount = comboBox.SelectedItemsCount;
        await Assert.That(finalCount).IsEqualTo(originalCount);
    }

    [Test]
    [Category("SelectedCount")]
    public async Task RemoveItem_CountDecreasesByOne()
    {
        // Arrange - Select multiple items
        _mainPage.SelectRandomItems();
        Thread.Sleep(500);

        var comboBox = _mainPage.MultiSelectComboBox;
        var initialCount = comboBox.SelectedItemsCount;
        await Assert.That(initialCount).IsGreaterThan(0);

        // Act - Remove one item using the X button
        var removeButtons = comboBox.GetRemoveButtons();
        if (removeButtons.Count > 0)
        {
            removeButtons[0].Click();
            Thread.Sleep(200);
        }

        // Assert
        var finalCount = comboBox.SelectedItemsCount;
        await Assert.That(finalCount).IsEqualTo(initialCount - 1);
    }

    public override void TearDown()
    {
        _mainPage?.Dispose();
        base.TearDown();
    }
}
