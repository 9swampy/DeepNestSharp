namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Linq;

  public static class Extensions
  {
    public static double DistTo(this SvgPoint p, SvgPoint p2)
    {
      return Math.Sqrt(Math.Pow(p.X - p2.X, 2) + Math.Pow(p.Y - p2.Y, 2));
    }

    public static double DistTo(this PointF p, PointF p2)
    {
      return Math.Sqrt(Math.Pow(p.X - p2.X, 2) + Math.Pow(p.Y - p2.Y, 2));
    }

    public static T[] Splice<T>(this T[] p, int a, int b)
    {
      List<T> ret = new List<T>();
      for (int i = 0; i < p.Length; i++)
      {
        if (i >= a && i < (a + b))
        {
          continue;
        }

        ret.Add(p[i]);
      }

      return ret.ToArray();
    }

    public static List<List<ClipperLib.IntPoint>> Splice(this List<List<ClipperLib.IntPoint>> p, int a, int b)
    {
      List<List<ClipperLib.IntPoint>> ret = new List<List<ClipperLib.IntPoint>>();
      for (int i = a; i < (a + b); i++)
      {
        if (i >= a && i < (a + b))
        {
          continue;
        }

        ret.Add(p[i]);
      }

      return ret;
    }

    /// <summary>
    /// Skips a then takes the next (up to) b.
    /// </summary>
    /// <param name="p"></param>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static NFP[] Splice(this NFP[] p, int a, int b)
    {
      //return p.Skip(a + 1).Take(b).ToArray();
      List<NFP> ret = new List<NFP>();
      for (int i = 0; i < p.Length; i++)
      {
        if (i >= a && i < (a + b))
        {
          continue;
        }

        ret.Add(p[i]);
      }

      return ret.ToArray();
    }
  }
}
