namespace DeepNestSharp.Ui.Docking
{
  using System.Windows;
  using System.Windows.Controls;

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
