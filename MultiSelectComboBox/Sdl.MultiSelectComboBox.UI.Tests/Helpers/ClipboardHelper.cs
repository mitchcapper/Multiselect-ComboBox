using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sdl.MultiSelectComboBox.UI.Tests.Helpers;

internal static class ClipboardHelper
{
	public static void SetText(string text)
	{
		RunSta(() => {
			System.Windows.Clipboard.SetText(text ?? string.Empty);
			return 0;
		});
	}

	public static string GetText()
	{
		return RunSta(() => {
			return System.Windows.Clipboard.ContainsText()
				? System.Windows.Clipboard.GetText()
				: string.Empty;
		});
	}

	private static T RunSta<T>(Func<T> func)
	{
		if (func is null) throw new ArgumentNullException(nameof(func));

		var tcs = new TaskCompletionSource<T>();

		var thread = new Thread(() => {
			try {
				for (var attempt = 0; attempt < 5; attempt++) {
					try {
						tcs.SetResult(func());
						return;
					} catch {
						Thread.Sleep(50);
					}
				}

				tcs.SetResult(func());
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
		});

		thread.SetApartmentState(ApartmentState.STA);
		thread.IsBackground = true;
		thread.Start();

		return tcs.Task.GetAwaiter().GetResult();
	}
}
