namespace DeepNestLib.PairMap
{
  using DeepNestLib.GeneticAlgorithm;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  public class NfpPairsFactory
  {
    private static volatile object preProcessSyncLock = new object();

    private readonly IWindowUnk window;

    public NfpPairsFactory(IWindowUnk window)
    {
      this.window = window;
    }

    public List<NfpPair> Generate(bool useParallel, DeepNestGene gene)
    {
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
