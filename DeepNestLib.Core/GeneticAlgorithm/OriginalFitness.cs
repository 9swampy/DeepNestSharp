namespace DeepNestLib.GeneticAlgorithm
{
  using System;
  using System.Linq;
  using DeepNestLib.Geometry;
  using DeepNestLib.Placement;

  public class OriginalFitness : ISheetPlacementFitness
  {
    private readonly NestResult nestResult;

    public OriginalFitness(NestResult nestResult)
    {
      this.nestResult = nestResult;
    }

    public double Evaluate()
    {
      var sheetPlacementFitness = (ISheetPlacementFitness)this;
      return sheetPlacementFitness.Total;
    }

    private double TotalSheetArea
    {
      get
      {
        return nestResult.UsedSheets.Sum(o => o.Sheet.Area);
      }
    }

    /// <summary>
    /// Gets total fitness.
    /// </summary>
    double ISheetPlacementFitness.Total
    {
      get
      {
        return ((SheetPlacementCollection)nestResult.UsedSheets).Total + Unplaced;
      }
    }

    /// <summary>
    /// Gets penalty for each additional sheet needed.
    /// </summary>
    double ISheetPlacementFitness.Sheets
    {
      get
      {
        return ((SheetPlacementCollection)nestResult.UsedSheets).Sheets;
      }
    }

    /// <summary>
    /// Gets penalty for low material utilization.
    /// </summary>
    double ISheetPlacementFitness.Utilization
    {
      get
      {
        return ((SheetPlacementCollection)nestResult.UsedSheets).Utilization;
      }
    }

    /// <summary>
    /// Gets penalty for high material wastage; weighted to reward compression within the part of the sheet used.
    /// </summary>
    double ISheetPlacementFitness.Wasted
    {
      get
      {
        return ((SheetPlacementCollection)nestResult.UsedSheets).Wasted;
      }
    }

    /// <summary>
    /// Gets penalty for bounds of sheet used; for Gravity prefer left squeeze; for BoundingBox a smaller Bound; Squeeze tbc.
    /// </summary>
    double ISheetPlacementFitness.Bounds
    {
      get
      {
        return ((SheetPlacementCollection)nestResult.UsedSheets).Bounds;
      }
    }

    /// <summary>
    /// Get overweighted penalty for unplaced parts so an additional sheet will always get added when needed.
    /// </summary>
    public double Unplaced
    {
      get
      {
        var result = nestResult.UnplacedParts.Sum(o => 50 * Math.Abs(GeometryUtil.PolygonArea(o)));
        if (nestResult.UnplacedParts.Any(o => o.IsPriority))
        {
          result *= 2;
        }

        return result;
      }
    }
  }
}
