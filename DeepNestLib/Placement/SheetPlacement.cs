namespace DeepNestLib.Placement
{
  using System.Collections.Generic;
  using System.Linq;

  /// <summary>
  /// Represents a sheet that has had parts placed on it in the nest.
  /// </summary>
  public class SheetPlacement
  {
    public SheetPlacement(NFP sheet, List<PartPlacement> partPlacements)
    {
      this.SheetId = sheet.Id;
      this.SheetSource = sheet.Source;
      this.Sheet = sheet;
      this.PartPlacements = partPlacements;
    }

    /// <summary>
    /// Gets memoised sheet.Id; to maintain legacy - monitor if sheet.Id is ever getting updated (may be Liskov breach in Sheet?).
    /// </summary>
    public int SheetId { get; }

    /// <summary>
    /// Gets memoised sheet.Source; to maintain legacy - monitor if sheet.Id is ever getting updated (may be Liskov breach in Sheet?).
    /// </summary>
    public int SheetSource { get; }

    public NFP Sheet { get; }

    public List<PartPlacement> PartPlacements { get; } = new List<PartPlacement>();

    public PolygonBounds RectBounds
    {
      get
      {
        return CombinedRectBounds(this.PartPlacements);
      }
    }

    internal static PolygonBounds CombinedRectBounds(List<PartPlacement> partPlacements)
    {
      NFP allpoints = CombinedPoints(partPlacements);
      return GeometryUtil.getPolygonBounds(allpoints);
    }

    internal static NFP CombinedPoints(List<PartPlacement> partPlacements)
    {
      NFP allpoints = new NFP();
      for (int partIndex = 0; partIndex < partPlacements.Count; partIndex++)
      {
        var length = partPlacements[partIndex].Part.Points.Length;
        for (int pointIndex = 0; pointIndex < length; pointIndex++)
        {
          var part = partPlacements[partIndex].Part.Points[pointIndex];
          var placement = partPlacements[partIndex];
          allpoints.AddPoint(
              new SvgPoint(
               part.x + placement.x,
               part.y + placement.y));
        }
      }

      return allpoints;
    }
  }
}
