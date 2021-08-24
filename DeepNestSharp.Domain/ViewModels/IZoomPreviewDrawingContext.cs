namespace DeepNestSharp.Domain.ViewModels
{
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Models;
  using System.Collections.Generic;

  public interface IZoomPreviewDrawingContext : ICollection<object>, IList<object>  //, INotifyCollectionChanged, INotifyPropertyChanged
  {
    double Height { get; }
    double Width { get; }

    IZoomPreviewDrawingContext For(ISheetPlacement sheetPlacement);

    IZoomPreviewDrawingContext For(ObservableSheetPlacement sheetPlacement);

    IZoomPreviewDrawingContext For(ObservableNfp nfp);

    void Set(ISheetPlacement sheetPlacement);

    void Set(ObservableSheetPlacement sheetPlacement);

    void Clear();

    void AppendChild(ObservableFrame observableFrame);

    void AppendChild(ObservablePoint observablePoint);

    void AppendChild(ObservableHole child);
  }
}