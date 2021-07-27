namespace DeepNestLib
{
  public interface IMinkowskiSumService
  {
    int CallCounter { get; }

    NFP ClipperExecute(INfp a, INfp b, MinkowskiSumPick minkowskiSumPick);

    NFP ClipperExecute(SvgPoint[] a, SvgPoint[] b, MinkowskiSumPick minkowskiSumPick);

    INfp DllImportExecute(INfp a, INfp b, MinkowskiSumCleaning minkowskiSumCleaning = MinkowskiSumCleaning.None);
  }
}