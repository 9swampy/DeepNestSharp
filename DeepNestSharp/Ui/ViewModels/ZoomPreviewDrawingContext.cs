namespace DeepNestSharp.Ui.ViewModels
{
  using System.Collections.ObjectModel;
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Models;

  public class ZoomPreviewDrawingContext : ObservableCollection<object>, IZoomPreviewDrawingContext
  {
    public double Width { get; private set; }

    public double Height { get; private set; }

    public void Set(ISheetPlacement sheetPlacement)
    {
      For(sheetPlacement);
    }

    public IZoomPreviewDrawingContext For(ISheetPlacement sheetPlacement)
    {
      Set(new ObservableSheetPlacement((SheetPlacement)sheetPlacement));
      return this;
    }

    public void Set(ObservableSheetPlacement sheetPlacement)
    {
      For(sheetPlacement);
    }

    public IZoomPreviewDrawingContext For(ObservableSheetPlacement sheetPlacement)
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
          AppendChild(new ObservableHole(child.Shift(partPlacement)));
        }
      }

      return this;
    }

    internal ZoomPreviewDrawingContext For(INfp part)
    {
      For(new ObservableNfp(part));
      return this;
    }

    internal ZoomPreviewDrawingContext For(ObservableNfp part)
    {
      this.Clear();
      this.Width = part.WidthCalculated;
      this.Height = part.HeightCalculated;
      this.Add(part);
      foreach (var c in part.Children)
      {
        AppendChild(c);
      }

      return this;
    }

    private void AppendChild(INfp c)
    {
      if (c is ObservableHole observableHole)
      {
        AppendChild(observableHole);
      }
      else if (c is ObservablePoint observablePoint)
      {
        AppendChild(observablePoint);
      }
      else
      {
        AppendChild(new ObservableHole(c));
      }
    }

    /// <summary>
    /// Adding in children as <see cref="ObservableHoles"/> so can fill differently.
    /// </summary>
    /// <param name="child">Child to add; presumption's it will be a Hole.</param>
    private void AppendChild(ObservableHole child)
    {
      this.Add(child);
      foreach (var c in child.Children)
      {
        AppendChild(new ObservableHole(c));
      }
    }

    /// <summary>
    /// Adding in children as <see cref="ObservablePoints"/> so can fill differently.
    /// </summary>
    /// <param name="child">Child to add; presumption's it will be a Point.</param>
    internal void AppendChild(ObservablePoint child)
    {
      this.Add(child);
    }

    /// <summary>
    /// Adding in children as <see cref="ObservableFrame"/> so can fill differently.
    /// </summary>
    /// <param name="child">Child to add; presumption's it will be a Frame.</param>
    internal void AppendChild(ObservableFrame child)
    {
      this.Add(child);
    }
  }
}