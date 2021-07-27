namespace DeepNestLib
{
  using System.Collections.Generic;

  public class NfpPairDictionary : Dictionary<NfpPairEqualityComparerKey, NFP>
  {
    public NfpPairDictionary()
      : base(new NfpPairEqualityComparer())
    {
    }

    public bool TryGetValue(SvgPoint[] a, SvgPoint[] b, double aRotation, double bRotation, int aSource, int bSource, MinkowskiSumPick minkowskiSumPick, out NFP value)
    {
      return this.TryGetValue(new NfpPairEqualityComparerKey(a, b, aRotation, bRotation, aSource, bSource, minkowskiSumPick), out value);
    }

    public void Add(SvgPoint[] a, SvgPoint[] b, double aRotation, double bRotation, int aSource, int bSource, MinkowskiSumPick minkowskiSumPick, NFP value)
    {
      this.Add(new NfpPairEqualityComparerKey(a, b, aRotation, bRotation, aSource, bSource, minkowskiSumPick), value);
    }
  }
}
