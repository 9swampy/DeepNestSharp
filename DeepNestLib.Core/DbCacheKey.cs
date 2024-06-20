namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;

  public class DbCacheKey
  {
    public DbCacheKey(int? a, int? b, double aRotation, double bRotation, IEnumerable<INfp> nfps)
    {
      this.A = a;
      this.B = b;
      this.ARotation = aRotation;
      this.BRotation = bRotation;
      this.Nfp = nfps.ToArray();
    }

    public DbCacheKey(int? a, int? b, double aRotation, double bRotation)
    // : this(a, b, aRotation, bRotation, null)
    {
      this.A = a;
      this.B = b;
      this.ARotation = aRotation;
      this.BRotation = bRotation;
    }

    public int? A { get; }

    public int? B { get; }

    public double ARotation { get; }

    public double BRotation { get; }

    public INfp[] Nfp { get; }

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
