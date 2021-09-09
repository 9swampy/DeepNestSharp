namespace DeepNestLib
{
  using System.Collections.Generic;

  public class PolygonTreeItem
  {
    public INfp Polygon;
    public PolygonTreeItem Parent;
    public List<PolygonTreeItem> Childs = new List<PolygonTreeItem>();
  }
}
