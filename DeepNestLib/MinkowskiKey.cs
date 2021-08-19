namespace DeepNestLib
{
  using System;
using System.Collections.Generic;
using System.Collections;
  using System.Linq;
  using System.Text.Json.Serialization;

  public class MinkowskiKey : Tuple<int, decimal[], int, int[], decimal[], int, decimal[]>
  {
    [JsonConstructor]
    public MinkowskiKey(int item1, decimal[] item2, int item3, int[] item4, decimal[] item5, int item6, decimal[] item7)
      : base(item1, item2, item3, item4, item5, item6, item7)
    {
    }

    public MinkowskiKey(int aLength, IEnumerable<double> aPoints, int aChildrenLength, int[] arr1, IEnumerable<double> hdat, int bLength, IEnumerable<double> bPoints)
      : base(aLength, ToScaledLongArray(aPoints), aChildrenLength, arr1, ToScaledLongArray(hdat), bLength, ToScaledLongArray(bPoints))
    {
    }

    private static decimal[] ToScaledLongArray(IEnumerable<double> doubleArray)
    {
      //return doubleArray.Select(o => (decimal)o * KeyScaler).ToArray();
      return doubleArray.Select(o => Truncate((decimal)o, 10)).ToArray();
      //return doubleArray;
    }

    private static decimal Truncate(decimal d, byte decimals)
    {
      decimal r = Math.Round(d, decimals);

      if (d > 0 && r > d)
      {
        return r - new decimal(1, 0, 0, false, decimals);
      }
      else if (d < 0 && r < d)
      {
        return r + new decimal(1, 0, 0, false, decimals);
      }

      return r;
    }

    private static double[] ToDoubleArray(decimal[] doubleArray)
    {
      //return doubleArray.Select(o => (double)o / KeyScaler).ToArray();
      return doubleArray.Select(o => (double)o).ToArray();
      //return doubleArray;
    }

    [JsonIgnore]
    public int ALength => Item1;

    [JsonIgnore]
    public double[] APoints => ToDoubleArray(Item2);

    [JsonIgnore]
    public int AChildrenLength => Item3;

    [JsonIgnore]
    public int[] Arr1 => Item4;

    [JsonIgnore]
    public double[] Hdat => ToDoubleArray(Item5);

    [JsonIgnore]
    public int BLength => Item6;

    [JsonIgnore]
    public double[] BPoints => ToDoubleArray(Item7);

    public override string ToString()
    {
      return $"a{ALength}-ac{AChildrenLength}-arr{Arr1.Length}-h{Hdat.Length}-b{BLength}-bp{BPoints.Length}";
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(
        this.Item1,
        ((IStructuralEquatable)this.Item2).GetHashCode(EqualityComparer<decimal>.Default),
        this.Item3,
        ((IStructuralEquatable)this.Item4).GetHashCode(EqualityComparer<int>.Default),
        ((IStructuralEquatable)this.Item5).GetHashCode(EqualityComparer<decimal>.Default),
        this.Item6,
        ((IStructuralEquatable)this.Item7).GetHashCode(EqualityComparer<decimal>.Default));
    }

    public override bool Equals(object obj)
    {
      if (obj is MinkowskiKey key)
      {
        return new MinkowskiKeyEqualityComparer().Equals(this, key);
      }

      return false;
    }
  }
}
