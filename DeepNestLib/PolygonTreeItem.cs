namespace DeepNestLib
{
    using System.Collections.Generic;

    public class PolygonTreeItem
    {
        public NFP Polygon;
        public PolygonTreeItem Parent;
        public List<PolygonTreeItem> Childs = new List<PolygonTreeItem>();
    }
}
