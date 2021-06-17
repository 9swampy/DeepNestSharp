namespace DeepNestLib
{
    using System.Collections.Generic;

    public class PopulationItem
    {
        public object processing = null;

        public double? fitness;

        public float[] Rotation;
        public List<NFP> placements;

        public NFP[] paths;
        public double area;
    }
}
