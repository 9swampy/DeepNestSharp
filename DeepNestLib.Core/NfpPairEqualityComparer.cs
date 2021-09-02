namespace DeepNestLib
{
  using System;
  using System.Collections;
  using System.Collections.Generic;

  public class NfpPairEqualityComparer : IEqualityComparer<NfpPairEqualityComparerKey>
  {
    public bool Equals(NfpPairEqualityComparerKey a, NfpPairEqualityComparerKey b)
    {
      if (a.Item1.Length == b.Item1.Length && a.Item2.Length == b.Item2.Length)
      {
        return GetHashCode(a) == GetHashCode(b);
      }

      return false;
    }

    public int GetHashCode(NfpPairEqualityComparerKey obj)
    {
      return HashCode.Combine(((IStructuralEquatable)obj.Item1).GetHashCode(EqualityComparer<SvgPoint>.Default), ((IStructuralEquatable)obj.Item1).GetHashCode(EqualityComparer<SvgPoint>.Default), obj.Item3, obj.Item4, obj.Item5, obj.Item6, obj.Item7);
    }
  }
}
