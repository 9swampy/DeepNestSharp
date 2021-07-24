namespace DeepNestLib
{
  using System.Collections.Generic;

  public class WindowUnk : IWindowUnk
  {
    public WindowUnk()
    {
      db = new DbCache(this);
    }

    public Dictionary<string, List<INfp>> nfpCache { get; } = new Dictionary<string, List<INfp>>();

    private IDbCache db { get; }

    public INfp[] Find(DbCacheKey obj, bool inner = false)
    {
      return this.db.Find(obj, inner);
    }

    public bool Has(DbCacheKey dbCacheKey)
    {
      return this.db.Has(dbCacheKey);
    }

    public void Insert(DbCacheKey obj, bool inner = false)
    {
      this.db.Insert(obj, inner);
    }
  }
}
