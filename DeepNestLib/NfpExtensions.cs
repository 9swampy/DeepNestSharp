namespace DeepNestLib
{
    using System.Collections.Generic;
    using System.Linq;

    public static class NfpExtensions
    {
        public static bool ContainsDxfs(this ICollection<NFP> list) => list.Any(o => o.Name.ToLower().Contains(".dxf"));

        public static bool ContainsSvgs(this ICollection<NFP> list) => list.Any(o => o.Name.ToLower().Contains(".svg"));
    }
}
