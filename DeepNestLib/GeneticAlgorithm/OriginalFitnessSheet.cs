namespace DeepNestLib.GeneticAlgorithm
{
  using System;
  using System.Linq;
  using DeepNestLib.Placement;

  public class OriginalFitnessSheet : IOriginalFitnessSheet
  {
    private static volatile object syncLock = new object();

    private readonly ISheetPlacement sheetPlacement;
    private double? materialWasted;
    private double? sheets;
    private double? bounds;
    private double? materialUtilization;

    public OriginalFitnessSheet(ISheetPlacement sheetPlacement)
    {
      this.sheetPlacement = sheetPlacement;
    }

    public double Evaluate()
    {
      var result = 0D;
      result += Bounds;
      result += Sheets;
      result += MaterialWasted;
      result += MaterialUtilization;

      return result;
    }

    private float TotalSheetArea
    {
      get
      {
        return sheetPlacement.Sheet.Area;
      }
    }

    /// <summary>
    /// Penalise for each additional sheet needed.
    /// </summary>
    public double Sheets
    {
      get
      {
        lock (syncLock)
        {
          if (!sheets.HasValue)
          {
            sheets = sheetPlacement.Sheet.Area;
            if (sheetPlacement.PartPlacements.Any(p => p.Part.IsPriority))
            {
              sheets += sheetPlacement.Sheet.Area;
            }
          }
        }

        return sheets.Value;
      }
    }

    /// <summary>
    /// Penalise high material wastage; weighted to reward compression within the part of the sheet used.
    /// </summary>
    public double MaterialWasted
    {
      get
      {
        lock (syncLock)
        {
          if (!materialWasted.HasValue)
          {
            var rectBounds = sheetPlacement.RectBounds;
            materialWasted = sheetPlacement.MaterialUtilization < 0.6 ? rectBounds.width * rectBounds.height * 2 : sheetPlacement.Sheet.Area;
            materialWasted += sheetPlacement.Hull.Area + (rectBounds.width * rectBounds.height);
            materialWasted *= 2;
            materialWasted -= sheetPlacement.MaterialUtilization < 0.6 ? 7 : 6 * sheetPlacement.TotalPartsArea;
            if (sheetPlacement.MaterialUtilization < 0.2)
            {
              materialWasted *= 3;
            }

            materialWasted = Math.Max(0, materialWasted.Value);
          }
        }

        return materialWasted.Value;
      }
    }

    /// <summary>
    /// Penalise low material utilization.
    /// </summary>
    public double MaterialUtilization
    {
      get
      {
        lock (syncLock)
        {
          if (!materialUtilization.HasValue)
          {
            materialUtilization = Math.Pow((double)(1 - this.sheetPlacement.MaterialUtilization), 1.1D) * sheetPlacement.Sheet.Area;
          }
        }

        return materialUtilization.Value;
      }
    }

    /// <summary>
    /// For Gravity prefer left squeeze; BoundingBox the smaller Bound; Squeeze tbc.
    /// </summary>
    public double Bounds
    {
      get
      {
        lock (syncLock)
        {
          if (this.bounds == null)
          {
            double area;
            var rectBounds = sheetPlacement.RectBounds;
            if (sheetPlacement.PlacementType == PlacementTypeEnum.Gravity)
            {
              area = (rectBounds.width * 3) * rectBounds.height;
            }
            else
            {
              area = rectBounds.width * rectBounds.height;
            }

            bounds = (((rectBounds.width * 2) / sheetPlacement.Sheet.Area) + area + sheetPlacement.Hull.Area) / 6;
          }
        }

        return this.bounds.Value;
      }
    }

    public override string ToString()
    {
      return $"{Evaluate():N0}=B{Bounds:N0}+S{Sheets:N0}+W{MaterialWasted:N0}+U{MaterialUtilization:N0}";
    }
  }
}
