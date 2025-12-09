using System.Collections;

namespace Sdl.MultiSelectComboBox.EventArgs
{
	/// <summary>
	/// Raised when the filter criteria has changed
	/// </summary>
	public class FilterTextChangedEventArgsBase : System.EventArgs
	{
		/// <summary>
		/// The filter critera applied on the collection of items
		/// </summary>
		public string Text { get; }

		/// <summary>
		/// The filtered list of items
		/// </summary>
		public ICollection Items { get; }

		/// <summary>
		/// Creates a new instance of FilterTextChangedEventArgsBase
		/// </summary>
		public FilterTextChangedEventArgsBase(string text, ICollection items)
		{
			Text = text;
			Items = items;
		}
	}
}
