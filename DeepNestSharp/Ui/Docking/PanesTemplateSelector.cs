namespace DeepNestSharp.Ui.Docking
{
  using System.Windows;
  using System.Windows.Controls;
  using DeepNestSharp.Domain.ViewModels;

  internal class PanesTemplateSelector : DataTemplateSelector
  {
    public PanesTemplateSelector()
    {
    }

    public DataTemplate? NfpCandidateListTemplate
    {
      get;
      set;
    }

    public DataTemplate? SheetPlacementEditorTemplate
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

    public DataTemplate? NestResultEditorTemplate
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
        if (this.NestMonitorTemplate == null)
        {
          throw new System.InvalidOperationException($"{nameof(this.NestMonitorTemplate)} not set.");
        }
        else
        {
          return this.NestMonitorTemplate;
        }
      }
      else if (item is INestProjectViewModel)
      {
        if (this.NestProjectEditorTemplate == null)
        {
          throw new System.InvalidOperationException($"{nameof(this.NestProjectEditorTemplate)} not set.");
        }
        else
        {
          return this.NestProjectEditorTemplate;
        }
      }
      else if (item is NestResultViewModel)
      {
        if (this.NestResultEditorTemplate == null)
        {
          throw new System.InvalidOperationException($"{nameof(this.NestResultEditorTemplate)} not set.");
        }
        else
        {
          return this.NestResultEditorTemplate;
        }
      }
      else if (item is NfpCandidateListViewModel)
      {
        if (this.NfpCandidateListTemplate == null)
        {
          throw new System.InvalidOperationException($"{nameof(this.NfpCandidateListTemplate)} not set.");
        }
        else
        {
          return this.NfpCandidateListTemplate;
        }
      }
      else if (item is SheetPlacementViewModel)
      {
        if (this.SheetPlacementEditorTemplate == null)
        {
          throw new System.InvalidOperationException($"{nameof(this.SheetPlacementEditorTemplate)} not set.");
        }
        else
        {
          return this.SheetPlacementEditorTemplate;
        }
      }
      else if (item is PreviewViewModel)
      {
        if (this.PreviewTemplate == null)
        {
          throw new System.InvalidOperationException($"{nameof(this.PreviewTemplate)} not set.");
        }
        else
        {
          return this.PreviewTemplate;
        }
      }
      else if (item is SvgNestConfigViewModel)
      {
        if (this.SettingsEditorTemplate == null)
        {
          throw new System.InvalidOperationException($"{nameof(this.SettingsEditorTemplate)} not set.");
        }
        else
        {
          return this.SettingsEditorTemplate;
        }
      }
      else if (item is PartEditorViewModel)
      {
        if (this.PartEditorTemplate == null)
        {
          throw new System.InvalidOperationException($"{nameof(this.PartEditorTemplate)} not set.");
        }
        else
        {
          return this.PartEditorTemplate;
        }
      }
      else if (item is PropertiesViewModel)
      {
        if (this.PropertiesEditorTemplate == null)
        {
          throw new System.InvalidOperationException($"{nameof(this.PropertiesEditorTemplate)} not set.");
        }
        else
        {
          return this.PropertiesEditorTemplate;
        }
      }

      return base.SelectTemplate(item, container);
    }
  }
}
