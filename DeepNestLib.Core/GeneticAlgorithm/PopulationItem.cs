namespace DeepNestLib.GeneticAlgorithm
{
  using System.Collections.Generic;
  using System.Linq;

  public class PopulationItem
  {
    public PopulationItem(List<INfp> parts, double[] rotations)
      : this(BuildGene(parts, rotations).ToArray())
    {
    }

    public PopulationItem(Chromosome[] gene)
    {
      this.Gene = gene;
    }

    private static IEnumerable<Chromosome> BuildGene(List<INfp> parts, double[] rotations)
    {
      for (int i = 0; i < parts.Count; i++)
      {
        yield return new Chromosome(parts[i], rotations[i]);
      }
    }

    public bool Processing { get; set; } = false;

    public double Fitness { get; internal set; } = -1;

    public Chromosome[] Gene { get; }

    public bool IsPending => !Processing && Fitness == -1;

    public int Index { get; internal set; }
  }
}
