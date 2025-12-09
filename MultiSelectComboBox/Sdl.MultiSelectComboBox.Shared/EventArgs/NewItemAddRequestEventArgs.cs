using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sdl.MultiSelectComboBox.EventArgs {
	public class NewItemAddRequestEventArgs :
#if WINUI
		System.EventArgs
#else
		RoutedEventArgs
#endif
{

		public string TypedText { get; }

		public NewItemAddRequestEventArgs(
#if WINUI
		System.EventArgs
#else
		RoutedEvent
#endif
			routedEvent, string TypedText)  {
			this.TypedText = TypedText;
		}
	}
}
