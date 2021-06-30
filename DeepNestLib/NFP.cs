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
      return $"nfp: id: {this.Id}; source: {this.Source}; rotation: {this.rotation}; points: {str1}";
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

    public int? Source { get; set; } = null;

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
  }
}
