using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;

namespace Sdl.MultiSelectComboBox.UI.Tests.PageObjects;

/// <summary>
/// Page Object for the main example window
/// </summary>
public class MainWindowPage : IDisposable
{
    private readonly Window _window;
    private readonly AutomationBase _automation;
    private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(5);
    private bool _disposed;

    public MainWindowPage(Window window, AutomationBase automation)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _automation = automation ?? throw new ArgumentNullException(nameof(automation));
        MultiSelectComboBox = new MultiSelectComboBoxPage(window, automation);
    }

    /// <summary>
    /// The MultiSelectComboBox control page object
    /// </summary>
    public MultiSelectComboBoxPage MultiSelectComboBox { get; }

    /// <summary>
    /// Gets the displayed selected items count from the header
    /// </summary>
    public int DisplayedSelectedCount
    {
        get
        {
            // Find the textbox that displays the count - it's near "Selected" text
            var textBoxes = _window.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit));
            foreach (var tb in textBoxes)
            {
                var text = tb.AsTextBox().Text;
                if (int.TryParse(text, out var count) && count >= 0)
                {
                    // Verify this is the count textbox by checking if "Selected" text is nearby
                    var parent = tb.Parent;
                    if (parent != null)
                    {
                        var siblings = parent.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit));
                        var hasSelectedText = siblings.Any(s => s.AsTextBox().Text?.Contains("Selected") == true);
                        if (hasSelectedText || count < 1000) // Reasonable count range
                        {
                            return count;
                        }
                    }
                }
            }
            return 0;
        }
    }

    /// <summary>
    /// Clicks the "Clear selected items" button
    /// </summary>
    public void ClearSelectedItems()
    {
        var clearButton = _window.FindFirstDescendant(cf => cf.ByName("Clear selected items"))?.AsButton();
        if (clearButton == null)
        {
            // Try finding by content
            var buttons = _window.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button));
            clearButton = buttons.FirstOrDefault(b =>
                (b.Name ?? string.Empty).Contains("Clear", StringComparison.OrdinalIgnoreCase))?.AsButton();
        }

        clearButton?.Click();
        Thread.Sleep(200);
    }

    /// <summary>
    /// Clicks the "Select 20 random items" button
    /// </summary>
    public void SelectRandomItems()
    {
        var button = _window.FindFirstDescendant(cf => cf.ByName("Select 20 random items"))?.AsButton();
        button?.Click();
        Thread.Sleep(500); // Wait for items to be selected
    }

    /// <summary>
    /// Sets a checkbox state by name
    /// </summary>
    public void SetCheckboxState(string checkboxName, bool isChecked)
    {
        var checkboxes = _window.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.CheckBox));
        var checkbox = checkboxes.FirstOrDefault(cb =>
            (cb.Name ?? string.Empty).Contains(checkboxName, StringComparison.OrdinalIgnoreCase))?.AsCheckBox();

        if (checkbox != null && checkbox.IsChecked != isChecked)
        {
            checkbox.Click();
            Thread.Sleep(100);
        }
    }

    /// <summary>
    /// Gets the event log text
    /// </summary>
    public string EventLogText
    {
        get
        {
            var textBoxes = _window.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit));
            // The event log is a large multi-line textbox
            var eventLog = textBoxes.FirstOrDefault(tb =>
            {
                var textBox = tb.AsTextBox();
                var text = textBox.Text ?? string.Empty;
                return text.Contains("=>") || text.Length > 100;
            });
            return eventLog?.AsTextBox().Text ?? string.Empty;
        }
    }

    /// <summary>
    /// Clears the event log
    /// </summary>
    public void ClearEventLog()
    {
        var clearLogButton = _window.FindFirstDescendant(cf => cf.ByName("Clear log"))?.AsButton();
        if (clearLogButton == null)
        {
            var buttons = _window.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button));
            clearLogButton = buttons.FirstOrDefault(b =>
                (b.Name ?? string.Empty).Contains("Clear log", StringComparison.OrdinalIgnoreCase))?.AsButton();
        }
        clearLogButton?.Click();
        Thread.Sleep(100);
    }

    /// <summary>
    /// Sets the selection mode (Multiple or Single)
    /// </summary>
    public void SetSelectionMode(string mode)
    {
        var comboBoxes = _window.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.ComboBox));
        var selectionModeCombo = comboBoxes.FirstOrDefault()?.AsComboBox();
        if (selectionModeCombo != null)
        {
            selectionModeCombo.Select(mode);
            Thread.Sleep(200);
        }
    }

    /// <summary>
    /// Waits for a condition to be true
    /// </summary>
    public void WaitFor(Func<bool> condition, TimeSpan? timeout = null, string? message = null)
    {
        timeout ??= _defaultTimeout;
        Retry.WhileTrue(
            () => !condition(),
            timeout: timeout.Value,
            throwOnTimeout: true,
            timeoutMessage: message ?? "Condition not met in time"
        );
    }

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            MultiSelectComboBox?.Dispose();
        }

        _disposed = true;
    }

    #endregion
}
