using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sdl.MultiSelectComboBox.EventArgs {

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
	

		///// <summary>
		///// The base event arguments
		///// </summary>
		//private readonly FilterTextChangedEventArgsBase _baseArgs;

		///// <summary>
		///// The filter critera applied on the collection of items
		///// </summary>
		//public string Text => _baseArgs.Text;

		///// <summary>
		///// The filtered list of items
		///// </summary>
		////public ICollection Items => _baseArgs.Items;

		//internal SelectedItemsChangedEventArgs(RoutedEvent routedEvent, string text, ICollection items) : base(routedEvent)
		//{
		//	_baseArgs = new SelectedItemsChangedEventArgsBase(text, items);
		//}
	}
}
