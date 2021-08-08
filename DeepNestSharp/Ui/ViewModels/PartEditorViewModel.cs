namespace DeepNestSharp.Ui.ViewModels
{
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Ui.Docking;
  using DeepNestSharp.Ui.Models;

  public class PartEditorViewModel : FileViewModel
  {
    private INfp? part;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartEditorViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public PartEditorViewModel(MainViewModel mainViewModel)
      : base(mainViewModel)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartEditorViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    /// <param name="filePath">Path to the file to open.</param>
    public PartEditorViewModel(MainViewModel mainViewModel, string filePath)
      : base(mainViewModel, filePath)
    {
    }

    public INfp? Part
    {
      get
      {
        return this.part;
      }

      set
      {
        SetProperty(ref part, value, nameof(Part));
      }
    }

    public override string TextContent => this.Part?.ToJson() ?? string.Empty;

    protected override void LoadContent()
    {
      var part = DxfParser.LoadDxfFile(this.FilePath).Result.ToNfp();
      this.Part =new ObservableNfp( Background.ShiftPolygon(part, -part?.MinX ?? 0, -part?.MinY ?? 0));
    }

    protected override void NotifyContentUpdated()
    {
      OnPropertyChanged(nameof(SheetPlacement));
    }
  }
}