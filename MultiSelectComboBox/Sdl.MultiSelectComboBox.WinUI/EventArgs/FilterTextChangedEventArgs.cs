using Microsoft.UI.Xaml;
using System.Collections;

namespace Sdl.MultiSelectComboBox.WinUI.EventArgs
{
	/// <summary>
	/// Raised when the filter criteria has changed (WinUI version)
	/// </summary>
	public class FilterTextChangedEventArgs : RoutedEventArgs
	{
		/// <summary>
		/// The base event arguments
		/// </summary>
		private readonly Sdl.MultiSelectComboBox.EventArgs.FilterTextChangedEventArgsBase _baseArgs;

		/// <summary>
		/// The filter critera applied on the collection of items
		/// </summary>
		public string Text => _baseArgs.Text;

		/// <summary>
		/// The filtered list of items
		/// </summary>
		public ICollection Items => _baseArgs.Items;

		internal FilterTextChangedEventArgs(RoutedEvent routedEvent, string text, ICollection items)
		{
			_baseArgs = new Sdl.MultiSelectComboBox.EventArgs.FilterTextChangedEventArgsBase(text, items);
		}
	}
}
