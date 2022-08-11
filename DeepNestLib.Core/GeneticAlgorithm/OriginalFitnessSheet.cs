namespace DeepNestLib.GeneticAlgorithm
{
  using System;
  using DeepNestLib.Placement;

  public class OriginalFitnessSheet : ISheetPlacementFitness
  {
    private static volatile object syncLock = new object();

    private readonly ISheetPlacement sheetPlacement;
    private double? wasted;
    private double? sheets;
    private double? bounds;
    private double? utilization;

    public OriginalFitnessSheet(ISheetPlacement sheetPlacement)
    {
      this.sheetPlacement = sheetPlacement;
    }

    public double Total
    {
      get
      {
        var result = 0d;
        result += Bounds;
        result += Sheets;
        result += Wasted;
        result += Utilization;

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
    public double Wasted
    {
      get
      {
        lock (syncLock)
        {
          if (!wasted.HasValue)
          {
            var rectBounds = sheetPlacement.RectBounds;
            var utilization = sheetPlacement.TotalPartsArea / rectBounds.Area;
            var wastage = 1 - utilization;
            if (wastage == 0)
            {
              wastage = Math.Pow(1 - (sheetPlacement.TotalPartsArea / sheetPlacement.Sheet.Area), 2);
            }

            wasted = Math.Min(rectBounds.Area * 2, sheetPlacement.Sheet.Area);
            wasted += sheetPlacement.Hull.Area + rectBounds.Area;
            wasted /= 3;

            wasted = Math.Max(0, wasted.Value * wastage * 4);
            if (wasted > Sheets)
            {
              wasted = Sheets;
            }
          }

          return ScaleBySimpleUtilization(wasted.Value);
        }
      }
    }

    /// <summary>
    /// Penalise low material utilization.
    /// </summary>
    public double Utilization
    {
      get
      {
        lock (syncLock)
        {
          if (!utilization.HasValue)
          {
            utilization = (double)Math.Pow(1 - this.sheetPlacement.MaterialUtilization, 1.2) * sheetPlacement.Sheet.Area;
            if (!utilization.HasValue || double.IsNaN(utilization.Value))
            {
              utilization = sheetPlacement.Sheet.Area;
            }

            if (this.sheetPlacement.MaterialUtilization <= 0.1)
            {
              var altUtilization = Math.Pow(this.Wasted / 10, 2);
              altUtilization += Bounds * 10;
              utilization = Math.Min(altUtilization, utilization.Value * .9);
              if (this.sheetPlacement.MaterialUtilization <= 0.04)
              {
                utilization /= Math.Min(3, sheetPlacement.PartPlacements.Count);
              }
            }
          }

          return ScaleBySimpleUtilization(utilization.Value);
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
        try
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
                bound = rectBounds.Width / sheetPlacement.Sheet.WidthCalculated * sheetPlacement.Sheet.Area;
              }
              else
              {
                area = rectBounds.Width * rectBounds.Height;
                bound = area;
              }

              bounds = ((bound * 4) + area + sheetPlacement.Hull.Area) / 7;
            }

            return ScaleBySimpleUtilization(this.bounds.Value);
          }
        }
        catch (Exception ex)
        {
          System.Diagnostics.Debug.Print(ex.Message);
          System.Diagnostics.Debug.Print(ex.StackTrace);
          throw;
        }
      }
    }

    public double Evaluate()
    {
      return Total;
    }

    public override string ToString()
    {
      return $"{Total:N0}=B{Bounds:N0}+S{Sheets:N0}+W{Wasted:N0}+U{Utilization:N0}";
    }

    private double ScaleBySimpleUtilization(double value)
    {
      return value * Math.Pow(1 - sheetPlacement.MaterialUtilization, 0.5);
    }
  }
}
