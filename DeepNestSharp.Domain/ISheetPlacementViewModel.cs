namespace DeepNestSharp.Domain
{
  using System.ComponentModel;
  using DeepNestLib.Placement;

  public interface ISheetPlacementViewModel : INotifyPropertyChanged
  {
    ISheetPlacement SheetPlacement { get; }

    IPartPlacement SelectedItem { get; }
  }
}