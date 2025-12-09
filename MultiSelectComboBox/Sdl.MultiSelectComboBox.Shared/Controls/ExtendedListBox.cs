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
	/// Extended ListBox/ListView that uses ExtendedListBoxItem as the item container
	/// </summary>
#if WINUI
	public class ExtendedListBox : ListView
#else
	public class ExtendedListBox : ListBox
#endif
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
