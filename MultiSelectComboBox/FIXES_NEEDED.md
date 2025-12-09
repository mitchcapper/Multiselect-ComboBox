# Critical Fixes Needed for MultiSelectComboBox

## Issues Found

### WPF:
1. Arrow keys don't navigate dropdown - they call `OpenDropDownList()` even when already open
2. X button doesn't work when dropdown is open - calls broken `UpdateSelectedItemsContainer`

### WinUI:
1. Selection works but shows "0 selected" - binding issue
2. X button doesn't work - no remove button handler in WinUI pointer event
3. Focus stealing/bouncing when dropdown opens after typing

---

## Fix #1: WPF Arrow Key Navigation (Line 1922-1923)

**File**: `Sdl.MultiSelectComboBox.Shared\Controls\MultiSelectComboBox.cs`

**Replace**:
```csharp
System.Diagnostics.Debug.WriteLine($"[WINUI] Calling OpenDropDownList");
OpenDropDownList();
```

**With**:
```csharp
// Only open dropdown if it's not already open
// If already open, let dropdown's key handler navigate
if (!IsDropDownOpen)
{
    System.Diagnostics.Debug.WriteLine($"[WINUI] Calling OpenDropDownList");
    OpenDropDownList();
}
else
{
#if !WINUI
    // For WPF: Set focus to dropdown so arrow keys work for navigation
    if (DropdownListBox != null && !DropdownListBox.IsKeyboardFocusWithin)
    {
        System.Diagnostics.Debug.WriteLine($"[WPF] Setting focus to dropdown for navigation");
        DropdownListBox.Focus();
        if (DropdownListBox.Items.Count > 0 && DropdownListBox.SelectedIndex < 0)
        {
            DropdownListBox.SelectedIndex = 0;
        }
    }
#endif
}
```

---

## Fix #2: Fix AttemptToRemoveSelectedItem (Line 1754-1775)

**Problem**: Calls broken `UpdateSelectedItemsContainer` which no longer updates selection

**Replace entire method**:
```csharp
private void AttemptToRemoveSelectedItem(object comboBoxItem)
{
    System.Diagnostics.Debug.WriteLine($"[WINUI] AttemptToRemoveSelectedItem called for: {comboBoxItem}");

    var listBoxItem = GetListViewItem(comboBoxItem);
    if (listBoxItem != null)
    {
        listBoxItem.IsChecked = false;
    }

    // Remove from selection
    if (SelectedItemsInternal.Contains(comboBoxItem))
    {
        SelectedItemsInternal.Remove(comboBoxItem);

        var selectedItems = SelectedItemsInternal.Where(a => a != null).ToList();
        UpdateSelectedItems(selectedItems);

        RaiseSelectedItemsChangedEvent(new List<object>(), new List<object> { comboBoxItem }, selectedItems);
        System.Diagnostics.Debug.WriteLine($"[WINUI] Removed item, SelectedItems.Count now: {SelectedItems.Count}");
    }
}
```

---

## Fix #3: Add WinUI Remove Button Handler (Line 2109-2117)

**Replace**:
```csharp
private void SelectedItemsControl_OnPointerPressed(object sender, PointerRoutedEventArgs e)
{
    if (IsEditMode)
    {
        IsDropDownOpen = !IsDropDownOpen;
    }

    AssignIsEditMode();
}
```

**With**:
```csharp
private void SelectedItemsControl_OnPointerPressed(object sender, PointerRoutedEventArgs e)
{
    if (IsEditMode)
    {
        // Check if user clicked the X button to remove an item
        if (IsEditable && IsRemoveItemButton(e))
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element?.DataContext is object item)
            {
                System.Diagnostics.Debug.WriteLine($"[WINUI] Remove button clicked for item: {item}");

                // Check for Ctrl+Click to delete (if enabled)
                var ctrlPressed = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

                if (ctrlPressed && EnableDeleteItemUI)
                {
                    RaiseItemDeleteRequestEvent(new Collection<object>() { item });
                    e.Handled = true;
                }
                else
                {
                    AttemptToRemoveSelectedItem(item);
                }
            }
        }
        else
        {
            IsDropDownOpen = !IsDropDownOpen;
        }
    }

    AssignIsEditMode();
}
```

---

## Additional Logging to Add

### Line 1911 - Add logging to WPF KeyUp:
```csharp
System.Diagnostics.Debug.WriteLine($"[WPF] KeyUp: {e.Key}, IsDropDownOpen: {IsDropDownOpen}");
```

---

## Testing Checklist

After fixes:

**WPF**:
- [ ] Arrow keys navigate dropdown when open
- [ ] Enter/Space select items
- [ ] X button removes items even when dropdown is open
- [ ] Mouse clicks select items

**WinUI**:
- [ ] Items can be selected by clicking
- [ ] Items can be selected with Enter/Space
- [ ] Selected items appear in top pane
- [ ] X button removes items from selection
- [ ] Can type multiple letters without losing focus
- [ ] Selected count displays correctly

---

## Notes

- The `UpdateSelectedItemsContainer(ItemsSource)` method was gutted in a previous patch and now only syncs visuals FROM selection - it doesn't update selection itself
- Any code calling `UpdateSelectedItemsContainer` expecting it to update selection must be changed to use `ToggleItemSelection` or directly manipulate `SelectedItemsInternal`
