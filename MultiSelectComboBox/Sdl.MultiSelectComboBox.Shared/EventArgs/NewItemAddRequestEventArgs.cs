using System;
#if WINUI
using Microsoft.UI.Xaml;
#else
using System.Windows;
#endif

namespace Sdl.MultiSelectComboBox.EventArgs
{
#if WINUI
	/// <summary>
	/// Raised when user requests to add a new item (WinUI version)
	/// </summary>
	public class NewItemAddRequestEventArgs : System.EventArgs
	{
		/// <summary>
		/// The text typed by the user
		/// </summary>
		public string TypedText { get; }

		/// <summary>
		/// Creates a new instance of NewItemAddRequestEventArgs
		/// </summary>
		public NewItemAddRequestEventArgs(string typedText)
		{
			TypedText = typedText;
		}
	}
#else
	/// <summary>
	/// Raised when user requests to add a new item
	/// </summary>
	public class NewItemAddRequestEventArgs : RoutedEventArgs
	{
		/// <summary>
		/// The text typed by the user
		/// </summary>
		public string TypedText { get; }

		internal NewItemAddRequestEventArgs(RoutedEvent routedEvent, string typedText) : base(routedEvent)
		{
			TypedText = typedText;
		}
	}
#endif
}
