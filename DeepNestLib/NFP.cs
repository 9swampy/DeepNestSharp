namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text.Json;

  public class NFP : PolygonBase, INfp, IHiddenNfp, IStringify
  {
    public bool Fitted
    {
      get { return this.Sheet != null; }
    }

    public INfp Sheet { get; set; }

    public override string ToString()
    {
      var str1 = (this.points != null) ? this.points.Count() + string.Empty : "null";
      return $"nfp: id: {this.Id}; source: {this.Source}; rotation: {this.Rotation}; points: {str1}";
    }

    public NFP(IList<INfp> children)
        : this()
    {
      Children = children;
    }

    public NFP()
      : base(new SvgPoint[0])
    {
      this.points = new SvgPoint[0];
    }

    public NFP(IEnumerable<SvgPoint> points)
      : base(points.DeepClone())
    {
    }

    public string Name { get; set; } = string.Empty;

    public void AddPoint(SvgPoint point)
    {
      int i = this.points.Length;
      Array.Resize(ref this.points, i + 1);
      this.points[i] = point;
    }

    public bool isBin;

    public void reverse()
    {
      this.points.Reverse();
    }

    public double x { get; set; }

    public double y { get; set; }

    public double WidthCalculated
    {
      get
      {
        if (this.points.Length == 0)
        {
          return 0;
        }

        var maxx = this.points.Max(z => z.x);
        var minx = this.points.Min(z => z.x);

        return maxx - minx;
      }
    }

    public double HeightCalculated
    {
      get
      {
        if (this.points.Length == 0)
        {
          return 0;
        }

        var maxy = this.points.Max(z => z.y);
        var miny = this.points.Min(z => z.y);
        return maxy - miny;
      }
    }

    public SvgPoint this[int ind]
    {
      get
      {
        return this.points[ind];
      }
    }

    public IList<INfp> Children { get; set; } = new List<INfp>();

    public int Length
    {
      get
      {
        return this.points.Length;
      }
    }

    public int Id { get; set; }

    public double? Offsetx { get; set; }

    public double? Offsety { get; set; }

    public int Source { get; set; } = -1;

    public int PlacementOrder { get; set; } = -1;

    private float rotation;

    public float Rotation
    {
      get
      {
        return this.rotation;
      }

      set
      {
        this.rotation = value;
      }
    }

    public bool ForceRotations { get; set; }

    public float[] Rotations { get; } = new float[] { 90, 270 };

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

    public SvgPoint[] ReplacePoints(IEnumerable<SvgPoint> points)
    {
      this.points = points.ToArray();
      return this.Points;
    }

    public float Area
    {
      get
      {
        float ret = 0;
        if (this.points.Length < 3)
        {
          return 0;
        }

        List<SvgPoint> pp = new List<SvgPoint>();
        pp.AddRange(this.points);
        pp.Add(this.points[0]);
        for (int i = 1; i < pp.Count; i++)
        {
          var s0 = pp[i - 1];
          var s1 = pp[i];
          ret += (float)((s0.x * s1.y) - (s0.y * s1.x));
        }

        return (float)Math.Abs(ret / 2);
      }
    }

    public bool IsPriority { get; set; }

    public AnglesEnum StrictAngle { get; set; }

    void IHiddenNfp.Push(SvgPoint svgPoint)
    {
      this.points = this.points.Append(svgPoint).ToArray();
    }

    public INfp slice(int v)
    {
      var ret = new NFP();
      List<SvgPoint> pp = new List<SvgPoint>();
      for (int i = v; i < this.Length; i++)
      {
        pp.Add(new SvgPoint(this[i].x, this[i].y));
      }

      ret.ReplacePoints(pp.ToArray());
      return ret;
    }

    public string stringify()
    {
      throw new NotImplementedException();
    }

    public NFP Clone()
    {
      NFP result = new NFP();
      result.Id = this.Id;
      result.Source = this.Source;
      result.Rotation = this.Rotation;
      result.IsPriority = this.IsPriority;
      result.StrictAngle = this.StrictAngle;
      result.Name = this.Name;

      for (var i = 0; i < this.Length; i++)
      {
        result.AddPoint(new SvgPoint(this[i].x, this[i].y));
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

    /// <summary>
    /// Clone but only copy exact points.
    /// </summary>
    /// <returns>Clone but only copy exact points.</returns>
    public NFP CloneExact()
    {
      NFP clone = new NFP();
      clone.Id = this.Id;
      clone.Source = this.Source;
      clone.IsPriority = this.IsPriority;
      clone.StrictAngle = this.StrictAngle;
      clone.Name = this.Name;
      clone.ReplacePoints(this.Points.Select(z => new SvgPoint(z.x, z.y) { Exact = z.Exact }));
      if (this.Children != null)
      {
        foreach (var citem in this.Children)
        {
          clone.Children.Add(new NFP());
          var l = clone.Children.Last();
          l.Id = citem.Id;
          l.Source = citem.Source;
          l.ReplacePoints(citem.Points.Select(z => new SvgPoint(z.x, z.y) { Exact = z.Exact }));
        }
      }

      return clone;
    }

    public NFP Rotate(float degrees)
    {
      var angle = degrees * Math.PI / 180;
      List<SvgPoint> pp = new List<SvgPoint>();
      for (var i = 0; i < this.Length; i++)
      {
        var x = this[i].x;
        var y = this[i].y;
        var x1 = (x * Math.Cos(angle)) - (y * Math.Sin(angle));
        var y1 = (x * Math.Sin(angle)) + (y * Math.Cos(angle));

        pp.Add(new SvgPoint(x1, y1));
      }

      NFP rotated = this.Clone();
      rotated.Rotation = 0;
      rotated.ReplacePoints(pp);
      // rotated.Rotation += degrees;
      // rotated.Rotation = rotated.Rotation % 360f;

      if (this.Children != null && this.Children.Count > 0)
      {
        for (var j = 0; j < this.Children.Count; j++)
        {
          rotated.Children.Add(this.Children[j].Rotate(degrees));
        }
      }

      return rotated;
    }

    public NFP CloneTree()
    {
      NFP newtree = new NFP();
      foreach (var t in this.Points)
      {
        newtree.AddPoint(new SvgPoint(t.x, t.y) { Exact = t.Exact });
      }

      // jwb added the properties
      // newtree.Id = this.Id; //Id is set unique within the chromosome
      // newtree.Source = this.Source; //Source is set to the original Id cloned to form Adam.
      newtree.IsPriority = this.IsPriority;
      newtree.StrictAngle = this.StrictAngle;
      newtree.Name = this.Name;
      // jwb added the properties
      if (this.Children != null && this.Children.Count > 0)
      {
        foreach (var c in this.Children)
        {
          newtree.Children.Add(c.CloneTree());
        }
      }

      return newtree;
    }

    public NFP GetHull()
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
        points[i] = new double[] { this[i].x, this[i].y };
      }

      var hullpoints = D3.polygonHull(points);
      if (hullpoints == null)
      {
        return new NFP(this.Points);
      }
      else
      {
        var svgPoints = new SvgPoint[hullpoints.Length];
        for (int i = 0; i < hullpoints.Length; i++)
        {
          svgPoints[i] = new SvgPoint(hullpoints[i][0], hullpoints[i][1]);
        }

        return new NFP(svgPoints);
      }
    }

    public string ToJson()
    {
      return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }

    public static NFP FromJson(string json)
    {
      return JsonSerializer.Deserialize<NFP>(json);
    }
  }
}
