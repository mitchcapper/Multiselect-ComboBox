using System.Collections;

namespace Sdl.MultiSelectComboBox.EventArgs
{
	/// <summary>
	/// Raised when the selected items collection is modified
	/// </summary>
	public class SelectedItemsChangedEventArgsBase : System.EventArgs
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
		/// Creates a new instance of SelectedItemsChangedEventArgsBase
		/// </summary>
		public SelectedItemsChangedEventArgsBase(
			ICollection added,
			ICollection removed,
			ICollection selected)
		{
			Added = added;
			Removed = removed;
			Selected = selected;
		}
	}
}
