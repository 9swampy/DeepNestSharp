namespace DeepNestLib.GeneticAlgorithm
{
  public class PopulationItem
  {
    public PopulationItem(DeepNestGene gene)
    {
      this.Gene = gene;
    }

    public bool Processing { get; set; } = false;

    public double Fitness { get; internal set; } = -1;

    public DeepNestGene Gene { get; }

    public bool IsPending => !Processing && Fitness == -1;

    public int Index { get; internal set; }
  }
}
