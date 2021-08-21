namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class MinkowskiKeyEqualityComparer : IEqualityComparer<MinkowskiKey>
  {
    public bool Equals(MinkowskiKey x, MinkowskiKey y)
    {
      if (x.AChildrenLength == y.AChildrenLength &&
          x.ALength == y.ALength &&
          x.Arr1.Length == y.Arr1.Length &&
          x.BLength == y.BLength &&
          x.Hdat.Length == y.Hdat.Length &&
          x.Item2.SequenceEqual(y.Item2) &&
          x.Item4.SequenceEqual(y.Item4) &&
          x.Item5.SequenceEqual(y.Item5) &&
          x.Item7.SequenceEqual(y.Item7))
      {
        if (x.GetHashCode() != y.GetHashCode())
        {
          System.Diagnostics.Debugger.Break();
        }

        return true;
      }

      if (x.GetHashCode() == y.GetHashCode())
      {
        System.Diagnostics.Debugger.Break();
      }

      return false;
    }

    public int GetHashCode(MinkowskiKey key)
    {
      return key.GetHashCode();
    }
  }
}
