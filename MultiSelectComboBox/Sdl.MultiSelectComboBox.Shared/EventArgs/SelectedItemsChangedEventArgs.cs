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
	/// Raised when the selected items collection is modified (WinUI version)
	/// </summary>
	public class SelectedItemsChangedEventArgs : System.EventArgs
	{
		/// <summary>
		/// Items added to the collection
		/// </summary>
		public ICollection Added { get; }

		/// <summary>
		/// Items removed from the collection
		/// </summary>
		public ICollection Removed { get; }

		/// <summary>
		/// The selected items
		/// </summary>
		public ICollection Selected { get; }

		/// <summary>
		/// Creates a new instance of SelectedItemsChangedEventArgs
		/// </summary>
		public SelectedItemsChangedEventArgs(
			ICollection added,
			ICollection removed,
			ICollection selected)
		{
			Added = added;
			Removed = removed;
			Selected = selected;
		}
	}
#else
	/// <summary>
	/// Raised when the selected items collection is modified
	/// </summary>
	public class SelectedItemsChangedEventArgs : RoutedEventArgs
	{
		/// <summary>
		/// Items added to the collection
		/// </summary>
		public ICollection Added { get; }

		/// <summary>
		/// Items removed from the collection
		/// </summary>
		public ICollection Removed { get; }

		/// <summary>
		/// The selected items
		/// </summary>
		public ICollection Selected { get; }

		internal SelectedItemsChangedEventArgs(RoutedEvent routedEvent,
			ICollection added,
			ICollection removed,
			ICollection selected) : base(routedEvent)
		{
			Added = added;
			Removed = removed;
			Selected = selected;
		}
	}
#endif
}
