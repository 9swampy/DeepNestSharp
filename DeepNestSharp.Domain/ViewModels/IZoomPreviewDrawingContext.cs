namespace DeepNestSharp.Ui.ViewModels
{
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Models;

  public interface IZoomPreviewDrawingContext
  {
    double Height { get; }
    double Width { get; }

    IZoomPreviewDrawingContext For(ISheetPlacement sheetPlacement);
    IZoomPreviewDrawingContext For(ObservableSheetPlacement sheetPlacement);
    void Set(ISheetPlacement sheetPlacement);
    void Set(ObservableSheetPlacement sheetPlacement);
    void Clear();
  }
}