namespace DeepNestLib
{
  using System;
  using System.Linq;

  public class DbCache : IDbCache
  {
    public DbCache(IWindowUnk w)
    {
      Window = w;
    }

    public bool Has(DbCacheKey dbCacheKey)
    {
      lock (lockobj)
      {
        if (Window.nfpCache.ContainsKey(dbCacheKey.Key))
        {
          return true;
        }

        return false;
      }
    }

    public IWindowUnk Window;

    public static volatile object lockobj = new object();

    public void Insert(DbCacheKey obj, bool inner = false)
    {
      //if (window.performance.memory.totalJSHeapSize < 0.8 * window.performance.memory.jsHeapSizeLimit)
      {
        lock (lockobj)
        {
          if (!Window.nfpCache.ContainsKey(obj.Key))
          {
            Window.nfpCache.Add(obj.Key, CacheHelper.CloneNfp(obj.Nfp, inner).ToList());
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
        if (Window.nfpCache.ContainsKey(key.Key))
        {
          return CacheHelper.CloneNfp(Window.nfpCache[key.Key].ToArray(), inner);
        }

        return null;
      }
    }
  }
}
