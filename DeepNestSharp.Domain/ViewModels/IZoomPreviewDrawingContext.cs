namespace DeepNestSharp.Domain.ViewModels
{
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Models;
  using System.Collections.Generic;

  public interface IZoomPreviewDrawingContext : ICollection<object>, IList<object>  //, INotifyCollectionChanged, INotifyPropertyChanged
  {
    double Height { get; }

    double Width { get; }

    /// <summary>
    /// Clear and set for SheetPlacement including Parts with their children as Holes.
    /// </summary>
    /// <param name="sheetPlacement"></param>
    /// <returns></returns>
    IZoomPreviewDrawingContext For(ISheetPlacement sheetPlacement);

    /// <summary>
    /// Clear and set for SheetPlacement including Parts with their children as Holes.
    /// </summary>
    /// <param name="sheetPlacement"></param>
    /// <returns></returns>
    IZoomPreviewDrawingContext For(ObservableSheetPlacement sheetPlacement);

    /// <summary>
    /// Clear and set for Parts with children as Holes/Frames/Points.
    /// </summary>
    /// <param name="nfp"></param>
    /// <returns></returns>
    IZoomPreviewDrawingContext For(ObservableNfp nfp);

    /// <summary>
    /// Adding in children as <see cref="ObservableFrame"/> so can fill differently.
    /// </summary>
    /// <param name="child">Child to add; presumption's it will be a Frame.</param>
    void AppendChild(ObservableFrame observableFrame);

    /// <summary>
    /// Adding in children as <see cref="ObservablePoints"/> so can fill differently.
    /// </summary>
    /// <param name="child">Child to add; presumption's it will be a Point.</param>
    void AppendChild(ObservablePoint observablePoint);

    /// <summary>
    /// Adding in children as <see cref="ObservableHoles"/> so can fill differently.
    /// </summary>
    /// <param name="child">Child to add; presumption's it will be a Hole.</param>
    void AppendChild(ObservableHole child);
  }
}