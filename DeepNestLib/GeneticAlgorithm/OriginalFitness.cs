namespace DeepNestLib.GeneticAlgorithm
{
  using System;
  using System.Linq;
  using DeepNestLib.Placement;

  public class OriginalFitness
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
      result += Bounds;
      result += Sheets;
      result += MaterialWasted;
      result += MaterialUtilization;

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
    /// Penalise for each additional sheet needed.
    /// </summary>
    internal double Sheets
    {
      get
      {
        return nestResult.UsedSheets.Sum(o => o.Fitness.Sheets);
      }
    }

    /// <summary>
    /// Penalise low material utilization.
    /// </summary>
    internal double MaterialUtilization
    {
      get
      {
        return nestResult.UsedSheets.Sum(o => o.Fitness.MaterialUtilization);
      }
    }

    /// <summary>
    /// Penalise high material wastage; weighted to reward compression within the part of the sheet used.
    /// </summary>
    internal double MaterialWasted
    {
      get
      {
        return nestResult.UsedSheets.Sum(o => o.Fitness.MaterialWasted);
      }
    }

    /// <summary>
    /// For Gravity prefer left squeeze; BoundingBox the smaller Bound; Squeeze tbc.
    /// </summary>
    internal double Bounds
    {
      get
      {
        return nestResult.UsedSheets.Sum(o => o.Fitness.Bounds);
      }
    }

    /// <summary>
    /// Huge penalty for unplaced parts so an additional sheet will always get added if needed.
    /// </summary>
    internal double Unplaced
    {
      get
      {
        var result = nestResult.UnplacedParts.Sum(o => 100000000 * (Math.Abs(GeometryUtil.polygonArea(o)) / TotalSheetArea));
        if (nestResult.UnplacedParts.Any(o => o.IsPriority))
        {
          result *= 2;
        }

        return result;
      }
    }
  }
}
