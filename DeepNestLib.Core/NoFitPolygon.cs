namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using DeepNestLib.NestProject;
  using DeepNestLib.Placement;
  using IxMilia.Dxf;
  using IxMilia.Dxf.Entities;

  public class NoFitPolygon : PolygonBase, INfp, IHiddenNfp, IStringify
  {
    public const string FileDialogFilter = "AutoCad Drawing Exchange Format (*.dxf)|*.dxf|DeepNest Polygon (*.dnpoly)|*.dnpoly|All files (*.*)|*.*";
    private double rotation;

    public NoFitPolygon(IList<INfp> children)
        : this()
    {
      Children = children;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoFitPolygon"/> class.
    /// Creates a true clone of the source; only Sheet is a common object reference.
    /// </summary>
    /// <param name="source">The original object to clone.</param>
    public NoFitPolygon(INfp source, WithChildren withChildren)
      : this(source.Points)
    {
      this.Id = source.Id;
      this.IsPriority = source.IsPriority;
      this.Name = source.Name;
      this.OffsetX = source.OffsetX;
      this.OffsetY = source.OffsetY;
      this.PlacementOrder = source.PlacementOrder;
      this.Rotation = source.Rotation;
      this.Sheet = source.Sheet;
      this.Source = source.Source;
      this.StrictAngle = source.StrictAngle;
      this.X = source.X;
      this.Y = source.Y;

      if (withChildren == WithChildren.Included)
      {
        foreach (var child in source.Children)
        {
          this.Children.Add(new NoFitPolygon(child, withChildren));
        }
      }
    }

    public NoFitPolygon()
      : base(new SvgPoint[0])
    {
    }

    public NoFitPolygon(IEnumerable<SvgPoint> points)
      : base(points.DeepClone())
    {
    }

    /// <inheritdoc />
    public bool Fitted
    {
      get
      {
        return this.Sheet != null;
      }
    }

    /// <inheritdoc />
    public INfp Sheet { get; set; }

    /// <inheritdoc />
    public string Name { get; set; } = string.Empty;

    [JsonConverter(typeof(DoublePrecisionConverter))]
    /// <inheritdoc />
    public double X { get; set; }

    [JsonIgnore]
    /// <inheritdoc />
    public double MaxX => points.Length == 0 ? 0 : points.Max(p => p.X);

    [JsonIgnore]
    /// <inheritdoc />
    public double MinX => points.Length == 0 ? 0 : points.Min(p => p.X);

    [JsonConverter(typeof(DoublePrecisionConverter))]
    /// <inheritdoc />
    public double Y { get; set; }

    [JsonIgnore]
    /// <inheritdoc />
    public double MaxY => points.Length == 0 ? 0 : points.Max(p => p.Y);

    [JsonIgnore]
    /// <inheritdoc />
    public double MinY => points.Length == 0 ? 0 : points.Min(p => p.Y);

    [JsonIgnore]
    /// <inheritdoc />
    public double WidthCalculated
    {
      get
      {
        if (this.points.Length == 0)
        {
          return 0;
        }

        var maxx = this.points.Max(z => z.X);
        var minx = this.points.Min(z => z.X);

        return maxx - minx;
      }
    }

    [JsonIgnore]
    /// <inheritdoc />
    public double HeightCalculated
    {
      get
      {
        if (this.points.Length == 0)
        {
          return 0;
        }

        var maxy = this.points.Max(z => z.Y);
        var miny = this.points.Min(z => z.Y);
        return maxy - miny;
      }
    }

    /// <inheritdoc />
    public IList<INfp> Children { get; set; } = new List<INfp>();

    [JsonIgnore]
    /// <inheritdoc />
    public int Length
    {
      get
      {
        return this.points.Length;
      }
    }

    /// <inheritdoc />
    public int Id { get; set; }

    [JsonConverter(typeof(DoublePrecisionConverter))]
    /// <inheritdoc />
    public double? OffsetX { get; set; }

    [JsonConverter(typeof(DoublePrecisionConverter))]
    /// <inheritdoc />
    public double? OffsetY { get; set; }

    /// <inheritdoc />
    public int Source { get; set; } = -1;

    /// <inheritdoc />
    public int PlacementOrder { get; set; } = -1;

    [JsonConverter(typeof(DoublePrecisionConverter))]
    /// <inheritdoc />
    public double Rotation
    {
      get
      {
        return this.rotation;
      }

      set
      {
        if (value > 360D)
        {
          value = value % 360D;
        }

        this.rotation = value;
      }
    }

    /// <inheritdoc />
    public SvgPoint[] Points
    {
      get
      {
        return this.points;
      }

      set
      {
        this.points = value;
      }
    }

    /// <inheritdoc />
    [JsonIgnore]
    public double Area
    {
      get
      {
        return (double)Math.Abs(Geometry.GeometryUtil.PolygonArea(this));
      }
    }

    [JsonIgnore]
    public double NetArea
    {
      get
      {
        return this.Area - this.Children.Sum(o => o.Area);
      }
    }

    [JsonIgnore]
    /// <inheritdoc />
    public bool IsClosed
    {
      get
      {
        var tolerance = 0.0000001;
        if (this.Points.Length == 0)
        {
          return false;
        }

        if (Math.Abs(this.Points.First().X - this.Points.Last().X) < tolerance &&
            Math.Abs(this.Points.First().Y - this.Points.Last().Y) < tolerance)
        {
          return true;
        }

        IPointXY lastPoint = null;
        foreach (var point in this.Points)
        {
          if (lastPoint != null &&
              Math.Abs(point.X - lastPoint.X) < tolerance &&
              Math.Abs(point.Y - lastPoint.Y) < tolerance)
          {
            return true;
          }

          lastPoint = point;
        }

        return false;
      }
    }

    [JsonIgnore]
    /// <inheritdoc />
    public bool IsExact => !this.Points.Any(o => !o.Exact);

    /// <inheritdoc />
    public bool IsPriority { get; set; }

    /// <inheritdoc />
    public AnglesEnum StrictAngle { get; set; }

    /// <inheritdoc />
    public SvgPoint this[int ind]
    {
      get
      {
        return this.points[ind];
      }
    }

    /// <summary>
    /// Creates a new <see cref="NoFitPolygon"/> from the json supplied.
    /// </summary>
    /// <param name="json">Serialised representation of the NFP to create.</param>
    /// <returns>New <see cref="NoFitPolygon"/>.</returns>
    public static NoFitPolygon FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new NfpJsonConverter());
      return JsonSerializer.Deserialize<NoFitPolygon>(json, options);
    }

    public static NoFitPolygon LoadFromFile(string fileName)
    {
      using (StreamReader inputFile = new StreamReader(fileName))
      {
        return FromJson(inputFile.ReadToEnd());
      }
    }

    /// <inheritdoc />
    public void AddPoint(SvgPoint point)
    {
      int i = this.points.Length;
      Array.Resize(ref this.points, i + 1);
      this.points[i] = point;
    }

    /// <inheritdoc/>
    public void Clean()
    {
      var cleaned = SvgNest.CleanPolygon2(this);
      if (cleaned != null)
      {
        this.ReplacePoints(cleaned.Points);
      }

      foreach (var child in this.Children)
      {
        child.Clean();
      }
    }

    /// <inheritdoc />
    public void Reverse()
    {
      this.points.Reverse();
    }

    public bool Overlaps(INfp other)
    {
      bool result = NfpSimplifier.IsIntersect(this, other, SvgNest.Config.ClipperScale);
      if (result)
      {
        if (other.Children.Count == 0)
        {
          return true;
        }
        else
        {
          foreach (var hole in other.Children)
          {
            if (hole.Children.Count == 0)
            {
              if (NfpSimplifier.IsInnerContainedByOuter(this, hole))
              {
                return false;
              }
            }
          }

          return true;
        }
      }

      return false;
    }

    /// <inheritdoc />
    void IHiddenNfp.Push(SvgPoint svgPoint)
    {
      this.points = this.points.Append(svgPoint).ToArray();
    }

    /// <inheritdoc />
    public void ReplacePoints(IEnumerable<SvgPoint> points)
    {
      this.points = points.ToArray();
    }

    /// <inheritdoc />
    public void ReplacePoints(INfp replacementNfp)
    {
      this.points = replacementNfp.Points.ToArray();
      for (int i = 0; i < this.Children.Count; i++)
      {
        this.Children[i].ReplacePoints(replacementNfp.Children[i]);
      }
    }

    /// <inheritdoc />
    public INfp Slice(int v)
    {
      var ret = new NoFitPolygon();
      List<SvgPoint> pp = new List<SvgPoint>();
      for (int i = v; i < this.Length; i++)
      {
        pp.Add(new SvgPoint(this[i].X, this[i].Y));
      }

      ret.ReplacePoints(pp.ToArray());
      return ret;
    }

    /// <inheritdoc />
    public string Stringify()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override string ToString()
    {
      var pointStr = (this.points != null) ? this.points.Count() + string.Empty : "null";
      return $"{this.GetType().Name}: id:{this.Id}; src:{this.Source}; rot:{this.Rotation}°@{this.X:0.###},{this.Y:0.###}; points:{pointStr} - {this.Name}";
    }

    /// <inheritdoc />
    public string ToShortString()
    {
      return $"{(this.GetType().Name == "NoFitPolygon" ? "NFP" : this.GetType().Name)}: i:{this.Id};s:{this.Source};r:{this.Rotation}°@{this.X:0.###},{this.Y:0.###}";
    }

    /// <inheritdoc />
    public NoFitPolygon Clone()
    {
      NoFitPolygon result = new NoFitPolygon();
      CopyStateProperties(result);
      result.IsPriority = this.IsPriority;
      result.StrictAngle = this.StrictAngle;

      for (var i = 0; i < this.Length; i++)
      {
        result.AddPoint(new SvgPoint(this[i].X, this[i].Y));
      }

      if (this.Children != null && this.Children.Count > 0)
      {
        foreach (var child in this.Children)
        {
          result.Children.Add(child.Clone());
        }
      }

      return result;
    }

    /// <inheritdoc />
    public INfp CloneTop()
    {
      var newp = new NoFitPolygon();
      for (var i = 0; i < this.Length; i++)
      {
        newp.AddPoint(new SvgPoint(
             this[i].X,
             this[i].Y));
      }

      return newp;
    }

    /// <inheritdoc />
    public NoFitPolygon CloneExact()
    {
      NoFitPolygon clone = new NoFitPolygon();
      CopyStateProperties(clone);
      CopyInstructionProperties(clone);

      clone.ReplacePoints(this.Points.Select(z => new SvgPoint(z.X, z.Y) { Exact = z.Exact }));
      if (this.Children != null)
      {
        foreach (var citem in this.Children)
        {
          clone.Children.Add(new NoFitPolygon());
          var l = clone.Children.Last();
          l.Id = citem.Id;
          l.Source = citem.Source;
          l.ReplacePoints(citem.Points.Select(z => new SvgPoint(z.X, z.Y) { Exact = z.Exact }));
        }
      }

      return clone;
    }

    private void CopyInstructionProperties(NoFitPolygon clone)
    {
      clone.IsPriority = this.IsPriority;
      clone.StrictAngle = this.StrictAngle;
    }

    /// <inheritdoc/>
    public INfp Rotate(double degrees, WithChildren withChildren = WithChildren.Included)
    {
      var angle = Geometry.GeometryUtil.ToRadians(degrees);
      List<SvgPoint> pp = new List<SvgPoint>();
      for (var i = 0; i < this.Length; i++)
      {
        var x = this[i].X;
        var y = this[i].Y;
        var x1 = (x * Math.Cos(angle)) - (y * Math.Sin(angle));
        var y1 = (x * Math.Sin(angle)) + (y * Math.Cos(angle));

        pp.Add(new SvgPoint(x1, y1));
      }

      NoFitPolygon rotated = this.Clone();
      rotated.Rotation = 0;
      rotated.ReplacePoints(pp);

      // rotated.Rotation += degrees;

      // rotated.Rotation = rotated.Rotation % 360f;

      if (withChildren == WithChildren.Included && this.Children != null && this.Children.Count > 0)
      {
        for (var j = 0; j < this.Children.Count; j++)
        {
          rotated.Children[j] = this.Children[j].Rotate(degrees, withChildren);
        }
      }

      return rotated;
    }

    /// <inheritdoc />
    public INfp CloneTree()
    {
      INfp result;
      if (this is Sheet sheet)
      {
        result = new Sheet();
        //{
        //  Width = sheet.WidthCalculated,
        //  Height = sheet.HeightCalculated,
        //};
      }
      else
      {
        result = new NoFitPolygon();
      }

      foreach (var t in this.Points)
      {
        result.AddPoint(new SvgPoint(t.X, t.Y) { Exact = t.Exact });
      }

      // jwb added the properties
      // newtree.Id = this.Id; //Id is set unique within the chromosome
      // newtree.Source = this.Source; //Source is set to the original Id cloned to form Adam.
      result.IsPriority = this.IsPriority;
      result.StrictAngle = this.StrictAngle;
      result.Name = this.Name;

      if (this.Children != null && this.Children.Count > 0)
      {
        foreach (var c in this.Children)
        {
          result.Children.Add(c.CloneTree());
        }
      }

      return result;
    }

    /// <inheritdoc />
    public NoFitPolygon GetHull()
    {
      // convert to hulljs format
      /*var hull = new ConvexHullGrahamScan();
      for(var i=0; i<polygon.length; i++){
          hull.addPoint(polygon[i].x, polygon[i].y);
      }

      return hull.getHull();*/
      double[][] points = new double[this.Length][];
      for (var i = 0; i < this.Length; i++)
      {
        points[i] = new double[] { this[i].X, this[i].Y };
      }

      var hullpoints = D3.PolygonHull(points);
      if (hullpoints == null)
      {
        return new NoFitPolygon(this.Points);
      }
      else
      {
        var svgPoints = new SvgPoint[hullpoints.Length];
        for (int i = 0; i < hullpoints.Length; i++)
        {
          svgPoints[i] = new SvgPoint(hullpoints[i][0], hullpoints[i][1]);
        }

        return new NoFitPolygon(svgPoints);
      }
    }

    public Chromosome ToChromosome()
    {
      return new Chromosome(this);
    }

    /// <inheritdoc />
    public virtual string ToJson()
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new NfpJsonConverter());
      options.WriteIndented = true;
      return JsonSerializer.Serialize(this, options);
    }

    /// <inheritdoc />
    public string ToOpenScadPolygon()
    {
      var resultBuilder = new StringBuilder("polygon ([");
      foreach (var p in this.Points)
      {
        resultBuilder.AppendLine($"[{p.X:0.######},{p.Y:0.######}],");
      }

      resultBuilder.AppendLine("]);");

      if (Children.Count > 0)
      {
        var outer = resultBuilder.ToString();
        resultBuilder = new StringBuilder();
        resultBuilder.AppendLine("difference() {");
        resultBuilder.Append(outer);
        foreach (var c in Children)
        {
          resultBuilder.Append(c.ToOpenScadPolygon());
        }

        resultBuilder.AppendLine("}");
      }

      return resultBuilder.ToString();
    }

    /// <inheritdoc />
    public INfp Shift(IPartPlacement shift)
    {
      return Shift(shift.X, shift.Y);
    }

    /// <inheritdoc />
    public INfp Shift(double x, double y)
    {
      NoFitPolygon shifted = new NoFitPolygon();
      CopyStateProperties(shifted);

      shifted.PlacementOrder = this.PlacementOrder;
      shifted.StrictAngle = this.StrictAngle;

      for (var i = 0; i < this.Length; i++)
      {
        shifted.AddPoint(new SvgPoint(this[i].X + x, this[i].Y + y) { Exact = this[i].Exact });
      }

      if (this.Children != null /*&& p.Children.Count*/)
      {
        for (int i = 0; i < this.Children.Count(); i++)
        {
          shifted.Children.Add(this.Children[i].Shift(x, y));
        }
      }

      return shifted;
    }

    private void CopyStateProperties(NoFitPolygon other)
    {
      other.Id = this.Id;
      other.Name = this.Name;
      other.Rotation = this.Rotation;
      other.Source = this.Source;
    }

    /// <inheritdoc />
    public INfp ShiftToOrigin()
    {
      return Shift(-this.MinX, -this.MinY);
    }

    bool IEquatable<IPolygon>.Equals(IPolygon other)
    {
      var thisPolygon = this as IPolygon;
      if (other != null &&
          thisPolygon.Points.SequenceEqual(other.Points, new SvgPointCloseEqualityComparer()) &&
          thisPolygon.Id == other.Id &&
          thisPolygon.Name == other.Name &&
          thisPolygon.Rotation == other.Rotation &&
          thisPolygon.Source == other.Source &&
          thisPolygon.Children.Count == other.Children.Count)
      {
        for (int c = 0; c < thisPolygon.Children.Count; c++)
        {
          var childPolygon = thisPolygon.Children[c] as IEquatable<IPolygon>;
          if (!childPolygon.Equals(other.Children[c]))
          {
            return false;
          }
        }

        return true;
      }

      return false;
    }

    public void EnsureIsClosed()
    {
      if (!this.IsClosed)
      {
        var closedPoints = points.ToList();
        closedPoints.Add(closedPoints.First());
        ReplacePoints(closedPoints);
      }
    }

    internal static INfp FromDxf(DxfPolyline dxfPolyline)
    {
      return FromDxf(new List<DxfEntity>()
      {
        dxfPolyline
      });
    }

    public static INfp FromDxf(List<DxfEntity> dxfEntities)
    {
      IRawDetail raw;
      raw = DxfParser.ConvertDxfToRawDetail(string.Empty, dxfEntities);
      INfp result;
      raw.TryConvertToNfp(0, out result);
      return result;
    }

    public DxfFile ToDxfFile()
    {
      var result = new DxfFile();
      result.Entities.Add(ToDxfPolyLine());
      return result;
    }

    public DxfPolyline ToDxfPolyLine()
    {
      var resultSource = new List<DxfVertex>();
      foreach (var point in points)
      {
        resultSource.Add(new DxfVertex(new IxMilia.Dxf.DxfPoint(point.X, point.Y, 0)));
      }

      return new DxfPolyline(resultSource);
    }
  }
}