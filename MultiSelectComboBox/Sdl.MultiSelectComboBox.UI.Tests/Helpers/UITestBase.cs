using System.IO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Capturing;
using FlaUI.UIA3;
using TUnit.Core;

[assembly: NotInParallel]

namespace Sdl.MultiSelectComboBox.UI.Tests.Helpers;


/// <summary>
/// Base class for UI tests providing application lifecycle management
/// </summary>
public class UITestBase : IDisposable
{
    protected Application? App { get; private set; }
    protected UIA3Automation? Automation { get; private set; }
    protected Window? MainWindow { get; private set; }
    private bool _disposed;

    [Before(HookType.Test)]
    public virtual void Setup()
    {
        // Initialize automation
        Automation = new UIA3Automation();

        // Launch application
        var exePath = GetApplicationPath();
        App = Application.Launch(exePath);

        // Get main window with timeout
        MainWindow = App.GetMainWindow(Automation, TimeSpan.FromSeconds(60));

        if (MainWindow == null)
        {
            throw new InvalidOperationException("Could not find main window after 15 seconds");
        }

        // Wait for window to be fully loaded
        Thread.Sleep(500);
    }

    [After(HookType.Test)]
    public virtual void TearDown()
    {
        // Dispose resources after test completes
        Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            try
            {
                App?.Close();
            }
            catch
            {
                // Ignore errors during cleanup
            }

            try
            {
                App?.Dispose();
            }
            catch
            {
                // Ignore errors during cleanup
            }

            Automation?.Dispose();
        }

        _disposed = true;
    }

    protected virtual string GetApplicationPath()
    {
        // Get the base directory where the test assembly runs
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;

        // Navigate from bin/Debug/net8.0-windows/ to solution root
        var solutionDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));

        // Build path to the example application
        #if DEBUG
        const string config = "Debug";
        #else
        const string config = "Release";
        #endif

        var exePath = Path.Combine(
            solutionDir,
            "MultiSelectComboBox.Example",
            "bin",
            config,
            "net8.0-windows",
            "Sdl.MultiSelectComboBox.Example.exe"
        );

        if (!File.Exists(exePath))
        {
            throw new FileNotFoundException(
                $"Application executable not found at: {exePath}. " +
                "Please build the Sdl.MultiSelectComboBox.Example project first.",
                exePath
            );
        }

        return exePath;
    }

    protected void CaptureScreenshot(string testName)
    {
        try
        {
            var screenshot = Capture.Screen();
            var screenshotDir = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Screenshots"
            );

            Directory.CreateDirectory(screenshotDir);

            var safeTestName = string.Join("_", testName.Split(Path.GetInvalidFileNameChars()));
            var filename = $"{safeTestName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var filepath = Path.Combine(screenshotDir, filename);

            screenshot.ToFile(filepath);
            Console.WriteLine($"Screenshot saved to: {filepath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to capture screenshot: {ex.Message}");
        }
    }
}
