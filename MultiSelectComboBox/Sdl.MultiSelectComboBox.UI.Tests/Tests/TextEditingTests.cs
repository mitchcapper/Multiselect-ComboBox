using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using Sdl.MultiSelectComboBox.UI.Tests.Helpers;
using Sdl.MultiSelectComboBox.UI.Tests.PageObjects;

namespace Sdl.MultiSelectComboBox.UI.Tests.Tests;

/// <summary>
/// Tests for text editing (backspace, delete) in the MultiSelectComboBox filter
/// </summary>
public class TextEditingTests : UITestBase
{
    private MainWindowPage _mainPage = null!;
    
    public override void Setup()
    {
        base.Setup();
        _mainPage = new MainWindowPage(MainWindow!, Automation!);
    }

    [Test]
    [Category("TextEditing")]
    public async Task BackspaceKey_ErasesCharacters()
    {
        // Arrange
        var comboBox = _mainPage.MultiSelectComboBox;
        comboBox.TypeFilterText("United");
        Thread.Sleep(200);

        var initialText = comboBox.FilterText;
        await Assert.That(initialText.Length).IsGreaterThan(0);

        // Act - Press backspace 3 times
        comboBox.PressBackspace(3);
        Thread.Sleep(200);

        // Assert - Filter text should be shorter
        var newText = comboBox.FilterText;
        await Assert.That(newText.Length).IsLessThan(initialText.Length);
    }

    [Test]
    [Category("TextEditing")]
    public async Task BackspaceKey_ErasesEntireFilterText()
    {
        // Arrange
        var comboBox = _mainPage.MultiSelectComboBox;
        var textToType = "Test";
        comboBox.TypeFilterText(textToType);
        Thread.Sleep(200);

        // Act - Press backspace for each character plus a few extra
        comboBox.PressBackspace(textToType.Length + 2);
        Thread.Sleep(200);

        // Assert - Filter text should be empty
        var finalText = comboBox.FilterText;
        await Assert.That(finalText.Length).IsEqualTo(0);
    }

    [Test]
    [Category("TextEditing")]
    public async Task TypeText_ThenBackspace_ThenTypeMore_Works()
    {
        // Arrange
        var comboBox = _mainPage.MultiSelectComboBox;

        // Act
        comboBox.TypeFilterText("Germ");
        Thread.Sleep(200);
        comboBox.PressBackspace(2); // Remove "rm"
        Thread.Sleep(200);

        // Type something else
        comboBox.TypeFilterText("rman"); // Now should be "German"
        comboBox.OpenDropdown();
        Thread.Sleep(300);

        // Assert
        var visibleItems = comboBox.VisibleDropdownItems;
        var hasGerman = visibleItems.Any(item =>
            item.Contains("German", StringComparison.OrdinalIgnoreCase));
        await Assert.That(hasGerman).IsTrue();
    }

    [Test]
    [Category("TextEditing")]
    public async Task BackspaceOnEmptyFilter_DoesNotCrash()
    {
        // Arrange
        var comboBox = _mainPage.MultiSelectComboBox;
        comboBox.ClickToFocus();

        // Act - Press backspace multiple times on empty filter
        comboBox.PressBackspace(5);
        Thread.Sleep(200);

        // Assert - Should not crash and dropdown should still work
        comboBox.OpenDropdown();
        Thread.Sleep(200);

        await Assert.That(comboBox.IsDropdownOpen).IsTrue();
    }

    [Test]
    [Category("TextEditing")]
    public async Task FilterTextUpdatesAsUserTypes()
    {
        // Arrange
        var comboBox = _mainPage.MultiSelectComboBox;
        comboBox.ClickToFocus();

        // Act & Assert - Type character by character and verify filter updates
        comboBox.TypeFilterText("U");
        Thread.Sleep(100);
        var afterU = comboBox.FilterText;

        comboBox.TypeFilterText("n");
        Thread.Sleep(100);
        var afterUn = comboBox.FilterText;

        comboBox.TypeFilterText("i");
        Thread.Sleep(100);
        var afterUni = comboBox.FilterText;

        // Assert
        await Assert.That(afterU.Length).IsGreaterThanOrEqualTo(1);
        await Assert.That(afterUn.Length).IsGreaterThanOrEqualTo(2);
        await Assert.That(afterUni.Length).IsGreaterThanOrEqualTo(3);
    }

    [Test]
    [Category("TextEditing")]
    public async Task ClearFilterText_RemovesAllText()
    {
        // Arrange
        var comboBox = _mainPage.MultiSelectComboBox;
        comboBox.TypeFilterText("SomeFilterText");
        Thread.Sleep(200);

        await Assert.That(comboBox.FilterText.Length).IsGreaterThan(0);

        // Act
        comboBox.ClearFilterText();
        Thread.Sleep(200);

        // Assert
        await Assert.That(comboBox.FilterText.Length).IsEqualTo(0);
    }

    [Test]
    [Category("TextEditing")]
    public async Task BackspaceAfterSelection_AffectsFilterNotSelection()
    {
        // Arrange
        var comboBox = _mainPage.MultiSelectComboBox;

        // Select an item first
        comboBox.TypeFilterText("English");
        comboBox.OpenDropdown();
        Thread.Sleep(200);
        comboBox.SelectItemByKeyboard(1);
        Thread.Sleep(200);

        var selectedCountBefore = comboBox.SelectedItemsCount;

        // Now type some filter text and backspace
        comboBox.TypeFilterText("test");
        Thread.Sleep(200);
        comboBox.PressBackspace(4);
        Thread.Sleep(200);

        // Assert - Selected items should not be affected by backspace on filter
        var selectedCountAfter = comboBox.SelectedItemsCount;
        await Assert.That(selectedCountAfter).IsEqualTo(selectedCountBefore);
    }

    public override void TearDown()
    {
        _mainPage?.Dispose();
        base.TearDown();
    }
}
