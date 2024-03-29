﻿namespace DeepNestLib.Placement
{
  using System;
  using System.Collections.Generic;
  using DeepNestLib.NestProject;

  public interface INestResult
  {
    DateTime CreatedAt { get; }

    double Fitness { get; }

    double FitnessBounds { get; }

    double FitnessSheets { get; }

    double FitnessUnplaced { get; }

    bool IsValid { get; }

    double MergedLength { get; }

    PlacementTypeEnum PlacementType { get; }

    long PlacePartTime { get; }

    double[] Rotation { get; set; }

    int TotalParts { get; }

    IList<INfp> UnplacedParts { get; }

    IList<ISheetPlacement, SheetPlacement> UsedSheets { get; }

    int TotalPlacedCount { get; }

    int TotalPartsCount { get; }

    double PartsPlacedPercent { get; }

    double MaterialUtilization { get; }

    double TotalSheetsArea { get; }

    double TotalPartsArea { get; }

    string ToJson(bool writeIndented = false);
  }
}