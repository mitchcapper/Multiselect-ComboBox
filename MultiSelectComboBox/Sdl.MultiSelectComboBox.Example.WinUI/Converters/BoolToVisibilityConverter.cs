using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Sdl.MultiSelectComboBox.Example.WinUI.Converters
{
	public class BoolToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is bool b && b)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (value is Visibility visibility && visibility == Visibility.Visible)
			{
				return true;
			}
			return false;
		}
	}
}
