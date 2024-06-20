namespace DeepNestSharp.Domain.ViewModels
{
  using System.IO;
  using CommunityToolkit.Mvvm.Input;
  using DeepNestLib;
  using DeepNestLib.IO;
  using DeepNestSharp.Domain.Models;
  using DeepNestSharp.Ui.Docking;

  public class PartEditorViewModel : FileViewModel
  {
    private INfp part;
    private RelayCommand<string> rotateCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartEditorViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public PartEditorViewModel(IMainViewModel mainViewModel)
      : base(mainViewModel)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartEditorViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    /// <param name="filePath">Path to the file to open.</param>
    public PartEditorViewModel(IMainViewModel mainViewModel, string filePath)
      : base(mainViewModel, filePath)
    {
    }

    public INfp Part
    {
      get
      {
        return this.part;
      }

      set
      {
        this.SetProperty(ref this.part, value, nameof(this.Part));
      }
    }

    public override string FileDialogFilter => NoFitPolygon.FileDialogFilter;

    public IRelayCommand<string> RotateCommand => this.rotateCommand ?? (this.rotateCommand = new RelayCommand<string>(this.OnRotate));

    private void OnRotate(string degrees)
    {
      double castDegrees;
      if (degrees != null && double.TryParse(degrees, out castDegrees))
      {
        this.Part = new ObservableNfp(this.Part?.Rotate(castDegrees).ShiftToOrigin());
      }
    }

    public override string TextContent => this.Part?.ToJson() ?? string.Empty;

    protected override void LoadContent()
    {
      FileInfo fileInfo = new FileInfo(this.FilePath);
      if (!fileInfo.Exists)
      {
        this.MainViewModel.MessageService.DisplayMessageBox($"File not found: {this.FilePath}.", "File Not Found", MessageBoxIcon.Information);
        return;
      }

      if (fileInfo.Extension == ".dxf")
      {
        INfp part = DxfParser.LoadDxfFile(this.FilePath).Result.ToNfp();
        this.Part = new ObservableNfp(part.Shift(-part?.MinX ?? 0, -part?.MinY ?? 0));
      }
      else
      {
        NoFitPolygon part = NoFitPolygon.LoadFromFile(this.FilePath);
        this.Part = new ObservableNfp(part.Shift(-part?.MinX ?? 0, -part?.MinY ?? 0));
      }
    }

    protected override void NotifyContentUpdated()
    {
      this.OnPropertyChanged(nameof(this.Part));
    }

    protected override void SaveState()
    {
      // Don't do anything, DeepNestSharp only consumes and can be used to inspect Part files.
    }
  }
}