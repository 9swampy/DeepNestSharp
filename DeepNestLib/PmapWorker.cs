namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Threading;
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
      if (state.AveragePlacementTime == 0 || state.AveragePlacementTime >= 2000)
      {
        progressDisplayer.InitialiseLoopProgress(ProgressBar.Secondary, "Pmap. . .", pairs.Count);
      }

      NfpPair[] ret = new NfpPair[pairs.Count];
      if (this.useParallel)
      {
        Parallel.For(0, pairs.Count, (i) =>
        {
          ret[i] = this.Process(pairs[i]);
        });
      }
      else
      {
        for (int i = 0; i < pairs.Count; i++)
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
      var a = pair.A.Rotate(pair.ARotation);
      var b = pair.B.Rotate(pair.BRotation);

      NFP clipperNfp;
      lock (nfpPairCacheSyncLock)
      {
        if (!NfpPairCache.TryGetValue(a.Points, b.Points, pair.ARotation, pair.BRotation, pair.Asource, pair.Bsource, MinkowskiSumPick.Largest, out clipperNfp))
        {
          clipperNfp = minkoskiSumService.ClipperExecute(a.Points, b.Points, MinkowskiSumPick.Largest);
          NfpPairCache.Add(a.Points, b.Points, pair.ARotation, pair.BRotation, pair.Asource, pair.Bsource, MinkowskiSumPick.Largest, clipperNfp);
        }
      }

      pair.A = null;
      pair.B = null;
      pair.nfp = clipperNfp;
      progressDisplayer.IncrementLoopProgress(ProgressBar.Secondary);
      return pair;
    }
  }
}
