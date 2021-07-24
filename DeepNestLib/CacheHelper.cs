namespace DeepNestLib
{
  using System.Collections.Generic;
  using System.Linq;

  public class CacheHelper
  {
    public static INfp[] CloneNfp(INfp[] nfp, bool inner = false)
    {
      if (!inner)
      {
        return new[] { nfp.First().Clone() };
      }

      // inner nfp is actually an array of nfps
      List<INfp> result = new List<INfp>();
      for (var i = 0; i < nfp.Count(); i++)
      {
        result.Add(nfp[i].Clone());
      }

      return result.ToArray();
    }
  }
}
