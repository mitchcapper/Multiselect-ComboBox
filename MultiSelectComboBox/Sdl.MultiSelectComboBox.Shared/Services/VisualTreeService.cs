#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif
using System.Collections.Generic;

namespace Sdl.MultiSelectComboBox.Services
{
	internal class VisualTreeService
	{
		public static T FindVisualChild<T>(DependencyObject parent, string name)
			where T : DependencyObject
		{
			if (parent == null)
			{
				return default;
			}

			T foundFrameworkElement = default;

			var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
			for (var i = 0; i < childrenCount; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);

				if (child is T childType)
				{
					if (string.IsNullOrEmpty(name) || (child is FrameworkElement frameworkElement && frameworkElement.Name == name))
					{
						foundFrameworkElement = childType;
						break;
					}
				}

				foundFrameworkElement = FindVisualChild<T>(child, name);

				if (!EqualityComparer<T>.Default.Equals(foundFrameworkElement, default))
				{
					break;
				}
			}

			return foundFrameworkElement;
		}

		public static T FindVisualTemplatedParent<T>(DependencyObject dependencyObject, string name) where T : DependencyObject
		{
			var source = dependencyObject as FrameworkElement;
			var parent = source?.Parent;
#if WINUI
			// WinUI doesn't have TemplatedParent - we walk up the visual tree instead
			var templatedParent = VisualTreeHelper.GetParent(source);
#else
			var templatedParent = source?.TemplatedParent;
#endif

			if (source == null)
			{
				return default;
			}

			T foundFrameworkElement;

			var templatedParentElement = templatedParent as FrameworkElement;
			var templatedParentParentElement = templatedParentElement?.Parent as FrameworkElement;

			if (!(parent is T) && !(templatedParent is T) && !(templatedParentElement?.Parent is T))
			{
				foundFrameworkElement = FindVisualTemplatedParent<T>(templatedParent, name);
			}
			else if (!string.IsNullOrEmpty(name))
			{
				if (parent is FrameworkElement parentElement && parentElement.Name == name)
				{
					foundFrameworkElement = (T)parent;
				}
				else if (templatedParentElement != null && templatedParentElement.Name == name)
				{
					foundFrameworkElement = (T)templatedParent;
				}
				else if (templatedParentParentElement != null && templatedParentParentElement.Name == name)
				{
					foundFrameworkElement = (T)templatedParentElement.Parent;
				}
				else
				{
					foundFrameworkElement = FindVisualTemplatedParent<T>(templatedParent, name);
				}
			}
			else
			{
				foundFrameworkElement = parent is T
					? (T)parent
					: ((T)templatedParent is T
						? (T)templatedParent
						: (T)templatedParentElement?.Parent);
			}

			return foundFrameworkElement;
		}
	}
}
