using System;
using System.Threading.Tasks;
using System.Windows;

namespace Sdl.MultiSelectComboBox.Example
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			for (int i = 0; i < e.Args.Length; i++)
			{
				if (e.Args[i].Equals("--AutoExit", StringComparison.OrdinalIgnoreCase) && i + 1 < e.Args.Length)
				{
					if (int.TryParse(e.Args[i + 1], out int seconds))
					{
						Task.Delay(TimeSpan.FromSeconds(seconds)).ContinueWith(_ =>
						{
							Dispatcher.Invoke(Shutdown);
						});
					}
				}
			}
		}
	}
}
