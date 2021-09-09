namespace DeepNestLib
{
  using System.Collections.Generic;

  public interface IMinkowskiSumService
  {
    NoFitPolygon ClipperExecuteOuterNfp(SvgPoint[] pattern, SvgPoint[] path, MinkowskiSumPick minkowskiSumPick);

    INfp[] DllImportExecute(INfp a, INfp b, MinkowskiSumCleaning minkowskiSumCleaning);

    INfp[] NewMinkowskiSum(IList<SvgPoint> pattern, INfp path, WithChildren withChildren, bool takeOnlyBiggestArea);
  }
}