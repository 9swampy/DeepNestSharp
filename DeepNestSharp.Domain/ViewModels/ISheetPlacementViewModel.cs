namespace DeepNestSharp.Domain.ViewModels
{
  using System.ComponentModel;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Docking;

  public interface ISheetPlacementViewModel : INotifyPropertyChanged, IFileViewModel
  {
    ISheetPlacement SheetPlacement { get; }

    IPartPlacement SelectedItem { get; set; }

    void RaiseDrawingContext();
  }
}