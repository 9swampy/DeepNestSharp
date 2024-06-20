namespace DeepNestLib
{
  using System;
  using System.Text.Json.Serialization;

  public class SvgPoint : IEquatable<SvgPoint>, IPointXY
  {
    public bool Exact
    {
      get;
      set;
    }

      = true;

    public override string ToString()
    {
      return "x: " + this.X + "; y: " + this.Y;
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

    double IPointXY.X => this.X;

    double IPointXY.Y => this.Y;

    [JsonConverter(typeof(DoublePrecisionConverter))]
    public double Y { get; internal set; }

    public SvgPoint Clone()
    {
      return new SvgPoint(this);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(this.Exact, this.Marked, Math.Round(this.X, 4), Math.Round(this.Y, 4));
    }

    public bool Equals(SvgPoint other)
    {
      return this.GetHashCode() == other.GetHashCode();
    }
  }
}