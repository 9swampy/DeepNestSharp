namespace DeepNestSharp.Ui.UserControls
{
  using System;
  using System.ComponentModel;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Data;
  using System.Windows.Input;
  using System.Windows.Media;

  /// <summary>
  /// Interaction logic for NestMonitor.xaml
  /// </summary>
  public partial class NestMonitor : UserControl
  {
    private GridViewColumnHeader lastHeaderClicked;
    private ListSortDirection lastDirection = ListSortDirection.Ascending;
    private HorizontalScrollHandler horizontalScrollHandler;

    public NestMonitor()
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

        if (headerClicked is not null)
        {
          GridViewHeaderRowPresenter? presenter = headerClicked.Parent as GridViewHeaderRowPresenter;
          if (presenter is not null)
          {
            int zeroBasedDisplayIndex = presenter.Columns.IndexOf(headerClicked.Column);
            if (zeroBasedDisplayIndex == 5) // This is pretty hacky, but it works for now
            {
              return;
            }
          }

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

    private void HandleHorizontalScroll(object sender, MouseWheelEventArgs e)
    {
      horizontalScrollHandler.ListView_PreviewMouseWheel(sender, e);
    }
  }
}
