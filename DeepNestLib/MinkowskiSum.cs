﻿namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Linq;
  using ClipperLib;
  using Minkowski;

  public class MinkowskiSum : IMinkowskiSumService
  {
    private static volatile object minkowskiSyncLock = new object();
    private MinkowskiDictionary minkowskiCache = new MinkowskiDictionary();

    private readonly INestStateMinkowski state;

    /// <summary>
    /// Initializes a new instance of the <see cref="MinkowskiSum"/> class.
    /// Private because sharing/reusing the cache is dangerous. 
    /// Replacing static global dependencies with factories to facilitate Unit Tests.
    /// </summary>
    private MinkowskiSum(INestStateMinkowski state)
    {
      this.state = state;
    }

    /// <summary>
    /// Create a new instance with a self contained cache.
    /// </summary>
    /// <param name="nestState">Shared NestState (instead of NestState.Default).</param>
    /// <returns></returns>
    public static IMinkowskiSumService CreateInstance(INestStateMinkowski nestState) => new MinkowskiSum(nestState);

    INfp IMinkowskiSumService.DllImportExecute(INfp a, INfp b, MinkowskiSumCleaning minkowskiSumCleaning)
    {
      Dictionary<string, List<PointF>> dic1 = new Dictionary<string, List<PointF>>();
      Dictionary<string, List<double>> dic2 = new Dictionary<string, List<double>>();
      dic2.Add("A", new List<double>());
      foreach (var item in a.Points)
      {
        var target = dic2["A"];
        target.Add(item.X);
        target.Add(item.Y);
      }

      dic2.Add("B", new List<double>());
      foreach (var item in b.Points)
      {
        var target = dic2["B"];
        target.Add(item.X);
        target.Add(item.Y);
      }

      List<double> hdat = new List<double>();

      foreach (var item in a.Children)
      {
        foreach (var pitem in item.Points)
        {
          hdat.Add(pitem.X);
          hdat.Add(pitem.Y);
        }
      }

      var aa = dic2["A"];
      var bb = dic2["B"];
      var arr1 = a.Children.Select(z => z.Points.Count() * 2).ToArray();

      var key = new MinkowskiKey(aa.Count, aa.ToArray(), a.Children.Count, arr1, hdat.ToArray(), bb.Count, bb.ToArray());
      INfp ret;
      lock (minkowskiSyncLock)
      {
        if (!minkowskiCache.TryGetValue(key, out ret))
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

          state.IncrementCallCounter();

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

          minkowskiCache.Add(key, ret);
        }
      }

      if (minkowskiSumCleaning == MinkowskiSumCleaning.Cleaned)
      {
        ret = SvgNest.CleanPolygon2(ret);
      }

      return ret;
    }

    NFP IMinkowskiSumService.ClipperExecute(INfp a, INfp b, MinkowskiSumPick minkowskiSumPick)
    {
      return ((IMinkowskiSumService)this).ClipperExecute(a.Points, b.Points, minkowskiSumPick);
    }

    NFP IMinkowskiSumService.ClipperExecute(SvgPoint[] a, SvgPoint[] b, MinkowskiSumPick minkowskiSumPick)
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
        clipperNfp[i].X += b[0].X;
        clipperNfp[i].Y += b[0].Y;
      }

      return clipperNfp;
    }
  }
}
