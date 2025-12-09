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
	/// Raised when the filter criteria has changed (WinUI version)
	/// </summary>
	public class FilterTextChangedEventArgs : System.EventArgs
	{
		/// <summary>
		/// The filter criteria applied on the collection of items
		/// </summary>
		public string Text { get; }

		/// <summary>
		/// The filtered list of items
		/// </summary>
		public ICollection Items { get; }

		/// <summary>
		/// Creates a new instance of FilterTextChangedEventArgs
		/// </summary>
		public FilterTextChangedEventArgs(string text, ICollection items)
		{
			Text = text;
			Items = items;
		}
	}
#else
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
#endif
}
