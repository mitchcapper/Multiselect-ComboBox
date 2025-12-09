#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif

namespace Sdl.MultiSelectComboBox.Services
{
	public class DropdownItemTemplateService : DataTemplateSelector
	{
		private readonly DataTemplate _defaultItemTemplate;

		public DropdownItemTemplateService(DataTemplate selectedItemsItemTemplate)
		{
			_defaultItemTemplate = selectedItemsItemTemplate;
		}

#if WINUI
		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			if (item != null)
			{
				return _defaultItemTemplate;
			}

			return base.SelectTemplateCore(item, container);
		}
#else
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item != null)
			{
				return _defaultItemTemplate;
			}

			return base.SelectTemplate(null, container);
		}
#endif
	}
}
