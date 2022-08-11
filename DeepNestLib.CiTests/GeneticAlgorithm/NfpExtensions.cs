namespace DeepNestLib.CiTests.GeneticAlgorithm
{
  using DeepNestLib.GeneticAlgorithm;

  internal static class NfpExtensions
  {
    public static INfp[] ApplyIndex(this INfp[] nfps)
    {
      for (var idx = 0; idx < nfps.Length; idx++)
      {
        nfps[idx].Id = idx;
      }

      return nfps;
    }

    public static IDeepNestChromosome[] ApplyIndex(this IDeepNestChromosome[] gene)
    {
      for (var idx = 0; idx < gene.Length; idx++)
      {
        gene[idx].Part.Id = idx;
      }

      return gene;
    }
  }
}
