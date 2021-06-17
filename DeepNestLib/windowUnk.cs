namespace DeepNestLib
{
    using System.Collections.Generic;

    public class windowUnk
    {
        public windowUnk()
        {
            this.db = new DbCache(this);
        }

        public Dictionary<string, List<NFP>> nfpCache = new Dictionary<string, List<NFP>>();
        public DbCache db;
    }
}
