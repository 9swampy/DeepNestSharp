namespace DeepNestLib.Geometry
{
  using System.Collections.Generic;

  public class PolygonWithBounds : NFP
  {
    public PolygonWithBounds(IEnumerable<SvgPoint> points)
      : base(points)
    {
    }

    public double Width { get; set; }

    public double Height { get; set; }
  }
}
