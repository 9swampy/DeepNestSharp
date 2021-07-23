namespace DeepNestLib.Placement
{
  using System.Collections.Generic;
  using DeepNestLib.GeneticAlgorithm;

  public interface ISheetPlacement
  {
    OriginalFitnessSheet Fitness { get; }

    INfp Hull { get; }

    float MaterialUtilization { get; }

    IList<PartPlacement> PartPlacements { get; }

    PlacementTypeEnum PlacementType { get; }

    PolygonBounds RectBounds { get; }

    INfp Sheet { get; }

    int SheetId { get; }

    int SheetSource { get; }

    INfp Simplify { get; }

    float TotalPartsArea { get; }

    string ToJson();

    string ToString();
  }
}