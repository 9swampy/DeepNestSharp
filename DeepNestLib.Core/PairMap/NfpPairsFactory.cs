namespace DeepNestLib.PairMap
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using DeepNestLib.GeneticAlgorithm;

  public class NfpPairsFactory
  {
    private static volatile object preProcessSyncLock = new object();

    private readonly IWindowUnk window;
    private readonly IProgressDisplayer progressDisplayer;

    public NfpPairsFactory(IWindowUnk window, IProgressDisplayer progressDisplayer)
    {
      this.window = window;
      this.progressDisplayer = progressDisplayer;
    }

    public List<NfpPair> Generate(bool useParallel, DeepNestGene gene)
    {
      this.progressDisplayer.DisplayTransientMessage("Generate Nfp pairs. . .");
      List<NfpPair> pairs = new List<NfpPair>();
      if (useParallel)
      {
        Parallel.For(0, gene.Length, i =>
        {
          {
            foreach (var pair in CreatePairs(i, gene))
            {
              var doc = new DbCacheKey(pair.Asource, pair.Bsource, pair.ARotation, pair.BRotation);
              AddToPairs(pairs, pair, doc);
            }
          }
        });
      }
      else
      {
        for (var i = 0; i < gene.Length; i++)
        {
          foreach (var pair in CreatePairs(i, gene))
          {
            var doc = new DbCacheKey(pair.Asource, pair.Bsource, pair.ARotation, pair.BRotation);
            AddToPairs(pairs, pair, doc);
          }
        }
      }

      return pairs;
    }

    private IEnumerable<NfpPair> CreatePairs(int i, DeepNestGene gene)
    {
      var b = gene[i];
      for (var j = 0; j < i; j++)
      {
        var a = gene[j];
        yield return new NfpPair()
        {
          A = a.Part,
          B = b.Part,
          ARotation = a.Rotation,
          BRotation = b.Rotation,
          Asource = a.Part.Source,
          Bsource = b.Part.Source,
        };
      }
    }

    private void AddToPairs(List<NfpPair> pairs, NfpPair key, DbCacheKey doc)
    {
      lock (preProcessSyncLock)
      {
        if (!this.InPairs(key, pairs.ToArray()) && !window.Has(doc))
        {
          pairs.Add(key);
        }
      }
    }

    private bool InPairs(NfpPair key, NfpPair[] p)
    {
      for (var i = 0; i < p.Length; i++)
      {
        if (p[i].Asource == key.Asource && p[i].Bsource == key.Bsource && p[i].ARotation == key.ARotation && p[i].BRotation == key.BRotation)
        {
          return true;
        }
      }

      return false;
    }
  }
}
