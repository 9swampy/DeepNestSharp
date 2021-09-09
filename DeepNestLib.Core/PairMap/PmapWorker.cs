﻿namespace DeepNestLib.PairMap
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  public class PmapWorker
  {
    private static readonly NfpPairDictionary NfpPairCache = new NfpPairDictionary();
    private static volatile object nfpPairCacheSyncLock = new object();

    private readonly IList<NfpPair> pairs;
    private readonly IProgressDisplayer progressDisplayer;
    private readonly bool useParallel;
    private readonly IMinkowskiSumService minkoskiSumService;
    private readonly INestStateBackground state;

    public PmapWorker(IList<NfpPair> pairs, IProgressDisplayer progressDisplayer, bool useParallel, IMinkowskiSumService minkoskiSumService, INestStateBackground state)
    {
      this.pairs = pairs;
      this.progressDisplayer = progressDisplayer;
      this.useParallel = useParallel;
      this.minkoskiSumService = minkoskiSumService;
      this.state = state;
    }

    public NfpPair[] PmapDeepNest()
    {
      progressDisplayer.InitialiseLoopProgress(ProgressBar.Secondary, "Pmap. . .", pairs.Count);
      NfpPair[] ret = new NfpPair[pairs.Count];
      if (this.useParallel)
      {
        Parallel.For(0, this.pairs.Count, (i) =>
        {
          var item = pairs[i];
          ProcessAndCaptureResult(item, result => ret[i] = result);
        });
      }
      else
      {
        for (var i = 0; i < pairs.Count; i++)
        {
          var item = pairs[i];
          ret[i] = this.Process(item);
        }
      }

      state.SetNfpPairCachePercentCached(NfpPairCache.PercentCached);
      progressDisplayer.SetIsVisibleSecondaryProgressBar(false);
      return ret.ToArray();
    }

    internal NfpPair Process(NfpPair pair)
    {
      var pattern = pair.A.Rotate(pair.ARotation, WithChildren.Excluded);
      var path = pair.B.Rotate(pair.BRotation, WithChildren.Excluded);

      NoFitPolygon clipperNfp;
      lock (nfpPairCacheSyncLock)
      {
        if (!NfpPairCache.TryGetValue(pattern.Points, path.Points, pair.ARotation, pair.BRotation, pair.Asource, pair.Bsource, MinkowskiSumPick.Largest, out clipperNfp))
        {
          clipperNfp = minkoskiSumService.ClipperExecuteOuterNfp(pattern.Points, path.Points, MinkowskiSumPick.Largest);
          NfpPairCache.Add(pattern.Points, path.Points, pair.ARotation, pair.BRotation, pair.Asource, pair.Bsource, MinkowskiSumPick.Largest, clipperNfp);
        }
      }

      pair.A = null;
      pair.B = null;
      pair.Nfp = clipperNfp;
      progressDisplayer.IncrementLoopProgress(ProgressBar.Secondary);
      return pair;
    }

    private void ProcessAndCaptureResult(NfpPair item, Action<NfpPair> captureResultAction)
    {
      captureResultAction(this.Process(item));
    }
  }
}