namespace DeepNestSharp.Domain.ViewModels
{
  using System.IO;
  using DeepNestLib;
  using DeepNestLib.IO;
  using DeepNestSharp.Domain.Models;
  using DeepNestSharp.Ui.Docking;
  using Light.GuardClauses;
  using Microsoft.Toolkit.Mvvm.Input;

  public class PartEditorViewModel : FileViewModel
  {
    private INfp part;
    private RelayCommand<string> rotateCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartEditorViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public PartEditorViewModel(IMainViewModel mainViewModel, IRelativePathHelper relativePathHelper)
      : base(mainViewModel, relativePathHelper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartEditorViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    /// <param name="filePath">Path to the file to open.</param>
    public PartEditorViewModel(IMainViewModel mainViewModel, string filePath, IRelativePathHelper relativePathHelper)
      : base(mainViewModel, filePath, relativePathHelper)
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
        SetProperty(ref part, value, nameof(Part));
      }
    }

    public override string FileDialogFilter => NoFitPolygon.FileDialogFilter;

    public IRelayCommand<string> RotateCommand => rotateCommand ?? (rotateCommand = new RelayCommand<string>(OnRotate));

    private void OnRotate(string degrees)
    {
      double castDegrees;
      if (degrees != null && double.TryParse(degrees, out castDegrees))
      {
        this.Part = new ObservableNfp(this.Part?.Rotate(castDegrees).ShiftToOrigin());
      }
    }

    public override string TextContent => this.Part?.ToJson() ?? string.Empty;

    protected override void LoadContent(IRelativePathHelper relativePathHelper)
    {
      relativePathHelper.MustNotBeNull();
      var fileInfo = new FileInfo(this.FilePath);
      if (!fileInfo.Exists)
      {
        this.MainViewModel.MessageService.DisplayMessageBox($"File not found: {this.FilePath}.", "File Not Found", MessageBoxIcon.Information);
        return;
      }

      if (fileInfo.Extension == ".dxf")
      {
        var part = DxfParser.LoadDxfFile(this.FilePath).Result.ToNfp();
        this.Part = new ObservableNfp(part.Shift(-part?.MinX ?? 0, -part?.MinY ?? 0));
      }
      else
      {
        var part = NoFitPolygon.LoadFromFile(this.FilePath);
        this.Part = new ObservableNfp(part.Shift(-part?.MinX ?? 0, -part?.MinY ?? 0));
      }
    }

    protected override void NotifyContentUpdated()
    {
      OnPropertyChanged(nameof(Part));
    }

    protected override void SaveState()
    {
      // Don't do anything, DeepNestSharp only consumes and can be used to inspect Part files.
    }
  }
}