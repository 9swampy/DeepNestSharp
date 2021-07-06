namespace DeepNestLib
{
  using System;
  using System.Threading;

  public class SvgPoint : IEquatable<SvgPoint>
  {
    public bool exact = true;

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
      this.exact = point.exact;
      this.marked = point.marked;
      this.x = point.x;
      this.y = point.y;
    }

    public bool marked { get; set; }

    public double x { get; internal set; }

    public double y { get; internal set; }

    public SvgPoint Clone()
    {
      return new SvgPoint(this);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(exact, marked, x, y);
    }

    public bool Equals(SvgPoint other)
    {
      return this.GetHashCode() == other.GetHashCode();
    }
  }
}
