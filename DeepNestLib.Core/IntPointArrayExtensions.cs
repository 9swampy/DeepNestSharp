namespace DeepNestLib
{
  using System.Collections.Generic;
  using System.Linq;
  using ClipperLib;

  public static class IntPointArrayExtensions
  {
    public static NoFitPolygon ToNestCoordinates(this IntPoint[] polygon, double scale)
    {
      var clone = new List<SvgPoint>();

      for (var i = 0; i < polygon.Count(); i++)
      {
        clone.Add(new SvgPoint(
             polygon[i].X / scale,
             polygon[i].Y / scale));
      }

      return new NoFitPolygon(clone);
    }

    public static IntPoint[] ReversePath(this IntPoint[] path)
    {
      for (var i = 0; i < path.Length; i++)
      {
        path[i].X *= -1;
        path[i].Y *= -1;
      }

      return path;
    }

    //public static List<IntPoint> ReversePath(this List<IntPoint> path)
    //{
    //  var result = new List<IntPoint>(path.Count());
    //  foreach (var point in path)
    //  {
    //    point.X *= -1;
    //    point.Y *= -1;
    //    result.Add(point);
    //  }

    //  return path;
    //}
  }
}
