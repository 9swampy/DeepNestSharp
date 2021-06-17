namespace DeepNestLib
{
    using System;
    using System.Linq;

    public class DbCache
    {
        public DbCache(windowUnk w)
        {
            this.window = w;
        }

        public bool Has(DbCacheKey obj)
        {
            lock (this.lockobj)
            {
                var key = this.GetKey(obj);

                // var key = "A" + obj.A + "B" + obj.B + "Arot" + (int)Math.Round(obj.ARotation)                + "Brot" + (int)Math.Round(obj.BRotation);
                if (this.window.nfpCache.ContainsKey(key))
                {
                    return true;
                }

                return false;
            }
        }

        public windowUnk window;
        public object lockobj = new object();

        private string GetKey(DbCacheKey obj)
        {
            var key = "A" + obj.A + "B" + obj.B + "Arot" + (int)Math.Round(obj.ARotation * 10000) + "Brot" + (int)Math.Round(obj.BRotation * 10000) + ";" + obj.Type;
            return key;
        }

        internal void Insert(DbCacheKey obj, bool inner = false)
        {
            var key = this.GetKey(obj);

            // if (window.performance.memory.totalJSHeapSize < 0.8 * window.performance.memory.jsHeapSizeLimit)
            {
                lock (this.lockobj)
                {
                    if (!this.window.nfpCache.ContainsKey(key))
                    {
                        this.window.nfpCache.Add(key, Background.cloneNfp(obj.nfp, inner).ToList());
                    }
                    else
                    {
                        throw new Exception("trouble .cache allready has such key");

                        // window.nfpCache[key] = Background.cloneNfp(new[] { obj.nfp }, inner).ToList();
                    }
                }

                // console.log('cached: ',window.cache[key].poly);
                // console.log('using', window.performance.memory.totalJSHeapSize/window.performance.memory.jsHeapSizeLimit);
            }
        }

        public NFP[] Find(DbCacheKey obj, bool inner = false)
        {
            lock (this.lockobj)
            {
                var key = this.GetKey(obj);

                // var key = "A" + obj.A + "B" + obj.B + "Arot" + (int)Math.Round(obj.ARotation) + "Brot" + (int)Math.Round((obj.BRotation));

                // console.log('key: ', key);
                if (this.window.nfpCache.ContainsKey(key))
                {
                    return Background.cloneNfp(this.window.nfpCache[key].ToArray(), inner);
                }

                return null;
            }
        }
    }
}
