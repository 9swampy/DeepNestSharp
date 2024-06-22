namespace DeepNestSharp.Ui.UserControls
{
  using System;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using DeepNestSharp.Domain.ViewModels;
  using Xceed.Wpf.Toolkit.PropertyGrid;

  /// <summary>
  /// Interaction logic for SvgNestConfigEditor.xaml.
  /// </summary>
  public partial class SvgNestConfigEditor : UserControl
  {
    public SvgNestConfigEditor()
    {
      this.InitializeComponent();
      this.DataContextChanged += this.SvgNestConfigEditor_DataContextChanged;
    }

    private void SvgNestConfigEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (this.DataContext is SvgNestConfigViewModel svgNestConfigViewModel)
      {
        svgNestConfigViewModel.NotifyUpdatePropertyGrid += this.SvgNestConfigViewModel_NotifyUpdatePropertyGrid;
      }
    }

    private void SvgNestConfigViewModel_NotifyUpdatePropertyGrid(object? sender, EventArgs e)
    {
      this.propertyGrid.Update();
    }

    private void PropertyGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      PropertyGrid? propertyGrid = sender as PropertyGrid;
      if (propertyGrid is not null)
      {
        ScrollViewer? scrollViewer = propertyGrid.Parent as ScrollViewer;
        if (scrollViewer is not null)
        {
          var divider = Constants.HorizontalScrollDivider;
          var offset = scrollViewer.VerticalOffset;
          var delta = e.Delta;
          offset -= delta / divider;
          scrollViewer.ScrollToVerticalOffset(offset);
          e.Handled = true;
        }
      }
    }
  }
}
