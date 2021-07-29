namespace DeepNestSharp.Ui.Models
{
  using System.Windows;
  using System.Windows.Input;
  using DeepNestLib;
  using DeepNestLib.Placement;
  using Microsoft.Toolkit.Mvvm.Input;

  public class ObservablePartPlacement : ObservablePropertyObject, IPartPlacement
  {
    private readonly IPartPlacement partPlacement;
    private readonly Point originalPosition;
    private readonly double originalRotation;
    private System.Windows.Media.PointCollection? points;
    private RelayCommand? resetCommand = null;

    public ObservablePartPlacement(IPartPlacement partPlacement)
    {
      this.partPlacement = partPlacement;
      this.originalPosition = new Point(partPlacement.X, partPlacement.Y);
      this.originalRotation = partPlacement.Rotation;
      this.PropertyChanged += this.ObservablePartPlacement_PropertyChanged;
    }

    public int Source
    {
      get => partPlacement.Source;
      set => SetProperty(nameof(Source), () => partPlacement.Source, v => partPlacement.Source = v, value);
    }

    public System.Windows.Media.PointCollection Points
    {
      get
      {
        if (points == null)
        {
          points = new System.Windows.Media.PointCollection();
          INfp loadedNfp;

          //var fi = new FileInfo(partPlacement.Part.Name);
          //var baseDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
          //while (!fi.Exists && baseDir != null)
          //{
          //  fi = new FileInfo(Path.Join(baseDir.FullName, partPlacement.Part.Name));
          //  if (fi.Exists)
          //  {
          //    break;
          //  }
          //  else
          //  {
          //    baseDir = baseDir.Parent;
          //  }
          //}

          //if (fi.Exists && DxfParser.LoadDxfFile(fi.FullName).TryGetNfp(partPlacement.Part.Source, out loadedNfp))
          //{
          //  loadedNfp = loadedNfp.Rotate(partPlacement.Part.Rotation);
          //  loadedNfp.X = partPlacement.Part.X;
          //  loadedNfp.Y = partPlacement.Part.Y;
          //}
          //else
          //{
          loadedNfp = partPlacement.Part;
          //}

          foreach (var p in loadedNfp.Points)
          {
            points.Add(new System.Windows.Point(p.X, p.Y));
          }
        }

        return points;
      }
    }

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

    public double X
    {
      get => partPlacement.X;
      set => SetProperty(nameof(X), () => partPlacement.X, v => partPlacement.X = v, value);
    }

    public double Y
    {
      get => partPlacement.Y;
      set => SetProperty(nameof(Y), () => partPlacement.Y, v => partPlacement.Y = v, value);
    }

    public INfp Hull
    {
      get => partPlacement.Hull;
      set => SetProperty(nameof(Hull), () => partPlacement.Hull, v => partPlacement.Hull = v, value);
    }

    public INfp HullSheet
    {
      get => partPlacement.HullSheet;
      set => SetProperty(nameof(HullSheet), () => partPlacement.HullSheet, v => partPlacement.HullSheet = v, value);
    }

    public override bool IsDirty
    {
      get
      {
        return this.originalPosition.X != this.partPlacement.X ||
               this.originalPosition.Y != this.partPlacement.Y ||
               this.originalRotation != this.partPlacement.Rotation;
      }
    }

    public double? MergedLength => partPlacement.MergedLength;

    public object MergedSegments
    {
      get => partPlacement.MergedSegments;
      set => SetProperty(nameof(MergedSegments), () => partPlacement.MergedSegments, v => partPlacement.MergedSegments = v, value);
    }

    public INfp Part => partPlacement.Part;

    public double Rotation
    {
      get => partPlacement.Rotation;
      set => partPlacement.Rotation = value;
    }

    private void ObservablePartPlacement_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(IsDirty))
      {
        resetCommand?.NotifyCanExecuteChanged();
      }
    }

    private void OnReset()
    {
      this.X = originalPosition.X;
      this.Y = originalPosition.Y;
      this.Rotation = originalRotation;
    }
  }
}
