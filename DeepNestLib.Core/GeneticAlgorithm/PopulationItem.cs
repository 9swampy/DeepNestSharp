namespace DeepNestLib.GeneticAlgorithm
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class PopulationItem
  {
    public PopulationItem(Gene gene)
    {
      this.Gene = gene;
    }

    public bool Processing { get; set; } = false;

    public double Fitness { get; internal set; } = -1;

    public Gene Gene { get; }

    public bool IsPending => !Processing && Fitness == -1;

    public int Index { get; internal set; }
  }
}
