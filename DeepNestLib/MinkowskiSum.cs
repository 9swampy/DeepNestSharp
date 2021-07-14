namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Linq;
  using System.Threading;
  using ClipperLib;
  using Minkowski;

  public static class MinkowskiSum
  {
    private static volatile object minkowskiSyncLock = new object();
    private static MinkowskiDictionary minkowskiCache = new MinkowskiDictionary();

    private static int callCounter = 0;

    internal static int CallCounter
    {
      get
      {
        return callCounter;
      }
    }

    internal static INfp DllImportExecute(INfp a, INfp b, MinkowskiSumCleaning minkowskiSumCleaning = MinkowskiSumCleaning.None)
    {
      Dictionary<string, List<PointF>> dic1 = new Dictionary<string, List<PointF>>();
      Dictionary<string, List<double>> dic2 = new Dictionary<string, List<double>>();
      dic2.Add("A", new List<double>());
      foreach (var item in a.Points)
      {
        var target = dic2["A"];
        target.Add(item.x);
        target.Add(item.y);
      }

      dic2.Add("B", new List<double>());
      foreach (var item in b.Points)
      {
        var target = dic2["B"];
        target.Add(item.x);
        target.Add(item.y);
      }

      List<double> hdat = new List<double>();

      foreach (var item in a.Children)
      {
        foreach (var pitem in item.Points)
        {
          hdat.Add(pitem.x);
          hdat.Add(pitem.y);
        }
      }

      var aa = dic2["A"];
      var bb = dic2["B"];
      var arr1 = a.Children.Select(z => z.Points.Count() * 2).ToArray();

      var key = new MinkowskiKey(aa.Count, aa.ToArray(), a.Children.Count, arr1, hdat.ToArray(), bb.Count, bb.ToArray());
      INfp ret;
      if (!minkowskiCache.TryGetValue(key, out ret))
      {
        lock (minkowskiSyncLock)
        {
#if x64
          // System.Diagnostics.Debug.Print($"{callCounter}.Minkowski_x64");
          long[] longs = arr1.Select(o => (long)o).ToArray();
          MinkowskiWrapper.setData(key.ALength, key.APoints, key.AChildrenLength, longs, key.Hdat, key.BLength, key.BPoints);
#else
          // System.Diagnostics.Debug.Print($"{callCounter}.Minkowski_x86/AnyCpu");
          MinkowskiWrapper.setData(key.ALength, key.APoints, key.AChildrenLength, arr1, key.Hdat, key.BLength, key.BPoints);
#endif
          MinkowskiWrapper.calculateNFP();

          Interlocked.Increment(ref callCounter);

          int[] sizes;
          int[] sizes1;
          int[] sizes2;
          double[] dat1;
          double[] hdat1;
          sizes = new int[2];
          MinkowskiWrapper.getSizes1(sizes);
          sizes1 = new int[sizes[0]];
          sizes2 = new int[sizes[1]];
          MinkowskiWrapper.getSizes2(sizes1, sizes2);
          dat1 = new double[sizes1.Sum()];
          hdat1 = new double[sizes2.Sum()];

          MinkowskiWrapper.getResults(dat1, hdat1);

          if (sizes1.Count() > 1)
          {
            throw new ArgumentException("sizes1 cnt >1");
          }

          // convert back to answer here
          bool isa = true;
          List<PointF> apts = new List<PointF>();

          List<List<double>> holesval = new List<List<double>>();
          bool holes = false;

          for (int i = 0; i < dat1.Length; i += 2)
          {
            var x1 = (float)dat1[i];
            var y1 = (float)dat1[i + 1];
            apts.Add(new PointF(x1, y1));
          }

          int index = 0;
          for (int i = 0; i < sizes2.Length; i++)
          {
            holesval.Add(new List<double>());
            for (int j = 0; j < sizes2[i]; j++)
            {
              holesval.Last().Add(hdat1[index]);
              index++;
            }
          }

          List<List<PointF>> holesout = new List<List<PointF>>();
          foreach (var item in holesval)
          {
            holesout.Add(new List<PointF>());
            for (int i = 0; i < item.Count; i += 2)
            {
              var x = (float)item[i];
              var y = (float)item[i + 1];
              holesout.Last().Add(new PointF(x, y));
            }
          }

          ret = new NFP();
          foreach (var item in apts)
          {
            ret.AddPoint(new SvgPoint(item.X, item.Y));
          }

          foreach (var item in holesout)
          {
            ret.Children.Add(new NFP());
            foreach (var hitem in item)
            {
              ret.Children.Last().AddPoint(new SvgPoint(hitem.X, hitem.Y));
            }
          }
        }

        minkowskiCache.Add(key, ret);
      }

      if (minkowskiSumCleaning == MinkowskiSumCleaning.Cleaned)
      {
        ret = SvgNest.cleanPolygon2(ret);
      }

      return ret;
    }

    public static NFP ClipperExecute(INfp a, INfp b, MinkowskiSumPick minkowskiSumPick)
    {
      return ClipperExecute(a.Points, b.Points, minkowskiSumPick);
    }

    public static NFP ClipperExecute(SvgPoint[] a, SvgPoint[] b, MinkowskiSumPick minkowskiSumPick)
    {
      var ac = DeepNestClipper.ScaleUpPaths(a, 10000000);
      var bc = DeepNestClipper.ScaleUpPaths(b, 10000000);
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
