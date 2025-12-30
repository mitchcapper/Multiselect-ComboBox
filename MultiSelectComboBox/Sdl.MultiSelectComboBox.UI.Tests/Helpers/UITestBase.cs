using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
public class UITestBase : IDisposable {
	protected Application? App { get; private set; }
	protected UIA3Automation? Automation { get; private set; }
	protected Window? MainWindow { get; private set; }
	private bool _disposed;

	// Runs once before the entire test session starts
	//[Before(TestSession)]
	//[BeforeEvery(HookType.TestSession)]
	//public static async Task LogCommitHash() {
	//	//// Get the commit hash
	//	//var assembly = System.Reflection.Assembly.GetExecutingAssembly();
	//	//var infoVersion = assembly.GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()?.InformationalVersion;

	//	//// Split to get just the hash if necessary (format is often 1.0.0+hash)
	//	//var commitHash = infoVersion?.Split('+').LastOrDefault() ?? "Unknown";
	//	//// Write it to the standard output
	//	//TestSessionContext.Current.OutputWriter.WriteLine($"---------- BUILD INFO ----------");
	//	//TestSessionContext.Current.OutputWriter.WriteLine($"Commit Hash: {commitHash}");
	//	//TestSessionContext.Current.OutputWriter.WriteLine($"--------------------------------");
	//	var process = new Process {
	//		StartInfo = new ProcessStartInfo("git", "rev-parse HEAD") {
	//			RedirectStandardOutput = true,
	//			UseShellExecute = false
	//		}
	//	};
	//	process.Start();
	//	var commitId = process.StandardOutput.ReadToEnd().Trim();
	//	Console.WriteLine($"Commit: {commitId}");
	//}

	public Task ShortDelay() => Delay(20);
	public Task LongShortDelay() => Delay(200);
	public async Task Delay(int milliseconds) {
		await Task.Delay(milliseconds);
	}
	static UITestBase() {
		// Be very careful adding new test cases they must return the same data for .net 8 and netframework as we test on both
		AddKnownSearch(KnownSearch.CANA, "English (Canada)", "French (Canada)", "Inuktitut (Latin, Canada)", "Mohawk (Canada)");
		AddKnownSearch(KnownSearch.No,  "Albanian (North Macedonia)", "Arabic (Lebanon)", "English (Norfolk Island)", "English (Northern Mariana Islands)", "Filipino (Philippines)", "Italian (San Marino)", "Macedonian (North Macedonia)", "North Ndebele (Zimbabwe)", "Norwegian Bokmål (Norway)", "Norwegian Bokmål (Svalbard & Jan Mayen)", "Norwegian Nynorsk (Norway)", "Sami", "Lule (Norway)", "Sami", "Northern (Finland)", "Sami", "Northern (Norway)", "Sami", "Northern (Sweden)", "Sami", "Southern (Norway)");
		AddKnownSearch(KnownSearch.dut, "Dutch (Aruba)", "Dutch (Belgium)", "Dutch (Bonaire, Sint Eustatius and Saba)", "Dutch (Curaçao)", "Dutch (Netherlands)", "Dutch (Sint Maarten)", "Dutch (Suriname)");
	}
	[Before(HookType.Test)]
	public virtual void Setup() {
		// Initialize automation
		Automation = new UIA3Automation();


		// Launch application
		var exePath = GetApplicationPath();
		App = Application.Launch(exePath, $"--AutoExit {(System.Diagnostics.Debugger.IsAttached ? 60 * 10 : 20)}");

		// Get main window with timeout
		MainWindow = App.GetMainWindow(Automation, TimeSpan.FromSeconds(15));

		if (MainWindow == null) {
			throw new InvalidOperationException("Could not find main window after 15 seconds");
		}

		// Wait for window to be fully loaded
		Thread.Sleep(500);
	}

	[After(HookType.Test)]
	public virtual void TearDown() {
		try {
			var wind = new PageObjects.MainWindowPage(MainWindow, Automation);
			var txt = wind.EventLogText;
			TestContext.Current.Output.StandardOutput.WriteLine($"Event Log Text:\n{txt}");
			//testContext.Output.AttachArtifact(new Artifact
			//{
			//    File = new FileInfo(videoPath),
			//    DisplayName = "Test Recording",
			//    Description = "Video recording of the failed test"
			//});


		} catch {

		}
		// Dispose resources after test completes
		Dispose();
	}
	/// <summary>
	/// Contains info about known searches that can be used in tests, ExpectedCount should be right (but is not visible count) but the ItemToPosition mapping is not necessarily complete
	/// </summary>
	/// <param name="Term"></param>
	/// <param name="ExpectedCount"></param>
	/// <param name="VisibleItemToPosition"></param>
	public record KnownSearchInfo(String Term, int ExpectedCount, params KeyValuePair<string, int>[] VisibleItemToPosition) {
	}
	public enum KnownSearch{
		CANA,
		No,
		dut
	}
	protected static KnownSearchInfo GetKnownSearch(KnownSearch search) => KnownSearches[search];
	private static KnownSearchInfo AddKnownSearch(KnownSearch search, params string[] VisibleItems) => AddKnownSearch(search, search.ToString(), VisibleItems.Length, VisibleItems);
	private static KnownSearchInfo AddKnownSearch(KnownSearch search, int ExpectedCount, params string[] VisibleItems) => AddKnownSearch(search, search.ToString(), ExpectedCount, VisibleItems);
	private static KnownSearchInfo AddKnownSearch(KnownSearch search, String Term, int ExpectedCount, params string[] VisibleItems) {
		var add = new KnownSearchInfo(Term, ExpectedCount, VisibleItems.Select((item, index) => new KeyValuePair<string, int>(item, index)).ToArray());
		KnownSearches[search] = add;
		return add;
	}
	public static ConcurrentDictionary<KnownSearch,KnownSearchInfo> KnownSearches = new();



	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing) {
		if (_disposed) {
			return;
		}

		if (disposing) {
			try {
				App?.Close();
			} catch {
				// Ignore errors during cleanup
			}

			try {
				App?.Dispose();
			} catch {
				// Ignore errors during cleanup
			}

			Automation?.Dispose();
		}

		_disposed = true;
	}

	protected virtual string GetApplicationPath() {
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

		if (!File.Exists(exePath)) {
			throw new FileNotFoundException(
				$"Application executable not found at: {exePath}. " +
				"Please build the Sdl.MultiSelectComboBox.Example project first.",
				exePath
			);
		}

		return exePath;
	}

	protected void CaptureScreenshot(string testName) {
		try {
			var screenshot = Capture.Element(MainWindow);
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
		} catch (Exception ex) {
			Console.WriteLine($"Failed to capture screenshot: {ex.Message}");
		}
	}
}
