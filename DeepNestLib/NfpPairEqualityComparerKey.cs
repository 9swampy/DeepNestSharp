namespace DeepNestLib
{
  using System;

  public class NfpPairEqualityComparerKey : Tuple<SvgPoint[], SvgPoint[], float, float, int, int, MinkowskiSumPick>
  {
    public NfpPairEqualityComparerKey(SvgPoint[] a, SvgPoint[] b, float aRotation, float bRotation, int aSource, int bSource, MinkowskiSumPick minkowskiSumPick)
      : base(a, b, aRotation, bRotation, aSource, bSource, minkowskiSumPick)
    {
    }
  }
}
