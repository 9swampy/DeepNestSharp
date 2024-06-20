namespace DeepNestLib.PairMap
{
  using DeepNestLib.Placement;
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
        this.progressDisplayer.InitialiseLoopProgress(ProgressBar.Secondary, "Pmap. . .", this.pairs.Count);
        this.showSecondaryProgress = true;
      }
      NfpPair[] ret = new NfpPair[this.pairs.Count];
      if (this.useParallel)
      {
        Parallel.For(0, this.pairs.Count, (i) =>
        {
          NfpPair item = this.pairs[i];
          this.ProcessAndCaptureResult(item, result => ret[i] = result);
        });
      }
      else
      {
        for (var i = 0; i < this.pairs.Count; i++)
        {
          NfpPair item = this.pairs[i];
          ret[i] = this.Process(item);
        }
      }

      if (UseNfpPairCache)
      {
        this.state.SetNfpPairCachePercentCached(NfpPairCache.PercentCached);
      }

      if (this.showSecondaryProgress)
      {
        this.progressDisplayer.IsVisibleSecondaryProgressBar = false;
      }

      return ret.ToArray();
    }

    internal NfpPair Process(NfpPair pair)
    {
      INfp pattern = pair.A.Rotate(pair.ARotation, WithChildren.Excluded);
      INfp path = pair.B.Rotate(pair.BRotation, WithChildren.Excluded);

      NoFitPolygon clipperNfp;
      if (UseNfpPairCache)
      {
        lock (nfpPairCacheSyncLock)
        {
          if (!NfpPairCache.TryGetValue(pattern.Points, path.Points, pair.ARotation, pair.BRotation, pair.Asource, pair.Bsource, MinkowskiSumPick.Largest, out clipperNfp))
          {
            clipperNfp = this.minkoskiSumService.ClipperExecuteOuterNfp(pattern.Points, path.Points, MinkowskiSumPick.Largest);
            NfpPairCache.Add(pattern.Points, path.Points, pair.ARotation, pair.BRotation, pair.Asource, pair.Bsource, MinkowskiSumPick.Largest, clipperNfp);
            if (this.showSecondaryProgress)
            {
              this.progressDisplayer.IncrementLoopProgress(ProgressBar.Secondary);
            }
          }
        }
      }
      else
      {
        clipperNfp = this.minkoskiSumService.ClipperExecuteOuterNfp(pattern.Points, path.Points, MinkowskiSumPick.Largest);
        if (this.showSecondaryProgress)
        {
          this.progressDisplayer.IncrementLoopProgress(ProgressBar.Secondary);
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
