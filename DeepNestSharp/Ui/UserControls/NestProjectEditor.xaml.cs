namespace DeepNestSharp.Ui.UserControls
{
  using System;
  using System.ComponentModel;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Data;
  using System.Windows.Input;

  public partial class NestProjectEditor : UserControl
  {
    private GridViewColumnHeader lastHeaderClicked;
    private ListSortDirection lastDirection = ListSortDirection.Ascending;
    private HorizontalScrollHandler horizontalScrollHandler;

    public NestProjectEditor()
    {
      horizontalScrollHandler = new HorizontalScrollHandler();
      this.InitializeComponent();
    }

    private static void Sort(ListView sender, string sortBy, ListSortDirection direction)
    {
      ICollectionView dataView = CollectionViewSource.GetDefaultView(sender.ItemsSource);

      dataView.SortDescriptions.Clear();
      SortDescription sd = new SortDescription(sortBy, direction);
      dataView.SortDescriptions.Add(sd);
      dataView.Refresh();
    }

    private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
    {
      if (sender is ListView listView)
      {
        GridViewColumnHeader? headerClicked = e.OriginalSource as GridViewColumnHeader;
        ListSortDirection direction;

        if (headerClicked != null)
        {
          if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
          {
            direction = headerClicked != this.lastHeaderClicked
                ? ListSortDirection.Ascending
                : this.lastDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

            Binding? columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
            var sortBy = (string)(columnBinding?.Path.Path ?? headerClicked.Column.Header);

            Sort(listView, sortBy, direction);

            headerClicked.Column.HeaderTemplate = direction == ListSortDirection.Ascending
                ? this.Resources["HeaderTemplateArrowUp"] as DataTemplate
                : this.Resources["HeaderTemplateArrowDown"] as DataTemplate;

            this.RemovePreviousArrow(headerClicked);

            this.lastHeaderClicked = headerClicked;
            this.lastDirection = direction;
          }
        }
      }
    }

    private void RemovePreviousArrow(GridViewColumnHeader? headerClicked)
    {
      if (!this.LastHeaderSame(headerClicked))
      {
        this.lastHeaderClicked.Column.HeaderTemplate = null;
      }
    }

    private bool LastHeaderSame(GridViewColumnHeader? headerClicked)
    {
      return this.lastHeaderClicked == null || this.lastHeaderClicked == headerClicked;
    }

    private void PartEditor_GotFocus(object sender, RoutedEventArgs e)
    {
      ListView? listView = sender as ListView;
      if (listView is not null)
      {
        listView.SelectedIndex = -1;
      }
    }

    private void SheetEditor_GotFocus(object sender, RoutedEventArgs e)
    {
      ListView? listView = sender as ListView;
      if (listView is not null)
      {
        listView.SelectedIndex = -1;
      }
    }

    private void HandleHorizontalScroll(object sender, MouseWheelEventArgs e)
    {
      horizontalScrollHandler.ListView_PreviewMouseWheel(sender, e);
    }
  }
}
