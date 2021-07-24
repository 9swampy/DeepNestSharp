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

  public class NestItem
  {
    public NFP Polygon;
    public int Quantity;
    public bool IsSheet;
  }
}
