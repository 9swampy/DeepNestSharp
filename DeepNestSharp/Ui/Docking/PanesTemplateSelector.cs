namespace DeepNestSharp.Ui.Docking
{
  using System.Windows;
  using System.Windows.Controls;
  using DeepNestSharp.Ui.ViewModels;

  internal class PanesTemplateSelector : DataTemplateSelector
  {
    public PanesTemplateSelector()
    {
    }

    public DataTemplate? SheetPlacementEditorTemplate
    {
      get;
      set;
    }

    public DataTemplate? SheetPlacementPreviewEditorTemplate
    {
      get;
      set;
    }

    public DataTemplate? PreviewTemplate
    {
      get;
      set;
    }

    public DataTemplate? NestMonitorTemplate
    {
      get;
      set;
    }

    public DataTemplate? NestProjectEditorTemplate
    {
      get;
      set;
    }

    public DataTemplate? SettingsEditorTemplate
    {
      get;
      set;
    }

    public DataTemplate? PropertiesEditorTemplate
    {
      get;
      set;
    }

    public DataTemplate? PartEditorTemplate
    {
      get;
      set;
    }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (item is NestMonitorViewModel)
      {
        if (NestMonitorTemplate == null)
        {
          throw new System.InvalidOperationException($"{nameof(NestMonitorTemplate)} not set.");
        }
        else
        {
          return NestMonitorTemplate;
        }
      }
      else if (item is NestProjectViewModel)
      {
        if (NestProjectEditorTemplate == null)
        {
          throw new System.InvalidOperationException($"{nameof(NestProjectEditorTemplate)} not set.");
        }
        else
        {
          return NestProjectEditorTemplate;
        }
      }
      else if (item is SheetPlacementViewModel)
      {
        if (SheetPlacementEditorTemplate == null)
        {
          throw new System.InvalidOperationException($"{nameof(SheetPlacementEditorTemplate)} not set.");
        }
        else
        {
          return SheetPlacementEditorTemplate;
        }
      }
      else if (item is PreviewViewModel)
      {
        if (PreviewTemplate == null)
        {
          throw new System.InvalidOperationException($"{nameof(PreviewTemplate)} not set.");
        }
        else
        {
          return PreviewTemplate;
        }
      }
      else if (item is SvgNestConfigViewModel)
      {
        if (SettingsEditorTemplate == null)
        {
          throw new System.InvalidOperationException($"{nameof(SettingsEditorTemplate)} not set.");
        }
        else
        {
          return SettingsEditorTemplate;
        }
      }
      else if (item is PartViewModel)
      {
        if (PartEditorTemplate == null)
        {
          throw new System.InvalidOperationException($"{nameof(PartEditorTemplate)} not set.");
        }
        else
        {
          return PartEditorTemplate;
        }
      }
      else if (item is PropertiesViewModel)
      {
        if (PropertiesEditorTemplate == null)
        {
          throw new System.InvalidOperationException($"{nameof(PropertiesEditorTemplate)} not set.");
        }
        else
        {
          return PropertiesEditorTemplate;
        }
      }

      return base.SelectTemplate(item, container);
    }
  }
}
