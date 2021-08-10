namespace DeepNestLib
{
  using System.Collections.Generic;
  using DeepNestLib.Placement;
  
  public interface INestingContextState
  {
    INestResult Current { get; }

    SvgNest Nest { get; }

    ICollection<INfp> Polygons { get; }

    IList<ISheet> Sheets { get; }
  }
}