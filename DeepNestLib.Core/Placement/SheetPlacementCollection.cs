namespace DeepNestLib.Placement
{
  using System;
  using System.Linq;
  using System.Text.Json.Serialization;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.NestProject;

  /// <summary>
  /// A collection of SheetPlacements (UsedSheets with Parts placed on them).
  /// </summary>
  public class SheetPlacementCollection : WrappableList<ISheetPlacement, SheetPlacement>, ISheetPlacementFitness
  {
    private double sheets = 0;
    private volatile object syncLock = new object();

    public SheetPlacementCollection()
      : base()
    {
    }

    public SheetPlacementCollection(IList<ISheetPlacement, SheetPlacement> items)
      : base(items)
    {
    }

    [JsonIgnore]
    public double Bounds => this.Sum(o => o.Fitness.Bounds);

    [JsonIgnore]
    public double Utilization => this.Sum(o => o.Fitness.MaterialUtilization);

    [JsonIgnore]
    public double Wasted => this.Sum(o => o.Fitness.MaterialWasted);

    [JsonIgnore]
    public double Sheets
    {
      get
      {
        lock (syncLock)
        {
          if (sheets == 0)
          {
            for (int i = 0; i < this.Count; i++)
            {
              var sheet = this[i];
              sheets += sheet.Fitness.Sheets;
              if (i < this.Count - 1 && this[i + 1].PartPlacements.Any(o => o.Part.IsPriority))
              {
                sheets += sheet.Sheet.Area;
              }
            }
          }

          return sheets;
        }
      }
    }

    public double Total => this.Sum(o => o.Fitness.Total);

    public int TotalPartsPlaced => this.Sum(o => o.PartPlacements.Count);
  }
}