namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading;
  using System.Threading.Tasks;
  using ClipperLib;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Placement;

  public class DbCacheKey
  {
    public DbCacheKey(int? a, int? b, float aRotation, float bRotation, IEnumerable<INfp> nfps)
    {
      A = a;
      B = b;
      ARotation = aRotation;
      BRotation = bRotation;
      nfp = nfps.ToArray();
    }

    public DbCacheKey(int? a, int? b, float aRotation, float bRotation)
    // : this(a, b, aRotation, bRotation, null)
    {
      A = a;
      B = b;
      ARotation = aRotation;
      BRotation = bRotation;
    }

    public int? A { get; }

    public int? B { get; }

    public float ARotation { get; }

    public float BRotation { get; }

    public INfp[] nfp { get; }

    public int Type { get; }

    public string Key
    {
      get
      {
        var key = new StringBuilder(30).Append("A")
                                      .Append(this.A)
                                      .Append("B")
                                      .Append(this.B)
                                      .Append("Arot")
                                      .Append((int)Math.Round(this.ARotation * 10000))
                                      .Append("Brot")
                                      .Append((int)Math
                                      .Round(this.BRotation * 10000))
                                      .Append(";")
                                      .Append(this.Type)
                                      .ToString();
        return key;
      }
    }
  }
}
