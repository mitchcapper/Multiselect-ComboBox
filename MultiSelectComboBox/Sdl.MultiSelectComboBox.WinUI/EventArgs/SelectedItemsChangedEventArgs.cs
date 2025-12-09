using Microsoft.UI.Xaml;
using System.Collections;

namespace Sdl.MultiSelectComboBox.WinUI.EventArgs
{
	/// <summary>
	/// Raised when the selected items collection is modified (WinUI version)
	/// </summary>
	public class SelectedItemsChangedEventArgs : RoutedEventArgs
	{
		/// <summary>
		/// The base event arguments
		/// </summary>
		private readonly Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgsBase _baseArgs;

		/// <summary>
		/// Items added to the collection
		/// </summary>
		public ICollection Added => _baseArgs.Added;

		/// <summary>
		/// Items removed from the collection
		/// </summary>
		public ICollection Removed => _baseArgs.Removed;

		/// <summary>
		/// The selected items
		/// </summary>
		public ICollection Selected => _baseArgs.Selected;

		internal SelectedItemsChangedEventArgs(RoutedEvent routedEvent,
			ICollection added,
			ICollection removed,
			ICollection selected) 
		{
			_baseArgs = new Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgsBase(added, removed, selected);
		}
	}
}
