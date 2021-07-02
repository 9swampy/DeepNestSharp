namespace DeepNestLib.GeneticAlgorithm
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using ClipperLib;

  public class PopulationItem
  {
    public object processing = null;

    public double? fitness;

    public float[] Rotation { get; }

    public List<NFP> placements { get; }

    public PopulationItem(List<NFP> placements, float[] rotation)
    {
      this.placements = placements;
      this.Rotation = rotation;
    }
  }
}
