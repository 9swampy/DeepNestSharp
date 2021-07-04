namespace DeepNestLib.GeneticAlgorithm
{
  using System.Collections.Generic;

  public class PopulationItem
  {
    public object processing = null;

    public double? fitness;

    public double fitnessAlt;

    public float[] Rotation { get; }

    public List<NFP> placements { get; }

    public PopulationItem(List<NFP> placements, float[] rotation)
    {
      this.placements = placements;
      this.Rotation = rotation;
    }
  }
}
