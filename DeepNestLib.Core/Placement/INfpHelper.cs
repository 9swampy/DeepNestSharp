namespace DeepNestLib
{
  using System;
  using DeepNestLib.Placement;

  public interface INfpHelper
  {
    INfp[] GetInnerNfp(ISheet sheet, INfp part, MinkowskiCache cache, double clipperScale, bool useDllImport, Action<string> verboseLog);

    INfp[] GetInnerNfp(INfp a, INfp b, MinkowskiCache minkowskiCache, double clipperScale, bool useDllImport, Action<string> verboseLog);
  }
}