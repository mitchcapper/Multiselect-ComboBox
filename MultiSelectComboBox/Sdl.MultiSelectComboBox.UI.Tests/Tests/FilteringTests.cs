using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using Sdl.MultiSelectComboBox.UI.Tests.Helpers;
using Sdl.MultiSelectComboBox.UI.Tests.PageObjects;
using static Sdl.MultiSelectComboBox.UI.Tests.Helpers.DelayHelper;

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

		// Assert
		var visibleItems = comboBox.VisibleDropdownItems;
		await Assert.That(visibleItems.Any(item =>
			item.Contains("English (United States)", StringComparison.OrdinalIgnoreCase))).IsTrue();
		await Assert.That(visibleItems.Any(item =>
			item.Contains("English (United Kingdom)", StringComparison.OrdinalIgnoreCase))).IsTrue();
	}

	[Test]
	[Category("Filtering")]
	public async Task TypePartialText_geria_ShowsNigeriaAndAlgeria() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// Act
		comboBox.TypeFilterText("geria");

		// Assert
		var visibleItems = comboBox.VisibleDropdownItems;
		await Assert.That(visibleItems.Any(item =>
			item.Contains("English (Nigeria)", StringComparison.OrdinalIgnoreCase))).IsTrue();
		await Assert.That(visibleItems.Any(item =>
			item.Contains("Arabic (Algeria)", StringComparison.OrdinalIgnoreCase))).IsTrue();
	}

	[Test]
	[Category("Filtering")]
	public async Task TypePartialText_German_ShowsGermanItems() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// Act
		comboBox.TypeFilterText("German");

		// Assert
		var visibleItems = comboBox.VisibleDropdownItems;
		await Assert.That(visibleItems.Any(item =>
			item.Contains("German (Germany)", StringComparison.OrdinalIgnoreCase))).IsTrue();
	}

	[Test]
	[Category("Filtering")]
	public async Task TypePartialText_French_ShowsFrenchItems() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// Act
		comboBox.TypeFilterText("French");
		comboBox.WaitForDropdownOpen();
		// Assert
		var visibleItems = comboBox.VisibleDropdownItems;
		await Assert.That(visibleItems.Any(item =>
			item.Contains("French (France)", StringComparison.OrdinalIgnoreCase))).IsTrue();
	}

	[Test]
	[Category("Filtering")]
	public async Task TypeNonExistentText_ShowsNoItems() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// Act
		comboBox.TypeFilterText("xyznonexistent123");
		// Assert
		var visibleItems = comboBox.VisibleDropdownItems;
		await Assert.That(visibleItems.Count).IsEqualTo(0);
	}

	[Test]
	[Category("Filtering")]
	public async Task ClearFilter_ShowsAllItems() {
		// Arrange
		var ks = GetKnownSearch(KnownSearch.CANA); // Small result set for predictable filtering
		var comboBox = _mainPage.MultiSelectComboBox;

		// First filter to a subset
		comboBox.TypeFilterText(ks.Term);

		var filteredItems = comboBox.VisibleDropdownItems.ToList();
		await Assert.That(filteredItems).Contains(ks.VisibleItemToPosition.First().Key);

		// Act - Clear the filter
		comboBox.ClearFilterTextAnyItemsUsingBackspace();
		
		comboBox.OpenDropdown();

		// Assert - Unfiltered list should include the filtered item plus broader set
		var unfilteredItems = comboBox.AllDropdownItems.ToList();
		await Assert.That(unfilteredItems.Any(item => item.Equals("Italian (Italy)", StringComparison.OrdinalIgnoreCase))).IsTrue(); // doesn't work as not in first few
		await Assert.That(unfilteredItems.Any(item => item.Equals("English (United States)", StringComparison.OrdinalIgnoreCase))).IsTrue();
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
		comboBox.SetFilterTextClearItems(searchText);

		// Assert
		var visibleItems = comboBox.VisibleDropdownItems;
		await Assert.That(visibleItems.Any(item =>
			item.Contains(expectedMatch, StringComparison.OrdinalIgnoreCase))).IsTrue();
	}

	[Test]
	[Category("Filtering")]
	public async Task FilterIsCaseInsensitive() {
		// Arrange
		var comboBox = _mainPage.MultiSelectComboBox;

		// Act - Type lowercase
		comboBox.TypeFilterText("unit");
		var lowerCaseResults = comboBox.VisibleDropdownItems.ToList();

		// Clear and try uppercase
		comboBox.ClearFilterTextAnyItemsUsingBackspace();
		comboBox.TypeFilterText("UNIT");

		var upperCaseResults = comboBox.VisibleDropdownItems.ToList();

		var lowerOrdered = lowerCaseResults.OrderBy(i => i).ToList();
		var upperOrdered = upperCaseResults.OrderBy(i => i).ToList();

		await Assert.That(lowerOrdered).IsEquivalentTo(upperOrdered);
		await Assert.That(lowerCaseResults.Any(item => item.Contains("English (United States)", StringComparison.OrdinalIgnoreCase))).IsTrue();
	}

	public override void TearDown() {
		_mainPage?.Dispose();
		base.TearDown();
	}
}
