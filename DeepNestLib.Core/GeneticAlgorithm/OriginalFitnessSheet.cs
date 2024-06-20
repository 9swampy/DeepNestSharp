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
      return this.Total;
    }

    public double Total
    {
      get
      {
        var result = 0d;
        result += this.Bounds;
        result += this.Sheets;
        result += this.MaterialWasted;
        result += this.MaterialUtilization;

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
          if (!this.sheets.HasValue)
          {
            this.sheets = this.sheetPlacement.Sheet.Area;
          }

          return this.sheets.Value;
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
          if (!this.materialWasted.HasValue)
          {
            Geometry.PolygonBounds rectBounds = this.sheetPlacement.RectBounds;
            this.materialWasted = this.sheetPlacement.MaterialUtilization < 0.6 ? rectBounds.Width * rectBounds.Height * 2 : this.sheetPlacement.Sheet.Area;
            this.materialWasted += this.sheetPlacement.Hull.Area + (rectBounds.Width * rectBounds.Height);
            this.materialWasted *= 2;
            this.materialWasted -= this.sheetPlacement.MaterialUtilization < 0.6 ? 7 : 6 * this.sheetPlacement.TotalPartsArea;
            if (this.sheetPlacement.MaterialUtilization < 0.3)
            {
              this.materialWasted *= 1.25;
            }

            this.materialWasted = Math.Max(0, this.materialWasted.Value / 2);
          }

          return this.materialWasted.Value;
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
          if (!this.materialUtilization.HasValue)
          {
            this.materialUtilization = (double)Math.Pow(1 - this.sheetPlacement.MaterialUtilization, 1.1) * this.sheetPlacement.Sheet.Area;
            if (!this.materialUtilization.HasValue || double.IsNaN(this.materialUtilization.Value))
            {
              this.materialUtilization = this.sheetPlacement.Sheet.Area;
            }
          }

          return this.materialUtilization.Value;
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
              Geometry.PolygonBounds rectBounds = this.sheetPlacement.RectBounds;
              double bound;
              if (this.sheetPlacement.PlacementType == PlacementTypeEnum.Gravity)
              {
                area = Math.Pow(((rectBounds.Width * 3) + rectBounds.Height) / 4, 2);
                bound = rectBounds.Width / this.sheetPlacement.Sheet.WidthCalculated * this.sheetPlacement.Sheet.Area;
              }
              else
              {
                area = rectBounds.Width * rectBounds.Height;
                bound = area;
              }

              this.bounds = ((bound * 4) + area + this.sheetPlacement.Hull.Area) / 7;
            }

            return this.bounds.Value;
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

    public override string ToString()
    {
      return $"{this.Evaluate():N0}=B{this.Bounds:N0}+S{this.Sheets:N0}+W{this.MaterialWasted:N0}+U{this.MaterialUtilization:N0}";
    }
  }
}
