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
      var result = 0D;
      result += Unplaced;
      var sheetPlacementFitness = (ISheetPlacementFitness)this;
      result += sheetPlacementFitness.Bounds;
      result += sheetPlacementFitness.Sheets;
      result += sheetPlacementFitness.MaterialWasted;
      result += sheetPlacementFitness.MaterialUtilization;

      return result;
    }

    private double TotalSheetArea
    {
      get
      {
        return nestResult.UsedSheets.Sum(o => o.Sheet.Area);
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
    double ISheetPlacementFitness.MaterialUtilization
    {
      get
      {
        return ((SheetPlacementCollection)nestResult.UsedSheets).MaterialUtilization;
      }
    }

    /// <summary>
    /// Gets penalty for high material wastage; weighted to reward compression within the part of the sheet used.
    /// </summary>
    double ISheetPlacementFitness.MaterialWasted
    {
      get
      {
        return ((SheetPlacementCollection)nestResult.UsedSheets).MaterialWasted;
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
