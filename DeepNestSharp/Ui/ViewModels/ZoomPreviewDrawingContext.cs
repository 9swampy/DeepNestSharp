namespace DeepNestSharp.Ui.ViewModels
{
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

    public double Width => sheetPlacement?.Sheet.WidthCalculated ?? 0;

    public double Height => sheetPlacement?.Sheet.HeightCalculated ?? 0;

    public void Set(ISheetPlacement sheetPlacement)
    {
      Set(new ObservableSheetPlacement(sheetPlacement));
    }

    public void Set(ObservableSheetPlacement sheetPlacement)
    {
      this.Clear();
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