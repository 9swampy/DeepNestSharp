namespace DeepNestLib
{
  public interface INfpHelper
  {
    INfp[] GetInnerNfp(ISheet sheet, INfp part, MinkowskiCache cache, double clipperScale);

    INfp[] GetInnerNfp(INfp a, INfp b, MinkowskiCache minkowskiCache, double clipperScale);
  }
}