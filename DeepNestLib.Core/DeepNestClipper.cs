namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  public class DeepNestClipper : IDeprecatedClipper
  {
    List<ClipperLib.IntPoint> IDeprecatedClipper.ScaleUpPathsOriginal(NoFitPolygon p, double scale)
    {
      List<ClipperLib.IntPoint> ret = new List<ClipperLib.IntPoint>();

      for (int i = 0; i < p.Points.Length; i++)
      {
        ret.Add(new ClipperLib.IntPoint(
            (long)Math.Round((decimal)p.Points[i].X * (decimal)scale),
            (long)Math.Round((decimal)p.Points[i].Y * (decimal)scale)));
      }

      return ret;
    } // 5 secs

    ClipperLib.IntPoint[] IDeprecatedClipper.ScaleUpPathsSlowerParallel(SvgPoint[] points, double scale)
    {
      var result = from point in points.AsParallel().AsSequential()
                   select new ClipperLib.IntPoint((long)Math.Round((decimal)point.X * (decimal)scale), (long)Math.Round((decimal)point.Y * (decimal)scale));

      return result.ToArray();
    } // 2 secs

    public static List<ClipperLib.IntPoint> ScaleUpPath(IList<SvgPoint> points, double scale)
    {
      var result = new ClipperLib.IntPoint[points.Count];

      Parallel.For(0, points.Count, i => result[i] = new ClipperLib.IntPoint((long)Math.Round((decimal)points[i].X * (decimal)scale), (long)Math.Round((decimal)points[i].Y * (decimal)scale)));

      return result.ToList();
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
  }
}
