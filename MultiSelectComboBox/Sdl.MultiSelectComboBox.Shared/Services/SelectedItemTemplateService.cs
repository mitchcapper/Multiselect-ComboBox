#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif

namespace Sdl.MultiSelectComboBox.Services
{
	public class SelectedItemTemplateService : DataTemplateSelector
	{
		private readonly DataTemplate _selectedItemsItemTemplate;
		private readonly DataTemplate _selectedItemsSearchableItemTemplate;

		public SelectedItemTemplateService(DataTemplate selectedItemsItemTemplate, DataTemplate selectedItemsSearchableItemTemplate)
		{
			_selectedItemsItemTemplate = selectedItemsItemTemplate;
			_selectedItemsSearchableItemTemplate = selectedItemsSearchableItemTemplate;
		}

#if WINUI
		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			if (!(container is FrameworkElement))
			{
				return base.SelectTemplateCore(item, container);
			}

			if (item != null)
			{
				return _selectedItemsItemTemplate;
			}

			return _selectedItemsSearchableItemTemplate;
		}
#else
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (!(container is FrameworkElement))
			{
				return base.SelectTemplate(item, container);
			}

			if (item != null)
			{
				return _selectedItemsItemTemplate;
			}

			return _selectedItemsSearchableItemTemplate;
		}
#endif
	}
}
