namespace DeepNestLib
{
  public interface IDbCache
  {
    bool Has(DbCacheKey dbCacheKey);

    NFP[] Find(DbCacheKey obj, bool inner = false);

    void Insert(DbCacheKey obj, bool inner = false);
  }
}