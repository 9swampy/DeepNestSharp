namespace DeepNestLib.CiTests
{
  internal static class NfpExtensions
  {
    public static INfp[] ApplyIndex(this INfp[] nfps)
    {
      for (int idx = 0; idx < nfps.Length; idx++)
      {
        nfps[idx].Id = idx;
      }

      return nfps;
    }

    public static Chromosome[] ApplyIndex(this Chromosome[] gene)
    {
      for (int idx = 0; idx < gene.Length; idx++)
      {
        gene[idx].Part.Id = idx;
      }

      return gene;
    }
  }
}
