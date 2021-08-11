namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using DeepNestLib;
  using DeepNestSharp.Domain.Models;
  using DeepNestSharp.Ui.Docking;
  using Microsoft.Toolkit.Mvvm.Input;

  public class PartEditorViewModel : FileViewModel
  {
    private INfp? part;
    private RelayCommand<string> rotateCommand;

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

    public override string FileDialogFilter => NFP.FileDialogFilter;

    public IRelayCommand<string> RotateCommand => rotateCommand ?? (rotateCommand = new RelayCommand<string>(OnRotate));

    private void OnRotate(string? degrees)
    {
      double castDegrees;
      if (degrees != null && double.TryParse(degrees, out castDegrees))
      {
        this.Part = new ObservableNfp(this.Part?.Rotate(castDegrees));
      }
    }

    public override string TextContent => this.Part?.ToJson() ?? string.Empty;

    protected override void LoadContent()
    {
      var part = DxfParser.LoadDxfFile(this.FilePath).Result.ToNfp();
      this.Part = new ObservableNfp(Background.ShiftPolygon(part, -part?.MinX ?? 0, -part?.MinY ?? 0));
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