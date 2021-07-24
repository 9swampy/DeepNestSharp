namespace DeepNestLib
{
  using System.Collections.Generic;
  using DeepNestLib.GeneticAlgorithm;

  public interface IDataInfo
  {
    int Index { get; }

    List<List<INfp>> Children { get; }

    int[] Ids { get; }

    PopulationItem Individual { get; }

    List<List<INfp>> SheetChildren { get; }

    int[] SheetIds { get; }

    INfp[] Sheets { get; }

    int[] SheetSources { get; }

    int[] Sources { get; }
  }
}