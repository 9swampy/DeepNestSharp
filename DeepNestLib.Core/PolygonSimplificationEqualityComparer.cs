namespace DeepNestLib
{
  using System;
  using System.Collections;
  using System.Collections.Generic;

  public class PolygonSimplificationEqualityComparer : IEqualityComparer<PolygonSimplificationKey>
  {
    public bool Equals(PolygonSimplificationKey x, PolygonSimplificationKey y)
    {
      if (x.Item1.Length == y.Item1.Length)
      {
        return GetHashCode(x) == GetHashCode(y);
      }

      return false;
    }

    public int GetHashCode(PolygonSimplificationKey obj)
    {
      return HashCode.Combine(((IStructuralEquatable)obj.Item1).GetHashCode(EqualityComparer<SvgPoint>.Default), obj.Item2 ?? 1, obj.Item3, obj.Item4);
    }
  }
}
