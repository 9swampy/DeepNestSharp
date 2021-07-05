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

      return result;
    }

    private static float GetTotalSheetArea(NestResult nestResult)
    {
      return nestResult.UsedSheets.Sum(o => o.Sheet.Area);
    }

    internal static double FitnessSheets(NestResult nestResult)
    {
      return nestResult.UsedSheets.Sum(o => o.Sheet.Area);
    }

    internal static double FitnessBounds(NestResult nestResult)
    {
      return nestResult.UsedSheets.Sum(o =>
      {
        double area;
        if (nestResult.PlacementType == PlacementTypeEnum.Gravity)
        {
          area = (o.RectBounds.width * 3) + o.RectBounds.height;
        }
        else
        {
          area = o.RectBounds.width * o.RectBounds.height;
        }

        return (o.RectBounds.width / o.Sheet.Area) + area;
      });
    }

    internal static double FitnessUnplaced(NestResult nestResult)
    {
      return nestResult.UnplacedParts.Sum(o => 100000000 * (Math.Abs(GeometryUtil.polygonArea(o)) / OriginalFitness.GetTotalSheetArea(nestResult)));
    }
  }
}
