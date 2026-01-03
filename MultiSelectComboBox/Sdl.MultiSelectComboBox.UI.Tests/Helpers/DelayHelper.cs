using System.Diagnostics;
using System.Threading;

namespace Sdl.MultiSelectComboBox.UI.Tests.Helpers;

public static class DelayHelper {
	public static void SleepShort() => Thread.Sleep(20);
	public static void SleepLong() => Thread.Sleep(120);
	public static void SleepDbg() => DbgSleepSecs(10);

    private static void DbgSleepSecs(int secs) {
        Debug.WriteLine($"Debug Sleeping for {secs} seconds");
        Thread.Sleep(1000 * secs);
    }

    public static void SleepDbgLong()=> DbgSleepSecs(600);
}
