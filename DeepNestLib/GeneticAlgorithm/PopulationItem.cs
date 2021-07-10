namespace DeepNestLib.GeneticAlgorithm
{
  using System.Collections.Generic;

  public class PopulationItem
  {
    public bool Processing { get; set; } = false;

    public double? fitness;

    public double fitnessAlt;

    public float[] Rotation { get; }

    public List<NFP> Placements { get; }

    public bool IsPending => !Processing && fitness == null;

    public PopulationItem(List<NFP> placements, float[] rotation)
    {
      this.Placements = placements;
      this.Rotation = rotation;
    }
  }
}
