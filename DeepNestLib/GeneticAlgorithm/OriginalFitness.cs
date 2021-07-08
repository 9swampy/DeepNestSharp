namespace DeepNestLib.GeneticAlgorithm
{
  using System;
  using System.Linq;
  using DeepNestLib.Placement;

  public class OriginalFitness
  {
    public double Evaluate(NestResult nestResult)
    {
      var result = 0D;
      result += FitnessUnplaced(nestResult);
      result += FitnessBounds(nestResult);
      result += FitnessSheets(nestResult);
      result += FitnessMaterialUtilization(nestResult);

      return result;
    }

    private static float GetTotalSheetArea(NestResult nestResult)
    {
      return nestResult.UsedSheets.Sum(o => o.Sheet.Area);
    }

    /// <summary>
    /// Penalise for each additional sheet needed.
    /// </summary>
    /// <param name="nestResult"></param>
    /// <returns></returns>
    internal static double FitnessSheets(NestResult nestResult)
    {
      return nestResult.UsedSheets.Sum(o => o.Sheet.Area);
    }

    /// <summary>
    /// Penalise low material usage.
    /// </summary>
    /// <param name="nestResult"></param>
    /// <returns></returns>
    internal static double FitnessMaterialUtilization(NestResult nestResult)
    {
      return nestResult.UsedSheets.Sum(s => ((s.Sheet.Area - s.PartPlacements.Sum(p => p.Part.Area)) / s.Sheet.Area));
    }

    /// <summary>
    /// For Gravity prefer left squeeze; BoundingBox the smaller Bound; Squeeze tbc.
    /// </summary>
    /// <param name="nestResult"></param>
    /// <returns></returns>
    internal static double FitnessBounds(NestResult nestResult)
    {
      return nestResult.UsedSheets.Sum(o =>
      {
        double area;
        if (nestResult.PlacementType == PlacementTypeEnum.Gravity)
        {
          area = (o.RectBounds.width * 2) * o.RectBounds.height;
        }
        else
        {
          area = o.RectBounds.width * o.RectBounds.height;
        }

        return (o.RectBounds.width / o.Sheet.Area) + area;
      });
    }

    /// <summary>
    /// Huge penalty for unplaced parts so an additional sheet will always get added if needed.
    /// </summary>
    /// <param name="nestResult"></param>
    /// <returns></returns>
    internal static double FitnessUnplaced(NestResult nestResult)
    {
      return nestResult.UnplacedParts.Sum(o => 100000000 * (Math.Abs(GeometryUtil.polygonArea(o)) / OriginalFitness.GetTotalSheetArea(nestResult)));
    }
  }
}
