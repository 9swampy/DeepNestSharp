namespace DeepNestLib
{
  public interface IMinkowskiSumService
  {
    NFP ClipperExecuteOuterNfp(SvgPoint[] a, SvgPoint[] b, MinkowskiSumPick minkowskiSumPick);

    INfp[] DllImportExecute(INfp a, INfp b, MinkowskiSumCleaning minkowskiSumCleaning);

    INfp[] NewMinkowskiSum(INfp pattern, INfp path, WithChildren withChildren, bool takeOnlyBiggestArea);
  }
}