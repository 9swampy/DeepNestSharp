namespace DeepNestLib
{
  using System.Collections.Generic;
  using System.Linq;

  public static class SvgPointArrayExtensions
  {
    public static SvgPoint[] DeepClone(this IEnumerable<SvgPoint> points)
    {
      List<SvgPoint> result = new List<SvgPoint>(points.Count());
      foreach (var point in points)
      {
        result.Add(point.Clone());
      }

      return result.ToArray();
    }
  }
}
