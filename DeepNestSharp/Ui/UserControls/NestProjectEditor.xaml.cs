namespace DeepNestSharp.Ui.UserControls
{
  using System.ComponentModel;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Data;

  public partial class NestProjectEditor : UserControl
  {
    private GridViewColumnHeader lastHeaderClicked;
    private ListSortDirection lastDirection = ListSortDirection.Ascending;

    public NestProjectEditor() => this.InitializeComponent();

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
        var headerClicked = e.OriginalSource as GridViewColumnHeader;
        ListSortDirection direction;

        if (headerClicked != null)
        {
          if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
          {
            direction = headerClicked != this.lastHeaderClicked
                ? ListSortDirection.Ascending
                : this.lastDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

            var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
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
  }
}
