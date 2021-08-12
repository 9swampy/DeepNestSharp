namespace DeepNestSharp.Ui.UserControls
{
  using System;
  using System.Windows;
  using System.Windows.Controls;
  using DeepNestSharp.Ui.ViewModels;

  /// <summary>
  /// Interaction logic for SvgNestConfigEditor.xaml.
  /// </summary>
  public partial class SvgNestConfigEditor : UserControl
  {
    public SvgNestConfigEditor()
    {
      InitializeComponent();
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
  }
}
