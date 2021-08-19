namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Text;
  using System.Text.Json;
  using System.Text.Json.Serialization;
#if NCRUNCH
  using System.Text;
#endif
  using ClipperLib;
  using DeepNestLib.NestProject;
  using DeepNestLib.Placement;
  using static DeepNestLib.PlacementWorker;

  public class SheetNfp
  {
    [JsonConstructor]
    public SheetNfp()
    {
    }

    // inner NFP
    public SheetNfp(NfpHelper nfpHelper, INfp sheet, INfp part, double clipperScale)
    {
      InnerNfp = nfpHelper.GetInnerNfp(sheet, part, MinkowskiCache.Cache, clipperScale);
    }

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    public INfp this[int index] { get => InnerNfp[index]; }

    internal bool CanAcceptPart
    {
      get
      {
        if (InnerNfp != null && InnerNfp.Count() > 0)
        {
          if (InnerNfp[0].Length == 0)
          {
            throw new ArgumentException();
          }
          else
          {
            return true;
          }
        }

        return false;
      }
    }

    public int Length => InnerNfp.Length;

    [JsonInclude]
    public INfp[] InnerNfp { get; private set; }
  }
}