using System.Collections;
using Sdl.MultiSelectComboBox.API;
#if WINUI
using Microsoft.UI.Xaml.Data;
#else
using System.Windows.Data;
#endif

namespace Sdl.MultiSelectComboBox.Services
{
	public class GroupComparerService : IComparer
	{
		public int Compare(object x, object y)
		{
#if WINUI
			// WinUI uses ICollectionViewGroup instead of CollectionViewGroup
			if (x is ICollectionViewGroup viewGroup1 && y is ICollectionViewGroup viewGroup2)
			{
				if (viewGroup1.Group is IItemGroup itemGroup1 && viewGroup2.Group is IItemGroup itemGroup2)
				{
					return itemGroup1.Order.CompareTo(itemGroup2.Order);
				}
			}
#else
			if (x is CollectionViewGroup viewGroup1 && y is CollectionViewGroup viewGroup2)
			{
				if (viewGroup1.Name is IItemGroup itemGroup1 && viewGroup2.Name is IItemGroup itemGroup2)
				{
					return itemGroup1.Order.CompareTo(itemGroup2.Order);
				}
			}
#endif

			return 0;
		}
	}
}
