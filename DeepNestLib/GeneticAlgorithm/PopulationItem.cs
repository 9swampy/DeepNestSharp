namespace DeepNestLib.GeneticAlgorithm
{
  using System.Collections.Generic;

  public class PopulationItem
  {
    public bool Processing { get; set; } = false;

    public double Fitness = -1;

    public double[] Rotation { get; }

    public List<INfp> Parts { get; }

    public bool IsPending => !Processing && Fitness == -1;

    public PopulationItem(List<INfp> parts, double[] rotations)
    {
      this.Parts = parts;
      this.Rotation = rotations;
    }
  }
}
