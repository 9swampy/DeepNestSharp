namespace DeepNestLib.GeneticAlgorithm
{
  public interface ISheetPlacementFitness
  {
    double Bounds { get; }

    double MaterialUtilization { get; }

    double MaterialWasted { get; }

    double Sheets { get; }
  }
}