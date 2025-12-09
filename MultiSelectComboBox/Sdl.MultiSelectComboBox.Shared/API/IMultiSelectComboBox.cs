using System;
using System.Collections;
using System.Collections.Generic;

namespace Sdl.MultiSelectComboBox.API
{
    /// <summary>
    /// Interface for the MultiSelectComboBox control
    /// </summary>
    public interface IMultiSelectComboBox
    {
        /// <summary>
        /// Gets or sets the source collection of items to display in the combo box
        /// </summary>
        IEnumerable ItemsSource { get; set; }

        /// <summary>
        /// Gets the collection of currently selected items
        /// </summary>
        IEnumerable SelectedItems { get; }

        /// <summary>
        /// Gets or sets the filter text used to search for items
        /// </summary>
        string FilterText { get; set; }

        /// <summary>
        /// Gets or sets the path to the property used for display
        /// </summary>
        string DisplayMemberPath { get; set; }

        /// <summary>
        /// Gets or sets the custom filter service
        /// </summary>
        IFilterService FilterService { get; set; }

        /// <summary>
        /// Gets or sets the custom suggestion provider
        /// </summary>
        ISuggestionProvider SuggestionProvider { get; set; }

        /// <summary>
        /// Gets or sets the auto-complete service
        /// </summary>
        IAutoCompleteService AutoCompleteService { get; set; }

        /// <summary>
        /// Gets a value indicating whether the dropdown is currently open
        /// </summary>
        bool IsDropDownOpen { get; set; }

        /// <summary>
        /// Selects all items in the combo box
        /// </summary>
        void SelectAll();

        /// <summary>
        /// Clears all selected items
        /// </summary>
        void UnselectAll();

        /// <summary>
        /// Updates the items displayed in the dropdown to apply filtering
        /// </summary>
        void UpdateFilter();
    }
}
