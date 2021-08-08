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

  public class NestItem<TNfp>
    where TNfp : INfp
  {
    public TNfp Polygon { get; set; }

    public int Quantity { get; set; }

    public bool IsSheet => Polygon is ISheet;
  }
}
