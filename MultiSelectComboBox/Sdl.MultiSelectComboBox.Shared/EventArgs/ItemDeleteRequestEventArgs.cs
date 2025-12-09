using System;
using System.Collections;
#if WINUI
using Microsoft.UI.Xaml;
#else
using System.Windows;
#endif

namespace Sdl.MultiSelectComboBox.EventArgs
{
#if WINUI
	/// <summary>
	/// Raised when user requests to delete items (WinUI version)
	/// </summary>
	public class ItemDeleteRequestEventArgs : System.EventArgs
	{
		/// <summary>
		/// The items that were requested to be removed
		/// </summary>
		public ICollection ItemsRemoveRequestedOn { get; }

		/// <summary>
		/// Creates a new instance of ItemDeleteRequestEventArgs
		/// </summary>
		public ItemDeleteRequestEventArgs(ICollection items)
		{
			ItemsRemoveRequestedOn = items;
		}
	}
#else
	/// <summary>
	/// Raised when user requests to delete items
	/// </summary>
	public class ItemDeleteRequestEventArgs : RoutedEventArgs
	{
		/// <summary>
		/// The items that were requested to be removed
		/// </summary>
		public ICollection ItemsRemoveRequestedOn { get; }

		internal ItemDeleteRequestEventArgs(RoutedEvent routedEvent, ICollection items) : base(routedEvent)
		{
			ItemsRemoveRequestedOn = items;
		}
	}
#endif
}
