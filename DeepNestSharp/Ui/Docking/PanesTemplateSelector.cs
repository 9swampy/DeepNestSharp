namespace DeepNestSharp.Ui.Docking
{
  using System.Windows;
  using System.Windows.Controls;
  using AvalonDock.Layout;
  using DeepNestSharp.Ui.ViewModels;

  internal class PanesTemplateSelector : DataTemplateSelector
  {
    public PanesTemplateSelector()
    {
    }

    public DataTemplate SheetPlacementEditorTemplate
    {
      get;
      set;
    }

    public DataTemplate SheetPlacementPreviewEditorTemplate
    {
      get;
      set;
    }

    public DataTemplate PreviewTemplate
    {
      get;
      set;
    }

    public DataTemplate NestProjectEditorTemplate
    {
      get;
      set;
    }

    public DataTemplate SettingsEditorTemplate
    {
      get;
      set;
    }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      var itemAsLayoutContent = item as LayoutContent;

      if (item is MainViewModel)
      {
        return SheetPlacementPreviewEditorTemplate;
      }
      else if (item is NestProjectViewModel)
      {
        return NestProjectEditorTemplate;
      }
      else if (item is SheetPlacementViewModel)
      {
        return SheetPlacementEditorTemplate;
      }
      else if (item is PreviewViewModel)
      {
        return PreviewTemplate;
      }
      else if (item is SvgNestConfigViewModel)
      {
        return SettingsEditorTemplate;
      }

      return base.SelectTemplate(item, container);
    }
  }
}
