namespace DeepNestLib.Placement
{
  using System.Collections.Generic;
  using System.Collections.ObjectModel;

  /// <summary>
  /// A collection of SheetPlacements (UsedSheets with Parts placed on them).
  /// </summary>
  public class SheetPlacementCollection : ReadOnlyCollection<SheetPlacement>
  {
    public SheetPlacementCollection()
      : base(new List<SheetPlacement>())
    {
    }

    public void Add(SheetPlacement item)
    {
      this.Items.Add(item);
    }
  }
}
