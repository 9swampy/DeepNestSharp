namespace DeepNestLib
{
  using System;
  using System.Collections;
  using System.Collections.Generic;

  public class MinkowskiEqualityComparer : IEqualityComparer<MinkowskiKey>
  {
    public bool Equals(MinkowskiKey x, MinkowskiKey y)
    {
      if (x.AChildrenLength == y.AChildrenLength &&
          x.ALength == y.ALength &&
          x.Arr1.Length == y.Arr1.Length &&
          x.BLength == y.BLength &&
          x.Hdat.Length == y.Hdat.Length)
      {
        return GetHashCode(x) == GetHashCode(y);
      }

      return false;
    }

    public int GetHashCode(MinkowskiKey key)
    {
      return HashCode.Combine(
        key.AChildrenLength,
        key.ALength,
        ((IStructuralEquatable)key.APoints).GetHashCode(EqualityComparer<double>.Default),
        ((IStructuralEquatable)key.Arr1).GetHashCode(EqualityComparer<int>.Default),
        key.BLength,
        ((IStructuralEquatable)key.BPoints).GetHashCode(EqualityComparer<double>.Default),
        ((IStructuralEquatable)key.Hdat).GetHashCode(EqualityComparer<double>.Default));
    }
  }
}
