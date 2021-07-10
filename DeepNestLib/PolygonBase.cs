namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public abstract class PolygonBase
  {
    protected SvgPoint[] points;

    protected PolygonBase(SvgPoint[] points)
    {
      this.points = points;
    }
  }
}
