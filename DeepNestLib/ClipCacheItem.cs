namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Runtime.InteropServices;
  using System.Text;
  using System.Threading.Tasks;
  using ClipperLib;
  using DeepNestLib.Placement;

  public class ClipCacheItem
  {
    public int index;
    public IntPoint[][] nfpp;
  }
}
