namespace DeepNestLib
{
  using System;

  public class SvgPoint : IEquatable<SvgPoint>
  {
    public bool Exact
    {
      get;
      set;
    }
      = true;

    public override string ToString()
    {
      return "x: " + x + "; y: " + y;
    }

    public SvgPoint(double x, double y)
    {
      this.x = x;
      this.y = y;
    }

    internal SvgPoint(SvgPoint point)
    {
      this.Exact = point.Exact;
      this.Marked = point.Marked;
      this.x = point.x;
      this.y = point.y;
    }

    public bool Marked { get; set; }

    public double x { get; internal set; }

    public double y { get; internal set; }

    public SvgPoint Clone()
    {
      return new SvgPoint(this);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(Exact, Marked, x, y);
    }

    public bool Equals(SvgPoint other)
    {
      return this.GetHashCode() == other.GetHashCode();
    }
  }
}
