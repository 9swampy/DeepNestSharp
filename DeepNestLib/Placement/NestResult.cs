namespace DeepNestLib.Placement
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using DeepNestLib.GeneticAlgorithm;

  public class NestResult : INestResult
  {
    private readonly OriginalFitness fitness;

    public NestResult(
      SheetPlacementCollection allPlacements,
      IList<NFP> unplacedParts,
      double mergedLength,
      PlacementTypeEnum placementType,
      long placePartTime)
    {
      this.UsedSheets = allPlacements;
      this.UnplacedParts = unplacedParts;
      this.MergedLength = mergedLength;
      this.PlacementType = placementType;
      this.PlacePartTime = placePartTime;
      this.fitness = new OriginalFitness(this);
    }

    public DateTime CreatedAt { get; } = DateTime.Now;

    public double Fitness
    {
      get
      {
        return this.fitness.Evaluate();
      }
    }

    public double[] Rotation { get; set; }

    public SheetPlacementCollection UsedSheets { get; private set; }

    public IList<NFP> UnplacedParts { get; }

    public double MergedLength { get; }

    public double FitnessSheets
    {
      get
      {
        return this.fitness.Sheets;
      }
    }

    public double MaterialWasted
    {
      get
      {
        return this.fitness.MaterialWasted;
      }
    }

    public double FitnessBounds
    {
      get
      {
        return this.fitness.Bounds;
      }
    }

    public double FitnessUnplaced
    {
      get
      {
        return this.fitness.Unplaced;
      }
    }

    internal int index { get; set; }

    public PlacementTypeEnum PlacementType { get; }

    public long PlacePartTime { get; }

    public int TotalPlacedCount => this.UsedSheets.Sum(o => o.PartPlacements.Count);

    public int TotalPartsCount => this.TotalPlacedCount + this.UnplacedParts.Count;

    public double PartsPlacedPercent => (double)this.TotalPlacedCount / this.TotalPartsCount;

    public double TotalPartsArea => this.UsedSheets.Sum(s => s.PartPlacements.Sum(p => p.Part.Area));

    public double TotalSheetsArea => this.UsedSheets.Sum(s => s.Sheet.Area);

    public double MaterialUtilization => Math.Abs(TotalPartsArea / TotalSheetsArea);

    public override string ToString()
    {
      return $"{fitness.Evaluate()}=SumB{fitness.Bounds:N0}+SumS{fitness.Sheets:N0}+SumU{fitness.MaterialWasted:N0}+U{fitness.Unplaced:N0}";
    }
  }
}
