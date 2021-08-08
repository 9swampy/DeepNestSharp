namespace DeepNestLib.Placement
{
  using System;
  using System.Collections.Generic;
  using DeepNestLib.NestProject;

  public interface INestResult
  {
    double Fitness { get; }

    double FitnessBounds { get; }

    double FitnessSheets { get; }

    double FitnessUnplaced { get; }

    double MergedLength { get; }

    PlacementTypeEnum PlacementType { get; }

    long PlacePartTime { get; }

    double[] Rotation { get; set; }

    IList<INfp> UnplacedParts { get; }

    IList<ISheetPlacement, SheetPlacement> UsedSheets { get; }

    int TotalPlacedCount { get; }

    int TotalPartsCount { get; }

    double PartsPlacedPercent { get; }

    double MaterialUtilization { get; }

    double TotalSheetsArea { get; }

    double TotalPartsArea { get; }

    DateTime CreatedAt { get; }

    string ToJson();
  }
}