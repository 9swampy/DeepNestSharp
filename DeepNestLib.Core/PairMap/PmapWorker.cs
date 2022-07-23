namespace DeepNestLib.PairMap
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  public class PmapWorker
  {
    private const bool UseNfpPairCache = true;
    private static readonly NfpPairDictionary NfpPairCache = new NfpPairDictionary();
    private static volatile object nfpPairCacheSyncLock = new object();

    private readonly IList<NfpPair> pairs;
    private readonly IProgressDisplayer progressDisplayer;
    private readonly bool useParallel;
    private readonly IMinkowskiSumService minkoskiSumService;
    private readonly INestStateBackground state;
    private bool showSecondaryProgress = false;

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
      if (NfpPairCache.Count == 0)
      {
        progressDisplayer.InitialiseLoopProgress(ProgressBar.Secondary, "Pmap. . .", pairs.Count);
        showSecondaryProgress = true;
      }
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

      if (UseNfpPairCache)
      {
        state.SetNfpPairCachePercentCached(NfpPairCache.PercentCached);
      }

      if (showSecondaryProgress)
      {
        progressDisplayer.IsVisibleSecondaryProgressBar = false;
      }

      return ret.ToArray();
    }

    internal NfpPair Process(NfpPair pair)
    {
      var pattern = pair.A.Rotate(pair.ARotation, WithChildren.Excluded);
      var path = pair.B.Rotate(pair.BRotation, WithChildren.Excluded);

      NoFitPolygon clipperNfp;
      if (UseNfpPairCache)
      {
        lock (nfpPairCacheSyncLock)
        {
          if (!NfpPairCache.TryGetValue(pattern.Points, path.Points, pair.ARotation, pair.BRotation, pair.Asource, pair.Bsource, MinkowskiSumPick.Largest, out clipperNfp))
          {
            clipperNfp = minkoskiSumService.ClipperExecuteOuterNfp(pattern.Points, path.Points, MinkowskiSumPick.Largest);
            NfpPairCache.Add(pattern.Points, path.Points, pair.ARotation, pair.BRotation, pair.Asource, pair.Bsource, MinkowskiSumPick.Largest, clipperNfp);
            if (showSecondaryProgress)
            {
              progressDisplayer.IncrementLoopProgress(ProgressBar.Secondary);
            }
          }
        }
      }
      else
      {
        clipperNfp = minkoskiSumService.ClipperExecuteOuterNfp(pattern.Points, path.Points, MinkowskiSumPick.Largest);
        if (showSecondaryProgress)
        {
          progressDisplayer.IncrementLoopProgress(ProgressBar.Secondary);
        }
      }

      pair.A = null;
      pair.B = null;
      pair.Nfp = clipperNfp;
      return pair;
    }

    private void ProcessAndCaptureResult(NfpPair item, Action<NfpPair> captureResultAction)
    {
      captureResultAction(this.Process(item));
    }
  }
}
