namespace DeepNestLib
{
  using System;
  using System.Linq;

  public class DbCache : IDbCache
  {
    public DbCache(IWindowUnk w)
    {
      window = w;
    }

    public bool Has(DbCacheKey dbCacheKey)
    {
      lock (lockobj)
      {
        if (window.nfpCache.ContainsKey(dbCacheKey.Key))
        {
          return true;
        }

        return false;
      }
    }

    public IWindowUnk window;

    public static volatile object lockobj = new object();

    public void Insert(DbCacheKey obj, bool inner = false)
    {
      //if (window.performance.memory.totalJSHeapSize < 0.8 * window.performance.memory.jsHeapSizeLimit)
      {
        lock (lockobj)
        {
          if (!window.nfpCache.ContainsKey(obj.Key))
          {
            window.nfpCache.Add(obj.Key, CacheHelper.CloneNfp(obj.nfp, inner).ToList());
          }
          else
          {
            throw new Exception("trouble .cache already has such key");
            //   window.nfpCache[key] = Background.cloneNfp(new[] { obj.nfp }, inner).ToList();
          }
        }
        //console.log('cached: ',window.cache[key].poly);
        //console.log('using', window.performance.memory.totalJSHeapSize/window.performance.memory.jsHeapSizeLimit);
      }
    }

    public INfp[] Find(DbCacheKey key, bool inner = false)
    {
      lock (lockobj)
      {
        //var key = "A" + obj.A + "B" + obj.B + "Arot" + (int)Math.Round(obj.ARotation) + "Brot" + (int)Math.Round((obj.BRotation));

        //console.log('key: ', key);
        if (window.nfpCache.ContainsKey(key.Key))
        {
          return CacheHelper.CloneNfp(window.nfpCache[key.Key].ToArray(), inner);
        }

        return null;
      }
    }
  }
}
