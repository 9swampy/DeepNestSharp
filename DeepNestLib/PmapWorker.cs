﻿namespace DeepNestLib
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
      this.progressDisplayer.DisplayProgress(Math.Min(1F, (float)this.processed / (float)this.pairs.Count));
    }

    public NfpPair[] PmapDeepNest()
    {
      NfpPair[] ret = new NfpPair[pairs.Count()];
      // DisplayProgress();
      if (this.useParallel)
      {
        Parallel.For(0, pairs.Count, (i) =>
        {
          ret[i] = this.Process(pairs[i]);
          // DisplayProgress();
        });
      }
      else
      {
        for (int i = 0; i < pairs.Count; i++)
        {
          var item = pairs[i];
          ret[i] = this.Process(item);
          // DisplayProgress();
        }
      }

      return ret.ToArray();
    }

    private NfpPair Process(NfpPair pair)
    {
      var A = pair.A.Rotate(pair.ARotation);
      var B = pair.B.Rotate(pair.BRotation);
      NFP clipperNfp = DeepNestClipper.MinkowskiSum(A, B, MinkowskiSumPick.Largest);

      pair.A = null;
      pair.B = null;
      pair.nfp = clipperNfp;
      return pair;
    }
  }
}
