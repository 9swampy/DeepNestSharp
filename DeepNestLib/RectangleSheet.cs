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

  public class RectangleSheet : Sheet
  {
    public void Rebuild()
    {
      this.ReplacePoints(new SvgPoint[4]
      {
        new SvgPoint(x, y),
        new SvgPoint(x + Width, y),
        new SvgPoint(x + Width, y + Height),
        new SvgPoint(x, y + Height),
      });
    }
  }
}
