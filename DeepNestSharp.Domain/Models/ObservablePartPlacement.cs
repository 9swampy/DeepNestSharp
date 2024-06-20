namespace DeepNestSharp.Domain.Models
{
  using System.Threading.Tasks;
  using System.Windows.Input;
  using CommunityToolkit.Mvvm.Input;
  using DeepNestLib;
  using DeepNestLib.IO;
  using DeepNestLib.Placement;

  public class ObservablePartPlacement : ObservablePropertyObject, IPartPlacement
  {
    private readonly IPartPlacement partPlacement;
    private readonly IPointXY originalPosition;
    private readonly double originalRotation;
    private RelayCommand resetCommand;
    private IAsyncRelayCommand loadExactCommand;

    public ObservablePartPlacement(IPartPlacement partPlacement, int order)
    {
      this.partPlacement = partPlacement;
      this.Order = order;
      this.originalPosition = new SvgPoint(partPlacement.X, partPlacement.Y);
      this.originalRotation = partPlacement.Rotation;
      this.PropertyChanged += this.ObservablePartPlacement_PropertyChanged;
    }

    /// <inheritdoc/>
    public bool IsDragging
    {
      get => this.partPlacement.IsDragging;
      set => this.SetProperty(nameof(this.IsDragging), () => this.partPlacement.IsDragging, v => this.partPlacement.IsDragging = v, value);
    }

    /// <inheritdoc/>
    public int Source
    {
      get => this.partPlacement.Source;
      set => this.SetProperty(nameof(this.Source), () => this.partPlacement.Source, v => this.partPlacement.Source = v, value);
    }

    /// <inheritdoc/>
    public int Id
    {
      get => this.partPlacement.Id;
      set => this.SetProperty(nameof(this.Id), () => this.partPlacement.Id, v => this.partPlacement.Id = v, value);
    }

    public ICommand ResetCommand => this.resetCommand ?? (this.resetCommand = new RelayCommand(this.OnReset, () => this.IsDirty));

    public IAsyncRelayCommand LoadExactCommand
    {
      get
      {
        if (this.loadExactCommand == null)
        {
          this.loadExactCommand = new AsyncRelayCommand(this.OnLoadExact, () => !this.IsExact);
        }

        return this.loadExactCommand;
      }
    }

    /// <inheritdoc/>
    public double X
    {
      get => this.partPlacement.X;
      set => this.SetProperty(nameof(this.X), () => this.partPlacement.X, v => this.partPlacement.X = v, value);
    }

    /// <inheritdoc/>
    public double Y
    {
      get => this.partPlacement.Y;
      set => this.SetProperty(nameof(this.Y), () => this.partPlacement.Y, v => this.partPlacement.Y = v, value);
    }

    ///// <inheritdoc/>
    //public INfp Hull
    //{
    //  get => partPlacement.Hull;
    //  set => SetProperty(nameof(Hull), () => partPlacement.Hull, v => partPlacement.Hull = v, value);
    //}

    ///// <inheritdoc/>
    //public INfp HullSheet
    //{
    //  get => partPlacement.HullSheet;
    //  set => SetProperty(nameof(HullSheet), () => partPlacement.HullSheet, v => partPlacement.HullSheet = v, value);
    //}

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
    public double? MergedLength => this.partPlacement.MergedLength;

    /// <inheritdoc/>
    public object MergedSegments
    {
      get => this.partPlacement.MergedSegments;
      set => this.SetProperty(nameof(this.MergedSegments), () => this.partPlacement.MergedSegments, v => this.partPlacement.MergedSegments = v, value);
    }

    /// <inheritdoc/>
    public double MinX => this.partPlacement.MinX;

    /// <inheritdoc/>
    public double MinY => this.partPlacement.MinY;

    /// <inheritdoc/>
    public INfp Part => this.partPlacement.Part;

    /// <inheritdoc/>
    public INfp PlacedPart => this.partPlacement.PlacedPart;

    /// <inheritdoc/>
    public double Rotation
    {
      get => this.partPlacement.Rotation;
      set => this.partPlacement.Rotation = value;
    }

    /// <inheritdoc/>
    public bool IsExact => this.Part.IsExact;

    public int Order { get; private set; }

    /// <inheritdoc/>
    private void ObservablePartPlacement_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(this.IsDirty))
      {
        this.resetCommand?.NotifyCanExecuteChanged();
      }
    }

    /// <inheritdoc/>
    private void OnReset()
    {
      this.X = this.originalPosition.X;
      this.Y = this.originalPosition.Y;
      this.Rotation = this.originalRotation;
    }

    /// <inheritdoc/>
    public async Task OnLoadExact()
    {
      IRawDetail raw = await DxfParser.LoadDxfFile(this.Part.Name);
      INfp loadedNfp;
      if (raw.TryConvertToNfp(this.Part.Source, out loadedNfp))
      {
        loadedNfp = loadedNfp.Rotate(this.Part.Rotation);
        this.Part.ReplacePoints(loadedNfp);
        this.OnPropertyChanged(nameof(this.IsExact));
        this.loadExactCommand?.NotifyCanExecuteChanged();
      }
    }
  }
}
