namespace DeepNestSharp.Domain.Models
{
  using System.Collections.Generic;
using System.ComponentModel;
  using System.Linq;
  using DeepNestLib;
  using DeepNestLib.NestProject;
  using DeepNestLib.Placement;

  public class ObservableNfp : ObservablePropertyObject, INfp
  {
    private readonly INfp item;
    
    public ObservableNfp(INfp nfp)
    {
      this.item = nfp;
    }

    /// <inheritdoc/>
    public INfp SourceItem => item;


    /// <inheritdoc/>
    public override bool IsDirty => true;

    [Description("The gross outer area, not discounting for any holes."), Category("Dimensions")]
    /// <inheritdoc/>
    public double Area => this.item.Area;

    /// <inheritdoc/>
    [Description("The children (aka holes) nested inside the part."), Category("Definition")]
    public IList<INfp> Children
    {
      get => this.item.Children;
      set => SetProperty(nameof(Children), () => this.item.Children, v => this.item.Children = v, value);
    }

    /// <inheritdoc/>
    public bool Fitted => this.item.Fitted;

    [Description("The overall height of the part."), Category("Dimensions")]
    /// <inheritdoc/>
    public double HeightCalculated => this.item.HeightCalculated;

    /// <inheritdoc/>
    [Description("The Id of the part."), Category("Description")]
    public int Id
    {
      get => this.item.Id;
      set => SetProperty(nameof(Id), () => this.item.Id, v => this.item.Id = v, value);
    }

    /// <inheritdoc/>
    public bool IsExact => !this.item.Points.Any(o => !o.Exact);

    /// <inheritdoc/>
    [Browsable(false)]
    public bool IsPriority
    {
      get => this.item.IsPriority;
      set => SetProperty(nameof(IsPriority), () => this.item.IsPriority, v => this.item.IsPriority = v, value);
    }

    /// <inheritdoc/>
    [Browsable(false)]
    public int Length => this.item.Length;

    /// <inheritdoc/>
    [Description("The MaxX of part's points."), Category("Placement")]
    public double MaxX => this.item.MaxX;

    /// <inheritdoc/>
    [Description("The MaxY of part's points."), Category("Placement")]
    public double MaxY => this.item.MaxY;

    /// <inheritdoc/>
    [Description("The MinX of part's points."), Category("Placement")]
    public double MinX => this.item.MinX;

    /// <inheritdoc/>
    [Description("The MinY of part's points."), Category("Placement")]
    public double MinY => this.item.MinY;

    /// <inheritdoc/>
    [Description("The name of file loaded as the part."), Category("Description")]
    public string Name
    {
      get => this.item.Name;
      set => SetProperty(nameof(Name), () => this.item.Name, v => this.item.Name = v, value);
    }

    /// <inheritdoc/>
    [Description("The X offset (Set and used by the export process)."), Category("Placement")]
    public double? Offsetx
    {
      get => this.item.Offsetx;
      set => SetProperty(nameof(Offsetx), () => this.item.Offsetx, v => this.item.Offsetx = v, value);
    }

    /// <inheritdoc/>
    [Description("The Y offset (Set and used by the export process)."), Category("Placement")]
    public double? Offsety
    {
      get => this.item.Offsety;
      set => SetProperty(nameof(Offsety), () => this.item.Offsety, v => this.item.Offsety = v, value);
    }

    /// <inheritdoc/>
    [Description("An index noting the order in the plcement sequence at which this part got inserted."), Category("Placement")]
    public int PlacementOrder
    {
      get => this.item.PlacementOrder;
      set => SetProperty(nameof(PlacementOrder), () => this.item.PlacementOrder, v => this.item.PlacementOrder = v, value);
    }

    /// <inheritdoc/>
    [Description("The points that make up the outer edge of the part."), Category("Definition")]
    SvgPoint[] IPolygon.Points => this.item.Points;

    /// <inheritdoc/>
    [Description("The degrees of rotation from the original imported part (tbc)."), Category("Placement")]
    public double Rotation
    {
      get => this.item.Rotation;
      set => SetProperty(nameof(Rotation), () => this.item.Rotation, v => this.item.Rotation = v, value);
    }

    /// <inheritdoc/>
    public INfp Sheet
    {
      get => this.item.Sheet;
      set => SetProperty(nameof(Sheet), () => this.item.Sheet, v => this.item.Sheet = v, value);
    }

    /// <inheritdoc/>
    public int Source
    {
      get => this.item.Source;
      set => SetProperty(nameof(Source), () => this.item.Source, v => this.item.Source = v, value);
    }

    /// <inheritdoc/>
    [Description("Denotes whether any restrictions on angle of placement have been imposed."), Category("Placement")]
    public AnglesEnum StrictAngle
    {
      get => this.item.StrictAngle;
      set => SetProperty(nameof(StrictAngle), () => this.item.StrictAngle, v => this.item.StrictAngle = v, value);
    }

    [Description("The overall width of the part."), Category("Dimensions")]
    /// <inheritdoc/>
    public double WidthCalculated => this.item.WidthCalculated;

    /// <inheritdoc/>
    [Description("The X offset of the part from the origin."), Category("Placement")]
    public double X
    {
      get => this.item.X;
      set => SetProperty(nameof(X), () => this.item.X, v => this.item.X = v, value);
    }

    /// <inheritdoc/>
    [Description("The Y offset of the part from the origin."), Category("Placement")]
    public double Y
    {
      get => this.item.Y;
      set => SetProperty(nameof(Y), () => this.item.Y, v => this.item.Y = v, value);
    }

    //public MainViewModel MainViewModel { get; }

    /// <inheritdoc/>
    public SvgPoint this[int ind] => this.item[ind];

    /// <inheritdoc/>
    public void AddPoint(SvgPoint point)
    {
      this.item.AddPoint(point);
    }

    /// <inheritdoc/>
    public NFP Clone()
    {
      return this.item.Clone();
    }

    /// <inheritdoc/>
    public NFP CloneExact()
    {
      return this.item.CloneExact();
    }

    /// <inheritdoc/>
    public INfp CloneTree()
    {
      return this.item.CloneTree();
    }

    /// <inheritdoc/>
    public INfp CloneTop()
    {
      return this.item.CloneTop();
    }

    /// <inheritdoc/>
    public NFP GetHull()
    {
      return this.item.GetHull();
    }

    /// <inheritdoc/>
    public void ReplacePoints(IEnumerable<SvgPoint> points)
    {
      this.item.ReplacePoints(points);
    }

    /// <inheritdoc/>
    public void ReplacePoints(INfp replacementNfp)
    {
      this.item.ReplacePoints(replacementNfp);
    }

    /// <inheritdoc/>
    public void Reverse()
    {
      this.item.Reverse();
    }

    /// <inheritdoc/>
    public INfp Rotate(double degrees)
    {
      return this.item.Rotate(degrees);
    }

    /// <inheritdoc/>
    public INfp Slice(int v)
    {
      return this.item.Slice(v);
    }

    /// <inheritdoc/>
    public string Stringify()
    {
      return this.item.Stringify();
    }

    /// <inheritdoc/>
    public string ToJson()
    {
      return this.item.ToJson();
    }

    /// <inheritdoc/>
    public string ToShortString()
    {
      return this.item.ToShortString();
    }

    /// <inheritdoc/>
    public string ToOpenScadPolygon()
    {
      return this.item.ToOpenScadPolygon();
    }

    public INfp Shift(IPartPlacement shift)
    {
      return this.item.Shift(shift);
    }

    public INfp Shift(double x, double y)
    {
      return this.item.Shift(x, y);
    }

    public INfp ShiftToOrigin()
    {
      return this.item.ShiftToOrigin();
    }
  }
}
