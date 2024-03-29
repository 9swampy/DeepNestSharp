﻿namespace DeepNestLib
{
  using DeepNestLib.Placement;
  using System;

  public interface INfpHelper
  {
    INfp[] GetInnerNfp(ISheet sheet, INfp part, MinkowskiCache cache, double clipperScale, bool useDllImport, Action<string> verboseLog);

    INfp[] GetInnerNfp(INfp a, INfp b, MinkowskiCache minkowskiCache, double clipperScale, bool useDllImport, Action<string> verboseLog);
  }
}