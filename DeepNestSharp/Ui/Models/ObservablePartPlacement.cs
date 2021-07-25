namespace DeepNestSharp.Ui.Models
{
  using DeepNestLib;
  using DeepNestLib.Placement;

  public class ObservablePartPlacement : ObservablePropertyObject, IPartPlacement
  {
    private readonly IPartPlacement partPlacement;
    private System.Windows.Media.PointCollection points;

    public ObservablePartPlacement(IPartPlacement partPlacement) => this.partPlacement = partPlacement;

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
  }
}
