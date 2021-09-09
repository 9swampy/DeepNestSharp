namespace DeepNestLib.GeneticAlgorithm
{
  public interface IOriginalFitnessSheet
  {
    double Bounds { get; }

    double MaterialUtilization { get; }

    double MaterialWasted { get; }

    double Sheets { get; }

    double Evaluate();

    string ToString();
  }
}