namespace DeepNestLib
{
  using System.Collections.Generic;
  using DeepNestLib.Placement;
  
  public interface INestingContextState
  {
    INestResult Current { get; }

    bool IsErrored { get; }

    SvgNest Nest { get; }

    ICollection<INfp> Polygons { get; }

    List<INfp> Sheets { get; }
  }
}