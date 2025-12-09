#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif

namespace Sdl.MultiSelectComboBox.Controls
{
	public sealed class ContentItemsControl : ItemsControl
	{
		protected override DependencyObject GetContainerForItemOverride()
		{
			return new ContentControl();
		}

#if WINUI
		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			base.PrepareContainerForItemOverride(element, item);
			
			// If the item is null (Filter Placeholder), clear the Style to avoid the "X" button and border
			// from the ItemContainerStyle applied by the ItemsControl.
			if (item == null && element is Control control)
			{
				control.Style = null; 
			}
		}
#endif
	}
}
