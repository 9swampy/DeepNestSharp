namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Drawing;
  using System.Linq;
  using System.Runtime.InteropServices;
  using System.Threading;
  using System.Threading.Tasks;
  using ClipperLib;
  using Minkowski;

  public class PmapWorker
  {
    private readonly IList<NfpPair> pairs;
    private readonly IProgressDisplayer progressDisplayer;
    private readonly bool useParallel;
    private int processed = 0;

    public PmapWorker(IList<NfpPair> pairs, IProgressDisplayer progressDisplayer, bool useParallel)
    {
      this.pairs = pairs;
      this.progressDisplayer = progressDisplayer;
      this.useParallel = useParallel;
    }

    private void DisplayProgress()
    {
      Interlocked.Increment(ref this.processed);
      this.progressDisplayer.DisplayProgress((float)this.processed / (float)this.pairs.Count);
    }

    public NfpPair[] PmapDeepNest()
    {
      NfpPair[] ret = new NfpPair[pairs.Count()];
      if (this.useParallel)
      {
        Parallel.For(0, pairs.Count, (i) =>
        {
          ret[i] = this.Process(pairs[i]);
          DisplayProgress();
        });
      }
      else
      {
        for (int i = 0; i < pairs.Count; i++)
        {
          var item = pairs[i];
          ret[i] = this.Process(item);
          DisplayProgress();
        }
      }

      return ret.ToArray();
    }

    private NfpPair Process(NfpPair pair)
    {
      var A = pair.A.Rotate(pair.ARotation);
      var B = pair.B.Rotate(pair.BRotation);

      ///////////////////
      var Ac = _Clipper.ScaleUpPaths(A.Points, 10000000);

      var Bc = _Clipper.ScaleUpPaths(B.Points, 10000000);
      for (var i = 0; i < Bc.Length; i++)
      {
        Bc[i].X *= -1;
        Bc[i].Y *= -1;
      }

      var solution = ClipperLib.Clipper.MinkowskiSum(new List<IntPoint>(Ac), new List<IntPoint>(Bc), true);
      NFP clipperNfp = null;

      double? largestArea = null;
      for (int i = 0; i < solution.Count(); i++)
      {
        var n = solution[i].ToArray().ToNestCoordinates(10000000);
        var sarea = -GeometryUtil.polygonArea(n);
        if (largestArea == null || largestArea < sarea)
        {
          clipperNfp = n;
          largestArea = sarea;
        }
      }

      for (var i = 0; i < clipperNfp.Length; i++)
      {
        clipperNfp[i].x += B[0].x;
        clipperNfp[i].y += B[0].y;
      }

      // return new SvgNestPort.NFP[] { new SvgNestPort.NFP() { Points = clipperNfp.Points } };

      //////////////

      pair.A = null;
      pair.B = null;
      pair.nfp = clipperNfp;
      return pair;
    }
  }
}
