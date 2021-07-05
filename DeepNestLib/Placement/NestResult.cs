namespace DeepNestLib.Placement
{
  using System;
  using System.Collections.Generic;
  using DeepNestLib.GeneticAlgorithm;

  public class NestResult
  {
    private double fitnessAlt = -1;

    [Obsolete("Exposed only for interim UnitTest comparisons of old with replacement.")]
    public double? fitness = null;

    public NestResult(int nestIndex, double area, SheetPlacementCollection allplacements, IList<NFP> unplacedParts, double? fitness, double mergedLength, double fitnessSheets, double fitnessBounds, double fitnessUnplaced, PlacementTypeEnum placementType)
    {
      UsedSheets = allplacements;
      this.UnplacedParts = unplacedParts;
      this.fitness = fitness;
      this.NestIndex = nestIndex;
      this.area = area;
      this.mergedLength = mergedLength;
      this.FitnessSheets = fitnessSheets;
      this.FitnessBounds = fitnessBounds;
      this.FitnessUnplaced = fitnessUnplaced;
      this.PlacementType = placementType;
    }

    public double FitnessAlt
    {
      get
      {
        if (this.fitness == null || this.fitnessAlt == -1)
        {
          this.fitnessAlt = new OriginalFitness().Evaluate(this);
        }

        return this.fitnessAlt;
      }
    }

    public double? Fitness
    {
      get
      {
        if (this.PlacementType == PlacementTypeEnum.Squeeze) return fitness;
        return this.FitnessAlt;
      }
    }

    public float[] Rotation { get; set; }

    public SheetPlacementCollection UsedSheets { get; private set; }

    public IList<NFP> UnplacedParts { get; }

    public int NestIndex { get; }

    /// <summary>
    /// Area of the placement sheet.
    /// </summary>
    public double area { get; }

    public double mergedLength { get; }

    public double FitnessSheets { get; }

    public double FitnessBounds { get; }

    public double FitnessUnplaced { get; }

    internal int index { get; set; }

    public PlacementTypeEnum PlacementType { get; }
  }
}
