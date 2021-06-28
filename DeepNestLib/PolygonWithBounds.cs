namespace DeepNestLib
{
  using System.Collections.Generic;

  public partial class GeometryUtil
  {
    public class PolygonWithBounds : NFP
    {
      public PolygonWithBounds(IEnumerable<SvgPoint> points)
        : base(points)
      {
      }

      public double Width;
      public double Height;
    }
  }
}
