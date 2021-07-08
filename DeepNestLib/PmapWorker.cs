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
    private int processed = 0;
    private static readonly NfpPairDictionary NfpPairCache = new NfpPairDictionary();
    private static volatile object nfpPairCacheSyncLock = new object();

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

    internal NfpPair Process(NfpPair pair)
    {
      var a = pair.A.Rotate(pair.ARotation);
      var b = pair.B.Rotate(pair.BRotation);

      NFP clipperNfp;
      lock (nfpPairCacheSyncLock)
      {
        if (!NfpPairCache.TryGetValue(a.Points, b.Points, pair.ARotation, pair.BRotation, pair.Asource, pair.Bsource, MinkowskiSumPick.Largest, out clipperNfp))
        {
          clipperNfp = MinkowskiSum.ClipperExecute(a.Points, b.Points, MinkowskiSumPick.Largest);
          NfpPairCache.Add(a.Points, b.Points, pair.ARotation, pair.BRotation, pair.Asource, pair.Bsource, MinkowskiSumPick.Largest, clipperNfp);
        }
      }

      pair.A = null;
      pair.B = null;
      pair.nfp = clipperNfp;
      return pair;
    }
  }
}
