#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif

namespace Sdl.MultiSelectComboBox.Controls
{
	/// <summary>
	/// Extended ListBox that uses ExtendedListBoxItem as the item container
	/// </summary>
	public class ExtendedListBox : ListBox
	{
		protected override DependencyObject GetContainerForItemOverride()
		{
			return new ExtendedListBoxItem();
		}

		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			return item is ExtendedListBoxItem;
		}
	}
}
