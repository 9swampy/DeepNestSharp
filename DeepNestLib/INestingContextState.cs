namespace DeepNestLib
{
  using DeepNestLib.Placement;
  using System.Collections.Generic;

  public interface INestingContextState
  {
    INestResult Current { get; }
    bool IsErrored { get; }
    int Iterations { get; }
    SvgNest Nest { get; }
    int PlacedPartsCount { get; }
    ICollection<INfp> Polygons { get; }
    List<INfp> Sheets { get; }
  }
}