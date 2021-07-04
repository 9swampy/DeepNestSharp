namespace DeepNestLib.Placement
{
  using System.Collections.Generic;

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
  }
}
