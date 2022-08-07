namespace DeepNestLib.Placement
{
  using System.Collections.Generic;
  using System.IO;
  using System.Threading.Tasks;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Geometry;

  public interface ISheetPlacement : IMinMaxXY
  {
    OriginalFitnessSheet Fitness { get; }

    INfp Hull { get; }

    /// <summary>
    /// Gets the simple area percentage used.
    /// </summary>
    double MaterialUtilization { get; }

    IReadOnlyList<IPartPlacement> PartPlacements { get; }

    IEnumerable<NoFitPolygon> PolygonsForExport { get; }

    PlacementTypeEnum PlacementType { get; }

    PolygonBounds RectBounds { get; }

    ISheet Sheet { get; }

    int SheetId { get; }

    int SheetSource { get; }

    INfp Simplify { get; }

    /// <summary>
    /// Gets the total gross outer area, discounting for any holes.
    /// </summary>
    double TotalPartsArea { get; }

    double MergedLength { get; }

    Task ExportDxf(Stream stream, bool mergeLines, bool differentiateChildren);

    string ToJson(bool writeIndented = false);

    string ToString();
  }
}