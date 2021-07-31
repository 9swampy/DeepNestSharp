namespace DeepNestSharp.Ui.Docking
{
  using System.Windows;
  using System.Windows.Controls;
  using DeepNestSharp.Ui.ViewModels;

  internal class PanesStyleSelector : StyleSelector
  {
    public Style? ToolStyle
    {
      get;
      set;
    }

    public Style? FileStyle
    {
      get;
      set;
    }

    public override System.Windows.Style SelectStyle(object item, DependencyObject container)
    {
      if (item is MainViewModel)
      {
        return ToolStyle;
      }

      if (item is ToolViewModel)
      {
        return ToolStyle;
      }

      if (item is FileViewModel)
      {
        return FileStyle;
      }

      return base.SelectStyle(item, container);
    }
  }
}
