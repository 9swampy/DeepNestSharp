namespace DeepNestLib.GeneticAlgorithm
{
  using System;
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
      return Total;
    }

    public double Total
    {
      get
      {
        var result = 0d;
        result += Bounds;
        result += Sheets;
        result += MaterialWasted;
        result += MaterialUtilization;

        return result;
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
          }

          return sheets.Value;
        }
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
            materialWasted = sheetPlacement.MaterialUtilization < 0.6 ? rectBounds.Width * rectBounds.Height * 2 : sheetPlacement.Sheet.Area;
            materialWasted += sheetPlacement.Hull.Area + (rectBounds.Width * rectBounds.Height);
            materialWasted *= 2;
            materialWasted -= sheetPlacement.MaterialUtilization < 0.6 ? 7 : 6 * sheetPlacement.TotalPartsArea;
            if (sheetPlacement.MaterialUtilization < 0.3)
            {
              materialWasted *= 1.25;
            }

            materialWasted = Math.Max(0, materialWasted.Value / 2);
          }

          return materialWasted.Value;
        }
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
            materialUtilization = (double)Math.Pow(1 - this.sheetPlacement.MaterialUtilization, 1.1) * sheetPlacement.Sheet.Area;
            if (!materialUtilization.HasValue || double.IsNaN(materialUtilization.Value))
            {
              materialUtilization = sheetPlacement.Sheet.Area;
            }
          }

          return materialUtilization.Value;
        }
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
          if (!this.bounds.HasValue)
          {
            double area;
            var rectBounds = sheetPlacement.RectBounds;
            double bound;
            if (sheetPlacement.PlacementType == PlacementTypeEnum.Gravity)
            {
              area = Math.Pow(((rectBounds.Width * 3) + rectBounds.Height) / 4, 2);
              var sheetWidth = sheetPlacement.Sheet.WidthCalculated; // == 0 ? sheetPlacement.Sheet.WidthCalculated : sheetPlacement.Sheet.Width;
              bound = rectBounds.Width / sheetWidth * sheetPlacement.Sheet.Area;
            }
            else
            {
              area = rectBounds.Width * rectBounds.Height;
              bound = area;
            }

            bounds = ((bound * 4) + area + sheetPlacement.Hull.Area) / 7;
          }

          return this.bounds.Value;
        }
      }
    }

    public override string ToString()
    {
      return $"{Evaluate():N0}=B{Bounds:N0}+S{Sheets:N0}+W{MaterialWasted:N0}+U{MaterialUtilization:N0}";
    }
  }
}
