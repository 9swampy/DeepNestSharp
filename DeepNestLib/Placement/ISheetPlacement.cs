namespace DeepNestLib.Placement
{
  using System.Collections.Generic;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Geometry;

  public interface ISheetPlacement : IMinMaxXY
  {
    OriginalFitnessSheet Fitness { get; }

    INfp Hull { get; }

    double MaterialUtilization { get; }

    IReadOnlyList<IPartPlacement> PartPlacements { get; }

    PlacementTypeEnum PlacementType { get; }

    PolygonBounds RectBounds { get; }

    ISheet Sheet { get; }

    int SheetId { get; }

    int SheetSource { get; }

    INfp Simplify { get; }

    double TotalPartsArea { get; }

    double MergedLength { get; }

    string ToJson(bool writeIndented = false);

    string ToString();
  }
}