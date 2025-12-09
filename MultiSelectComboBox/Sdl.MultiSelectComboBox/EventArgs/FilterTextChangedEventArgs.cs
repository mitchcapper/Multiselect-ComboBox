using System.Collections;
using System.Windows;

namespace Sdl.MultiSelectComboBox.EventArgs
{
	/// <summary>
	/// Raised when the filter criteria has changed
	/// </summary>
	public class FilterTextChangedEventArgs : RoutedEventArgs
	{
		/// <summary>
		/// The base event arguments
		/// </summary>
		private readonly FilterTextChangedEventArgsBase _baseArgs;

		/// <summary>
		/// The filter critera applied on the collection of items
		/// </summary>
		public string Text => _baseArgs.Text;

		/// <summary>
		/// The filtered list of items
		/// </summary>
		public ICollection Items => _baseArgs.Items;

		internal FilterTextChangedEventArgs(RoutedEvent routedEvent, string text, ICollection items) : base(routedEvent)
		{
			_baseArgs = new FilterTextChangedEventArgsBase(text, items);
		}
	}
}
