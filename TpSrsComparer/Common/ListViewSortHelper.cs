using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
namespace TpSrsComparer.Common
{
	public class ListViewSortHelper
	{
		public static readonly DependencyProperty SortEnabledProperty = DependencyProperty.RegisterAttached("SortEnabled", typeof(bool), typeof(ListViewSortHelper), new PropertyMetadata(OnSortEnabledChanged));

		public static readonly DependencyProperty SortPropertyProperty = DependencyProperty.RegisterAttached("SortProperty", typeof(string), typeof(ListViewSortHelper), new PropertyMetadata(OnSortPropertyChanged));

		public static bool GetSortEnabled(ListView lv)
		{
			return (bool)lv.GetValue(SortEnabledProperty);
		}

		public static void SetSortEnabled(ListView lv, bool value)
		{
			lv.SetValue(SortEnabledProperty, value);
		}

		public static string GetSortProperty(GridViewColumn column)
		{
			return (string)column.GetValue(SortPropertyProperty);
		}

		public static void SetSortProperty(GridViewColumn column, string propName)
		{
			column.SetValue(SortPropertyProperty, propName);
		}

		private static void OnSortEnabledChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
		{
			ListView listView = dobj as ListView;
			if (listView != null)
			{
				if ((bool)e.NewValue)
				{
					listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnGridViewColumnHeaderClicked));
				}
				else
				{
					listView.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnGridViewColumnHeaderClicked));
				}
			}
		}

		private static void OnSortPropertyChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
		{
			GridViewColumn gridViewColumn = dobj as GridViewColumn;
			string text = (string)e.NewValue;
			if (gridViewColumn.DisplayMemberBinding == null && gridViewColumn.CellTemplate == null && text != null)
			{
				new Binding(text);
			}
		}

		private static void OnGridViewColumnHeaderClicked(object sender, RoutedEventArgs e)
		{
			GridViewColumnHeader gridViewColumnHeader = e.OriginalSource as GridViewColumnHeader;
			ListView listView = sender as ListView;
			string sortProperty;
			if (listView != null && gridViewColumnHeader != null && (sortProperty = GetSortProperty(gridViewColumnHeader.Column)) != null)
			{
				UpdateSortDescription(CollectionViewSource.GetDefaultView(listView.ItemsSource), sortProperty);
			}
		}

		private static void UpdateSortDescription(ICollectionView view, string propName)
		{
			ListSortDirection direction = ListSortDirection.Ascending;
			if (view.SortDescriptions.Count > 0 && view.SortDescriptions[0].PropertyName == propName && view.SortDescriptions[0].Direction == ListSortDirection.Ascending)
			{
				direction = ListSortDirection.Descending;
			}
			view.SortDescriptions.Clear();
			view.SortDescriptions.Add(new SortDescription(propName, direction));
		}
	}
}
