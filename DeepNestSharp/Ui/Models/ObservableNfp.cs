namespace DeepNestSharp.Ui.Models
{
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using DeepNestLib;
  using DeepNestLib.NestProject;

  public class ObservableNfp : ObservablePropertyObject, INfp
  {
    private readonly INfp item;
    private System.Windows.Media.PointCollection points;

    public ObservableNfp(INfp nfp)
    {
      this.item = nfp;
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
          loadedNfp = item;
          //}

          AddToPoints(loadedNfp);
        }

        return points;
      }
    }

    private void AddToPoints(INfp loadedNfp)
    {
      foreach (var p in loadedNfp.Points)
      {
        points.Add(new System.Windows.Point(p.X, p.Y));
        //foreach (var child in loadedNfp.Children)
        //{
        //  AddToPoints(child);
        //}
      }
    }

    public override bool IsDirty => true;

    public double Area => this.item.Area;

    public IList<INfp> Children
    {
      get => this.item.Children;
      set => SetProperty(nameof(Children), () => this.item.Children, v => this.item.Children = v, value);
    }

    public bool Fitted => this.item.Fitted;

    public double HeightCalculated => this.item.HeightCalculated;

    public int Id
    {
      get => this.item.Id;
      set => SetProperty(nameof(Id), () => this.item.Id, v => this.item.Id = v, value);
    }

    public bool IsPriority
    {
      get => this.item.IsPriority;
      set => SetProperty(nameof(IsPriority), () => this.item.IsPriority, v => this.item.IsPriority = v, value);
    }

    public int Length => this.item.Length;

    public double MaxX => this.item.MaxX;

    public double MaxY => this.item.MaxY;

    public double MinX => this.item.MinX;

    public double MinY => this.item.MinY;

    public string Name
    {
      get => this.item.Name;
      set => SetProperty(nameof(Name), () => this.item.Name, v => this.item.Name = v, value);
    }

    public double? Offsetx
    {
      get => this.item.Offsetx;
      set => SetProperty(nameof(Offsetx), () => this.item.Offsetx, v => this.item.Offsetx = v, value);
    }

    public double? Offsety
    {
      get => this.item.Offsety;
      set => SetProperty(nameof(Offsety), () => this.item.Offsety, v => this.item.Offsety = v, value);
    }

    public int PlacementOrder
    {
      get => this.item.PlacementOrder;
      set => SetProperty(nameof(PlacementOrder), () => this.item.PlacementOrder, v => this.item.PlacementOrder = v, value);
    }

    SvgPoint[] INfp.Points => this.item.Points;

    public double Rotation
    {
      get => this.item.Rotation;
      set => SetProperty(nameof(Rotation), () => this.item.Rotation, v => this.item.Rotation = v, value);
    }

    public INfp Sheet
    {
      get => this.item.Sheet;
      set => SetProperty(nameof(Sheet), () => this.item.Sheet, v => this.item.Sheet = v, value);
    }

    public int Source
    {
      get => this.item.Source;
      set => SetProperty(nameof(Source), () => this.item.Source, v => this.item.Source = v, value);
    }

    public AnglesEnum StrictAngle
    {
      get => this.item.StrictAngle;
      set => SetProperty(nameof(StrictAngle), () => this.item.StrictAngle, v => this.item.StrictAngle = v, value);
    }

    public double WidthCalculated => this.item.WidthCalculated;

    public double X
    {
      get => this.item.X;
      set => SetProperty(nameof(X), () => this.item.X, v => this.item.X = v, value);
    }

    public double Y
    {
      get => this.item.Y;
      set => SetProperty(nameof(Y), () => this.item.Y, v => this.item.Y = v, value);
    }

    public SvgPoint this[int ind] => this.item[ind];

    public void AddPoint(SvgPoint point)
    {
      this.item.AddPoint(point);
    }

    public NFP Clone()
    {
      return this.item.Clone();
    }

    public NFP CloneExact()
    {
      return this.item.CloneExact();
    }

    public NFP CloneTree()
    {
      return this.item.CloneTree();
    }

    public INfp CloneTop()
    {
      return this.item.CloneTop();
    }

    public NFP GetHull()
    {
      return this.item.GetHull();
    }

    public void ReplacePoints(IEnumerable<SvgPoint> points)
    {
      this.item.ReplacePoints(points);
    }

    public void Reverse()
    {
      this.item.Reverse();
    }

    public NFP Rotate(double degrees)
    {
      return this.item.Rotate(degrees);
    }

    public INfp Slice(int v)
    {
      return this.item.Slice(v);
    }

    public string Stringify()
    {
      return this.item.Stringify();
    }

    public string ToJson()
    {
      return this.item.ToJson();
    }

    public string ToShortString()
    {
      return this.item.ToShortString();
    }

    public string ToOpenScadPolygon()
    {
      return this.item.ToOpenScadPolygon();
    }
  }
}
