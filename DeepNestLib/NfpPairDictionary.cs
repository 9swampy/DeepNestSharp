namespace DeepNestLib
{
  using System.Collections.Generic;
  using System.Threading;

  public class NfpPairDictionary : Dictionary<NfpPairEqualityComparerKey, NFP>
  {
    private int wasCached = 0;
    private int notCached = 0;

    public double PercentCached => (double)wasCached / (wasCached + notCached);

    public NfpPairDictionary()
      : base(new NfpPairEqualityComparer())
    {
    }

    public bool TryGetValue(SvgPoint[] a, SvgPoint[] b, double aRotation, double bRotation, int aSource, int bSource, MinkowskiSumPick minkowskiSumPick, out NFP value)
    {
      var result = this.TryGetValue(new NfpPairEqualityComparerKey(a, b, aRotation, bRotation, aSource, bSource, minkowskiSumPick), out value);
      if (result)
      {
        Interlocked.Increment(ref wasCached);
      }
      else
      {
        Interlocked.Increment(ref notCached);
      }

      return result;
    }

    public void Add(SvgPoint[] a, SvgPoint[] b, double aRotation, double bRotation, int aSource, int bSource, MinkowskiSumPick minkowskiSumPick, NFP value)
    {
      this.Add(new NfpPairEqualityComparerKey(a, b, aRotation, bRotation, aSource, bSource, minkowskiSumPick), value);
    }
  }
}
