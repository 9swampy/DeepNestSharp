namespace DeepNestLib.GeneticAlgorithm
{
  public interface ISheetPlacementFitness
  {
    double Bounds { get; }

    double Utilization { get; }

    double Wasted { get; }

    double Sheets { get; }

    double Total { get; }
  }
}