namespace DeepNestSharp.Ui.ViewModels
{
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Ui.Docking;
  using DeepNestSharp.Ui.Models;

  public class PartViewModel : FileViewModel
  {
    private INfp? part;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public PartViewModel(MainViewModel mainViewModel)
      : base(mainViewModel)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    /// <param name="filePath">Path to the file to open.</param>
    public PartViewModel(MainViewModel mainViewModel, string filePath)
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
      this.Part = new ObservableNfp(DxfParser.LoadDxfFile(this.FilePath).ToNfp());
    }

    protected override void NotifyContentUpdated()
    {
      OnPropertyChanged(nameof(SheetPlacement));
    }
  }
}