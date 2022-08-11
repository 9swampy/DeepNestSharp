namespace DeepNestLib.GeneticAlgorithm
{
  public interface IDeepNestChromosome
  {
    /// <summary>
    /// Gets the original part, as loaded. Should never be mutated.
    /// </summary>
    INfp Part { get; }

    /// <summary>
    /// Gets the rotation instruction.
    /// </summary>
    double Rotation { get; }

    string ToString();

    void SetRotation(double rotation);
  }
}