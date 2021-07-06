namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using ClipperLib;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Placement;

  public enum MinkowskiSumPick
  {
    Smallest,
    Largest
  }

  public class DeepNestClipper : IDeprecatedClipper
  {
    ClipperLib.IntPoint[] IDeprecatedClipper.ScaleUpPathsOriginal(NFP p, double scale)
    {
      List<ClipperLib.IntPoint> ret = new List<ClipperLib.IntPoint>();

      for (int i = 0; i < p.Points.Length; i++)
      {
        // p.Points[i] = new SvgNestPort.SvgPoint((float)Math.Round(p.Points[i].x * scale), (float)Math.Round(p.Points[i].y * scale));
        ret.Add(new ClipperLib.IntPoint(
            (long)Math.Round((decimal)p.Points[i].x * (decimal)scale),
            (long)Math.Round((decimal)p.Points[i].y * (decimal)scale)));
      }

      return ret.ToArray();
    } // 5 secs

    ClipperLib.IntPoint[] IDeprecatedClipper.ScaleUpPathsSlowerParallel(SvgPoint[] points, double scale)
    {
      var result = from point in points.AsParallel().AsSequential()
                   select new ClipperLib.IntPoint((long)Math.Round((decimal)point.x * (decimal)scale), (long)Math.Round((decimal)point.y * (decimal)scale));

      return result.ToArray();
    } // 2 secs

    public static ClipperLib.IntPoint[] ScaleUpPaths(SvgPoint[] points, double scale = 1)
    {
      var result = new ClipperLib.IntPoint[points.Length];

      Parallel.For(0, points.Length, i => result[i] = new ClipperLib.IntPoint((long)Math.Round((decimal)points[i].x * (decimal)scale), (long)Math.Round((decimal)points[i].y * (decimal)scale)));

      return result.ToArray();
    } // 2 secs

    /*public static IntPoint[] ScaleUpPath(IntPoint[] p, double scale = 1)
    {
        for (int i = 0; i < p.Length; i++)
        {

            //p[i] = new IntPoint(p[i].X * scale, p[i].Y * scale);
            p[i] = new IntPoint(
                (long)Math.Round((decimal)p[i].X * (decimal)scale),
                (long)Math.Round((decimal)p[i].Y * (decimal)scale));
        }
        return p.ToArray();
    }
    public static void ScaleUpPaths(List<List<IntPoint>> p, double scale = 1)
    {
        for (int i = 0; i < p.Count; i++)
        {
            for (int j = 0; j < p[i].Count; j++)
            {
                p[i][j] = new IntPoint(p[i][j].X * scale, p[i][j].Y * scale);

            }
        }


    }*/

    public static NFP MinkowskiSum(NFP a, NFP b, MinkowskiSumPick minkowskiSumPick)
    {
      var ac = ScaleUpPaths(a.Points, 10000000);
      var bc = ScaleUpPaths(b.Points, 10000000);
      for (var i = 0; i < bc.Length; i++)
      {
        bc[i].X *= -1;
        bc[i].Y *= -1;
      }

      var solution = ClipperLib.Clipper.MinkowskiSum(new List<IntPoint>(ac), new List<IntPoint>(bc), true);
      NFP clipperNfp = null;

      double? largestArea = null;
      for (int i = 0; i < solution.Count(); i++)
      {
        var n = solution[i].ToArray().ToNestCoordinates(10000000);
        var sarea = -GeometryUtil.polygonArea(n);
        if (largestArea == null ||
            (minkowskiSumPick == MinkowskiSumPick.Largest && largestArea < sarea) ||
            (minkowskiSumPick == MinkowskiSumPick.Smallest && largestArea > sarea))
        {
          clipperNfp = n;
          largestArea = sarea;
        }
      }

      for (var i = 0; i < clipperNfp.Length; i++)
      {
        clipperNfp[i].x += b[0].x;
        clipperNfp[i].y += b[0].y;
      }

      return clipperNfp;
    }
  }
}
