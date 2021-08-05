namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Collections.ObjectModel;
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Ui.Models;

  public class ZoomPreviewDrawingContext : ObservableCollection<object>
  {
    private readonly ISheetPlacement? sheetPlacement;

    public ZoomPreviewDrawingContext()
    {
    }

    public double Width { get; private set; }

    public double Height { get; private set; }

    public void Set(ISheetPlacement sheetPlacement)
    {
      For(sheetPlacement);
    }

    public ZoomPreviewDrawingContext For(ISheetPlacement sheetPlacement)
    {
      Set(new ObservableSheetPlacement(sheetPlacement));
      return this;
    }

    public void Set(ObservableSheetPlacement sheetPlacement)
    {
      For(sheetPlacement);
    }

    public ZoomPreviewDrawingContext For(ObservableSheetPlacement sheetPlacement)
    {
      this.Clear();
      this.Width = sheetPlacement.Sheet.WidthCalculated;
      this.Height = sheetPlacement.Sheet.HeightCalculated;
      this.Add(sheetPlacement);
      foreach (var partPlacement in sheetPlacement.PartPlacements)
      {
        INfp part = partPlacement.Part;
        this.Add(partPlacement);
        foreach (var child in part.Children)
        {
          Set(new ObservableHole((ObservablePartPlacement)partPlacement, Background.ShiftPolygon(child, partPlacement)));
        }
      }

      return this;
    }

    /// <summary>
    /// Adding in children as <see cref="ObservableHoles"/> so can fill differently.
    /// </summary>
    /// <param name="child">Child to add; presumption's it will be a Hole.</param>
    private void Set(ObservableHole child)
    {
      this.Add(child);
      foreach (var c in child.Children)
      {
        Set(new ObservableHole(c));
      }
    }
  }
}