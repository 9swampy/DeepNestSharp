namespace DeepNestLib.GeneticAlgorithm
{
  using System.Collections.Generic;

  public class PopulationItem
  {
    public bool Processing { get; set; } = false;

    public double Fitness = -1;

    public float[] Rotation { get; }

    public List<NFP> Parts { get; }

    public bool IsPending => !Processing && Fitness == -1;

    public PopulationItem(List<NFP> parts, float[] rotations)
    {
      this.Parts = parts;
      this.Rotation = rotations;
    }
  }
}
