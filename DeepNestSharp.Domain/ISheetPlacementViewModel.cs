namespace DeepNestSharp.Domain
{
  using DeepNestLib.Placement;

  public interface ISheetPlacementViewModel
  {
    ISheetPlacement SheetPlacement { get; }
  }
}