namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Drawing;
  using System.Linq;
  using System.Runtime.InteropServices;
  using System.Threading.Tasks;
  using ClipperLib;
  using Minkowski;

  public static class IntPointArrayExtensions
  {
    public static NFP ToNestCoordinates(this IntPoint[] polygon, double scale)
    {
      var clone = new List<SvgPoint>();

      for (var i = 0; i < polygon.Count(); i++)
      {
        clone.Add(new SvgPoint(
             polygon[i].X / scale,
             polygon[i].Y / scale));
      }

      return new NFP(clone);
    }
  }
}
