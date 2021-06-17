namespace DeepNestLib
{
    using System.Collections.Generic;

    public class PlacementItem
    {
        public double? mergedLength;
        public object mergedSegments;
        public List<List<ClipperLib.IntPoint>> nfp;
        public int id;
        public NFP hull;
        public NFP hullsheet;

        public float rotation;
        public double x;
        public double y;
        public int source;
    }
}
