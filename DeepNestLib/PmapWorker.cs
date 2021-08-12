namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;

  public class PmapWorker
  {
    private readonly IList<NfpPair> pairs;
    private readonly IProgressDisplayer progressDisplayer;
    private readonly bool useParallel;
    private readonly IMinkowskiSumService minkoskiSumService;
    private int processed = 0;
    private static readonly NfpPairDictionary NfpPairCache = new NfpPairDictionary();
    private static volatile object nfpPairCacheSyncLock = new object();

    public PmapWorker(IList<NfpPair> pairs, IProgressDisplayer progressDisplayer, bool useParallel, IMinkowskiSumService minkoskiSumService)
    {
      this.pairs = pairs;
      this.progressDisplayer = progressDisplayer;
      this.useParallel = useParallel;
      this.minkoskiSumService = minkoskiSumService;
    }

    private void DisplayProgress()
    {
      Interlocked.Increment(ref this.processed);
      if (this.pairs != null)
      {
        this.progressDisplayer.DisplayProgress(Math.Min(1F, (double)this.processed / (double)this.pairs.Count));
      }
    }

    public NfpPair[] PmapDeepNest()
    {
      progressDisplayer.InitialiseLoopProgress("PmapDeepNest", pairs.Count);
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
      DisplayProgress();
      return pair;
    }
  }
}
