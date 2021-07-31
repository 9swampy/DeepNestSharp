namespace DeepNestLib.Placement
{
  using System.Collections.Generic;
  using DeepNestLib.GeneticAlgorithm;

  public interface ISheetPlacement : IMinMaxXY
  {
    OriginalFitnessSheet Fitness { get; }

    INfp Hull { get; }

    double MaterialUtilization { get; }

    IReadOnlyList<IPartPlacement> PartPlacements { get; }

    PlacementTypeEnum PlacementType { get; }

    PolygonBounds RectBounds { get; }

    INfp Sheet { get; }

    int SheetId { get; }

    int SheetSource { get; }

    INfp Simplify { get; }

    double TotalPartsArea { get; }

    string ToJson();

    string ToString();
  }
}