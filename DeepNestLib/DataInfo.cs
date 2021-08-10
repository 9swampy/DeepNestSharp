namespace DeepNestLib
{
  using System.Collections.Generic;
  using DeepNestLib.GeneticAlgorithm;

  public class DataInfo : IDataInfo
  {
    public int Index { get; set; }

    public ISheet[] Sheets { get; set; }

    public int[] SheetIds { get; set; }

    public int[] SheetSources { get; set; }

    public List<List<INfp>> SheetChildren { get; set; }

    public PopulationItem Individual { get; set; }

    public int[] Ids { get; set; }

    public int[] Sources { get; set; }

    public List<List<INfp>> Children { get; set; }
  }
}
