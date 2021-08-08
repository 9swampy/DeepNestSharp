namespace DeepNestLib.Placement
{
  using System;
  using System.Collections.Generic;

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

    IList<NFP> UnplacedParts { get; }

    SheetPlacementCollection UsedSheets { get; }

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