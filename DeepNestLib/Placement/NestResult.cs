namespace DeepNestLib.Placement
{
  using System;
  using System.Collections.Generic;
  using DeepNestLib.GeneticAlgorithm;

  public class NestResult : INestResult
  {
    private double fitnessAlt = -1;

    [Obsolete("Exposed only for interim UnitTest comparisons of old with replacement.")]
    public double? fitness = null;

    public NestResult(
      int nestIndex,
      double area,
      SheetPlacementCollection allplacements,
      IList<NFP> unplacedParts,
      double? fitness,
      double mergedLength,
      double fitnessSheets,
      double fitnessBounds,
      double fitnessUnplaced,
      PlacementTypeEnum placementType,
      long placePartTime)
    {
      UsedSheets = allplacements;
      this.UnplacedParts = unplacedParts;
#pragma warning disable CS0618 // Type or member is obsolete
      this.fitness = fitness;
#pragma warning restore CS0618 // Type or member is obsolete
      this.NestIndex = nestIndex;
      this.Area = area;
      this.MergedLength = mergedLength;
      this.FitnessSheets = fitnessSheets;
      this.FitnessBounds = fitnessBounds;
      this.FitnessUnplaced = fitnessUnplaced;
      this.PlacementType = placementType;
      this.PlacePartTime = placePartTime;
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
    public double Area { get; }

    public double MergedLength { get; }

    public double FitnessSheets { get; }

    public double FitnessBounds { get; }

    public double FitnessUnplaced { get; }

    internal int index { get; set; }

    public PlacementTypeEnum PlacementType { get; }

    public long PlacePartTime { get; }
  }
}
