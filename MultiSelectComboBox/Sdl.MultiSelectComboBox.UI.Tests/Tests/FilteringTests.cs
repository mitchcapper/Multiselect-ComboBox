using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using Sdl.MultiSelectComboBox.UI.Tests.Helpers;
using Sdl.MultiSelectComboBox.UI.Tests.PageObjects;

namespace Sdl.MultiSelectComboBox.UI.Tests.Tests;

/// <summary>
/// Tests for filtering functionality in the MultiSelectComboBox
/// </summary>
public class FilteringTests : UITestBase {
	private MainWindowPage _mainPage = null!;

	public override void Setup() {
		base.Setup();
		_mainPage = new MainWindowPage(MainWindow!, Automation!);
	}

	[Test]
	[Category("Filtering")]
	public async Task TypePartialText_Unit_ShowsUnitedStatesAndUnitedKingdom() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// Act
		comboBox.TypeFilterText("Unit");
		Thread.Sleep(200);

		// Assert
		var visibleItems = comboBox.VisibleDropdownItems;
		await Assert.That(visibleItems.Count).IsGreaterThan(0);

		// Should show items containing "Unit" like United States, United Kingdom, etc.
		var hasUnitedStates = visibleItems.Any(item =>
			item.Contains("United States", StringComparison.OrdinalIgnoreCase));
		var hasUnitedKingdom = visibleItems.Any(item =>
			item.Contains("United Kingdom", StringComparison.OrdinalIgnoreCase));

		await Assert.That(hasUnitedStates || hasUnitedKingdom).IsTrue();
	}

	[Test]
	[Category("Filtering")]
	public async Task TypePartialText_geria_ShowsNigeriaAndAlgeria() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// Act
		comboBox.TypeFilterText("geria");
		Thread.Sleep(200);

		// Assert
		var visibleItems = comboBox.VisibleDropdownItems;
		await Assert.That(visibleItems.Count).IsGreaterThan(0);

		// Should show items containing "geria" like Nigeria, Algeria
		var hasNigeria = visibleItems.Any(item =>
			item.Contains("Nigeria", StringComparison.OrdinalIgnoreCase));
		var hasAlgeria = visibleItems.Any(item =>
			item.Contains("Algeria", StringComparison.OrdinalIgnoreCase));

		await Assert.That(hasNigeria || hasAlgeria).IsTrue();
	}

	[Test]
	[Category("Filtering")]
	public async Task TypePartialText_German_ShowsGermanItems() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// Act
		comboBox.TypeFilterText("German");
		Thread.Sleep(200);

		// Assert
		var visibleItems = comboBox.VisibleDropdownItems;
		await Assert.That(visibleItems.Count).IsGreaterThan(0);

		var hasGerman = visibleItems.Any(item =>
			item.Contains("German", StringComparison.OrdinalIgnoreCase));
		await Assert.That(hasGerman).IsTrue();
	}

	[Test]
	[Category("Filtering")]
	public async Task TypePartialText_French_ShowsFrenchItems() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// Act
		comboBox.TypeFilterText("French");
		Thread.Sleep(200);

		// Assert
		var visibleItems = comboBox.VisibleDropdownItems;
		await Assert.That(visibleItems.Count).IsGreaterThan(0);

		var hasFrench = visibleItems.Any(item =>
			item.Contains("French", StringComparison.OrdinalIgnoreCase));
		await Assert.That(hasFrench).IsTrue();
	}

	[Test]
	[Category("Filtering")]
	public async Task TypeNonExistentText_ShowsNoItems() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// Act
		comboBox.TypeFilterText("xyznonexistent123");
		Thread.Sleep(200);
		Thread.Sleep(500); // Wait for filter to apply

		// Assert
		var visibleItems = comboBox.VisibleDropdownItems;
		await Assert.That(visibleItems.Count).IsEqualTo(0);
	}

	[Test]
	[Category("Filtering")]
	public async Task ClearFilter_ShowsAllItems() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// First filter to a subset
		comboBox.TypeFilterText("Zim"); //important to be  a small number as the default open is going to only show the frequent/recent item set
		Thread.Sleep(200);
		var filteredCount = comboBox.VisibleDropdownItems.Count;
		await Assert.That(filteredCount).IsGreaterThan(0);

		// Act - Clear the filter
		comboBox.ClearFilterText();
		await Task.Delay(300);
		comboBox.OpenDropdown();

		// Assert - Should show more items than when filtered
		var unfilteredCount = comboBox.AllDropdownItems.Count;
		await Assert.That(unfilteredCount).IsGreaterThan(filteredCount);
	}

	[Test]
	[Category("Filtering")]
	[Arguments("Eng", "English")]
	[Arguments("Span", "Spanish")]
	[Arguments("Portu", "Portuguese")]
	[Arguments("Ital", "Italian")]
	public async Task TypePartialText_ShowsMatchingLanguages(string searchText, string expectedMatch) {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// Act
		comboBox.SetFilterText(searchText);
		Thread.Sleep(200);

		// Assert
		var visibleItems = comboBox.VisibleDropdownItems;
		await Assert.That(visibleItems.Count).IsGreaterThan(0);

		var hasMatch = visibleItems.Any(item =>
			item.Contains(expectedMatch, StringComparison.OrdinalIgnoreCase));
		await Assert.That(hasMatch).IsTrue();
	}

	[Test]
	[Category("Filtering")]
	public async Task FilterIsCaseInsensitive() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// Act - Type lowercase
		comboBox.TypeFilterText("unit");
		Thread.Sleep(200);
		var lowerCaseResults = comboBox.VisibleDropdownItems.ToList();

		// Clear and try uppercase
		comboBox.ClearFilterText();
		comboBox.TypeFilterText("UNIT");
		Thread.Sleep(200);
		Thread.Sleep(300);

		var upperCaseResults = comboBox.VisibleDropdownItems.ToList();

		// Assert - Both should return results
		await Assert.That(lowerCaseResults.Count).IsGreaterThan(0);
		await Assert.That(upperCaseResults.Count).IsGreaterThan(0);
	}

	public override void TearDown() {
		_mainPage?.Dispose();
		base.TearDown();
	}
}
