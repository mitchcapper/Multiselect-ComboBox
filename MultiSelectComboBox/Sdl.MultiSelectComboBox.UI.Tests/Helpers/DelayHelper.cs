using System.Threading;

namespace Sdl.MultiSelectComboBox.UI.Tests.Helpers;

public static class DelayHelper {
	public static void SleepShort() => Thread.Sleep(20);
	public static void SleepLong() => Thread.Sleep(120);
	public static void SleepDbg() => Thread.Sleep(1000 * 10);
	public static void SleepDbgLong() => Thread.Sleep(1000 * 600);
}
