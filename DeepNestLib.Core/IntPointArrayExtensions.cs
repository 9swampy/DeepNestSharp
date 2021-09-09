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
  }
}
