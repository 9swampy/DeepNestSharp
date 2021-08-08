namespace DeepNestSharp.Ui.ViewModels
{
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Models;
  using DeepNestSharp.Ui.Docking;

  public class NestResultViewModel : FileViewModel
  {
    private INestResult? nestResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="NestResultViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public NestResultViewModel(MainViewModel mainViewModel)
      : base(mainViewModel)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NestResultViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    /// <param name="filePath">Path to the file to open.</param>
    public NestResultViewModel(MainViewModel mainViewModel, string filePath)
      : base(mainViewModel, filePath)
    {
    }

    public NestResultViewModel(MainViewModel mainViewModel, INestResult nestResult)
      : this(mainViewModel)
    {
      this.nestResult = nestResult;
    }

    public INestResult? NestResult => this.nestResult;

    public override string FileDialogFilter => DeepNestLib.Placement.NestResult.FileDialogFilter;

    public override string TextContent => this.NestResult?.ToJson() ?? string.Empty;

    protected override void LoadContent()
    {
      // var part = DxfParser.LoadDxfFile(this.FilePath).Result.ToNfp();
      // this.Part = new ObservableNfp(Background.ShiftPolygon(part, -part?.MinX ?? 0, -part?.MinY ?? 0));
    }

    protected override void NotifyContentUpdated()
    {
      OnPropertyChanged(nameof(NestResult));
    }

    protected override void SaveState()
    {
      // Don't do anything, DeepNestSharp only consumes and can be used to inspect Part files.
    }
  }
}