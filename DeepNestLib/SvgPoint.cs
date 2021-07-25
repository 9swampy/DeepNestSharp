namespace DeepNestLib
{
  using System;
  using System.Text.Json.Serialization;

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
      return "x: " + X + "; y: " + Y;
    }

    public SvgPoint(double x, double y)
    {
      this.X = x;
      this.Y = y;
    }

    internal SvgPoint(SvgPoint point)
    {
      this.Exact = point.Exact;
      this.Marked = point.Marked;
      this.X = point.X;
      this.Y = point.Y;
    }

    public bool Marked { get; set; }

    [JsonConverter(typeof(DoublePrecisionConverter))]
    public double X { get; internal set; }

    [JsonConverter(typeof(DoublePrecisionConverter))]
    public double Y { get; internal set; }

    public SvgPoint Clone()
    {
      return new SvgPoint(this);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(Exact, Marked, X, Y);
    }

    public bool Equals(SvgPoint other)
    {
      return this.GetHashCode() == other.GetHashCode();
    }
  }
}
