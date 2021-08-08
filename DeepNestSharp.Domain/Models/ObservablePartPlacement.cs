namespace DeepNestSharp.Domain.Models
{
  using System.Threading.Tasks;
  using System.Windows.Input;
  using DeepNestLib;
  using DeepNestLib.Placement;
  using Microsoft.Toolkit.Mvvm.Input;

  public class ObservablePartPlacement : ObservablePropertyObject, IPartPlacement
  {
    private readonly IPartPlacement partPlacement;
    private readonly IPointXY originalPosition;
    private readonly double originalRotation;
    private RelayCommand resetCommand;
    private IAsyncRelayCommand loadExactCommand;

    public ObservablePartPlacement(IPartPlacement partPlacement)
    {
      this.partPlacement = partPlacement;
      this.originalPosition = new SvgPoint(partPlacement.X, partPlacement.Y);
      this.originalRotation = partPlacement.Rotation;
      this.PropertyChanged += this.ObservablePartPlacement_PropertyChanged;
    }

    /// <inheritdoc/>
    public bool IsDragging
    {
      get => partPlacement.IsDragging;
      set => SetProperty(nameof(IsDragging), () => partPlacement.IsDragging, v => partPlacement.IsDragging = v, value);
    }

    /// <inheritdoc/>
    public int Source
    {
      get => partPlacement.Source;
      set => SetProperty(nameof(Source), () => partPlacement.Source, v => partPlacement.Source = v, value);
    }

    /// <inheritdoc/>
    public int Id
    {
      get => partPlacement.Id;
      set => SetProperty(nameof(Id), () => partPlacement.Id, v => partPlacement.Id = v, value);
    }

    public ICommand ResetCommand
    {
      get
      {
        if (resetCommand == null)
        {
          resetCommand = new RelayCommand(OnReset, () => IsDirty);
        }

        return resetCommand;
      }
    }

    public IAsyncRelayCommand LoadExactCommand
    {
      get
      {
        if (loadExactCommand == null)
        {
          loadExactCommand = new AsyncRelayCommand(OnLoadExact, () => !IsExact);
        }

        return loadExactCommand;
      }
    }

    /// <inheritdoc/>
    public double X
    {
      get => partPlacement.X;
      set => SetProperty(nameof(X), () => partPlacement.X, v => partPlacement.X = v, value);
    }

    /// <inheritdoc/>
    public double Y
    {
      get => partPlacement.Y;
      set => SetProperty(nameof(Y), () => partPlacement.Y, v => partPlacement.Y = v, value);
    }

    /// <inheritdoc/>
    public INfp Hull
    {
      get => partPlacement.Hull;
      set => SetProperty(nameof(Hull), () => partPlacement.Hull, v => partPlacement.Hull = v, value);
    }

    /// <inheritdoc/>
    public INfp HullSheet
    {
      get => partPlacement.HullSheet;
      set => SetProperty(nameof(HullSheet), () => partPlacement.HullSheet, v => partPlacement.HullSheet = v, value);
    }

    /// <inheritdoc/>
    public override bool IsDirty
    {
      get
      {
        return this.originalPosition.X != this.partPlacement.X ||
               this.originalPosition.Y != this.partPlacement.Y ||
               this.originalRotation != this.partPlacement.Rotation;
      }
    }

    /// <inheritdoc/>
    public double MaxX => this.partPlacement.MaxX;

    /// <inheritdoc/>
    public double MaxY => this.partPlacement.MaxY;

    /// <inheritdoc/>
    public double? MergedLength => partPlacement.MergedLength;

    /// <inheritdoc/>
    public object MergedSegments
    {
      get => partPlacement.MergedSegments;
      set => SetProperty(nameof(MergedSegments), () => partPlacement.MergedSegments, v => partPlacement.MergedSegments = v, value);
    }

    /// <inheritdoc/>
    public double MinX => this.partPlacement.MinX;

    /// <inheritdoc/>
    public double MinY => this.partPlacement.MinY;

    /// <inheritdoc/>
    public INfp Part => partPlacement.Part;

    /// <inheritdoc/>
    public double Rotation
    {
      get => partPlacement.Rotation;
      set => partPlacement.Rotation = value;
    }

    /// <inheritdoc/>
    public bool IsExact => Part.IsExact;

    /// <inheritdoc/>
    private void ObservablePartPlacement_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(IsDirty))
      {
        resetCommand?.NotifyCanExecuteChanged();
      }
    }

    /// <inheritdoc/>
    private void OnReset()
    {
      this.X = originalPosition.X;
      this.Y = originalPosition.Y;
      this.Rotation = originalRotation;
    }

    /// <inheritdoc/>
    private async Task OnLoadExact()
    {
      var raw = await DxfParser.LoadDxfFile(this.Part.Name);
      INfp loadedNfp;
      if (raw.TryConvertToNfp(this.Part.Source, out loadedNfp))
      {
        loadedNfp = loadedNfp.Rotate(this.Part.Rotation);
        this.Part.ReplacePoints(loadedNfp);
        OnPropertyChanged(nameof(IsExact));
        loadExactCommand?.NotifyCanExecuteChanged();
      }
    }
  }
}
