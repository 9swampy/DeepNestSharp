namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class NFP : INfp, IStringify
  {
    private SvgPoint[] points;

    public bool fitted
    {
      get { return this.Sheet != null; }
    }

    public NFP Sheet;

    public override string ToString()
    {
      var str1 = (this.points != null) ? this.points.Count() + string.Empty : "null";
      return $"nfp: id: {this.Id}; source: {this.Source}; rotation: {this.Rotation}; points: {str1}";
    }

    public NFP(IList<NFP> children)
        : this()
    {
      Children = children;
    }

    public NFP()
    {
      this.points = new SvgPoint[0];
    }
        
    public NFP(IEnumerable<SvgPoint> points)
    {
      this.points = points.DeepClone();
    }

    public string Name { get; set; }

    public void AddPoint(SvgPoint point)
    {
      this.points = this.points.Append(point).ToArray();
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
        var maxx = this.points.Max(z => z.x);
        var minx = this.points.Min(z => z.x);

        return maxx - minx;
      }
    }

    public double HeightCalculated
    {
      get
      {
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

    public IList<NFP> Children { get; set; } = new List<NFP>();

    public int Length
    {
      get
      {
        return this.points.Length;
      }
    }

    public int Id { get; set; }

    public double? Offsetx;
    public double? Offsety;

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

    public bool IsPrimary { get; set; }

    internal void Push(SvgPoint svgPoint)
    {
      this.points = this.points.Append(svgPoint).ToArray();
    }

    public NFP slice(int v)
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
      result.IsPrimary = this.IsPrimary;

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
      clone.IsPrimary = this.IsPrimary;
      clone.ReplacePoints(this.Points.Select(z => new SvgPoint(z.x, z.y) { exact = z.exact }));
      if (this.Children != null)
      {
        foreach (var citem in this.Children)
        {
          clone.Children.Add(new NFP());
          var l = clone.Children.Last();
          l.Id = citem.Id;
          l.Source = citem.Source;
          l.ReplacePoints(citem.Points.Select(z => new SvgPoint(z.x, z.y) { exact = z.exact }));
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
        newtree.AddPoint(new SvgPoint(t.x, t.y) { exact = t.exact });
      }

      // jwb added the properties
      // newtree.Id = this.Id;
      // newtree.Source = this.Source;
      newtree.IsPrimary = this.IsPrimary;

      // newtree.Name = this.Name;
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
  }
}
