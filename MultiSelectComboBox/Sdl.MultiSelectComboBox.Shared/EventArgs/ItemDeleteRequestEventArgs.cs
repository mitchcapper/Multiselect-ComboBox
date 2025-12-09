using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sdl.MultiSelectComboBox.EventArgs {
	public class ItemDeleteRequestEventArgs :
#if WINUI
		System.EventArgs
#else
		RoutedEventArgs
#endif
		{
		public ICollection ItemsRemoveRequestedOn { get; }
		public ItemDeleteRequestEventArgs(
#if WINUI
		System.EventArgs
#else
		RoutedEvent
#endif
			routedEvent,
			ICollection items
			)  {
			ItemsRemoveRequestedOn = items;
		}
	}
}
