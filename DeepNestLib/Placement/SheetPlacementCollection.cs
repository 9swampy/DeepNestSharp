namespace DeepNestLib.Placement
{
  using DeepNestLib.GeneticAlgorithm;
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Linq;

  /// <summary>
  /// A collection of SheetPlacements (UsedSheets with Parts placed on them).
  /// </summary>
  public class SheetPlacementCollection : ReadOnlyCollection<SheetPlacement>, ISheetPlacementFitness
  {
    private double sheets = 0;

    public SheetPlacementCollection()
      : base(new List<SheetPlacement>())
    {
    }

    public double Bounds => this.Items.Sum(o => o.Fitness.Bounds);

    public double MaterialUtilization => this.Items.Sum(o => o.Fitness.MaterialUtilization);

    public double MaterialWasted => this.Items.Sum(o => o.Fitness.MaterialWasted);

    public double Sheets
    {
      get
      {
        if (sheets == 0)
        {
          for (int i = 0; i < Items.Count; i++)
          {
            var sheet = Items[i];
            sheets += sheet.Fitness.Sheets;
            if (i < Items.Count - 1 && Items[i + 1].PartPlacements.Any(o => o.Part.IsPriority))
            {
              sheets += sheet.Sheet.Area;
            }
          }
        }

        return sheets;
      }
    }

    public void Add(SheetPlacement item)
    {
      this.Items.Add(item);
    }
    internal void Remove(SheetPlacement sheetPlacement)
    {
      this.Items.Remove(sheetPlacement);
    }
  }
}
