namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading;
  using System.Threading.Tasks;
  using ClipperLib;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Placement;

  public class PolygonTreeItem
  {
    public INfp Polygon;
    public PolygonTreeItem Parent;
    public List<PolygonTreeItem> Childs = new List<PolygonTreeItem>();
  }
}
