namespace DeepNestLib.PairMap
{
  using System;
  using DeepNestLib.Placement;

  public class NfpPairEqualityComparerKey : Tuple<SvgPoint[], SvgPoint[], double, double, int, int, MinkowskiSumPick>
  {
    public NfpPairEqualityComparerKey(SvgPoint[] a, SvgPoint[] b, double aRotation, double bRotation, int aSource, int bSource, MinkowskiSumPick minkowskiSumPick)
      : base(a, b, aRotation, bRotation, aSource, bSource, minkowskiSumPick)
    {
    }
  }
}
