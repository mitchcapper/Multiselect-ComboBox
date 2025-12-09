#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif

namespace Sdl.MultiSelectComboBox.Controls
{
#if WINUI
	public class ExtendedListBoxItem : ListViewItem
#else
	public class ExtendedListBoxItem : ListBoxItem
#endif
	{
		public static readonly DependencyProperty IsCheckedProperty =
			DependencyProperty.Register("IsChecked", typeof(bool), typeof(ExtendedListBoxItem),
#if WINUI
				new PropertyMetadata(false));
#else
				new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
#endif

		public bool IsChecked
		{
			get => (bool)GetValue(IsCheckedProperty);
			set => SetValue(IsCheckedProperty, value);
		}
	}
}
