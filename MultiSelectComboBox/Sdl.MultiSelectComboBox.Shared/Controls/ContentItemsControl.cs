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
	}
}
