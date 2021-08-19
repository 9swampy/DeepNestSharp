namespace DeepNestLib
{
  using System;
  using System.Collections;
  using System.Collections.Generic;

  public class MinkowskiKeyEqualityComparer : IEqualityComparer<MinkowskiKey>
  {
    public bool Equals(MinkowskiKey x, MinkowskiKey y)
    {
      if (x.AChildrenLength == y.AChildrenLength &&
          x.ALength == y.ALength &&
          x.Arr1.Length == y.Arr1.Length &&
          x.BLength == y.BLength &&
          x.Hdat.Length == y.Hdat.Length)
      {
        return x.GetHashCode() == y.GetHashCode();
      }

      return false;
    }

    public int GetHashCode(MinkowskiKey key)
    {
      return key.GetHashCode();
    }
  }
}
