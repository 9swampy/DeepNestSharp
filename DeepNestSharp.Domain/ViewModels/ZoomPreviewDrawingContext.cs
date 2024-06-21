namespace DeepNestSharp.Domain.ViewModels
{
  using System;
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
      this.For(sheetPlacement);
    }

    public IZoomPreviewDrawingContext For(ISheetPlacement sheetPlacement)
    {
      return this.For(new ObservableSheetPlacement((SheetPlacement)sheetPlacement));
    }

    /// <inheritdoc />
    public IZoomPreviewDrawingContext For(ObservableSheetPlacement sheetPlacement)
    {
      this.Clear();
      // Width and Height should include the offset of the sheet in order for the sheet to be centered on the canvas (either that, or the sheets should be substituted with their originals and their Width/HeightCalculated). remove before flight.
      this.Width = sheetPlacement.OriginalSheet.WidthCalculated;
      this.Height = sheetPlacement.OriginalSheet.HeightCalculated;
      this.Add(sheetPlacement);
      foreach (IPartPlacement partPlacement in sheetPlacement.PartPlacements)
      {
        INfp part = partPlacement.Part;
        this.Add(partPlacement);
        foreach (INfp child in part.Children)
        {
          this.AppendChild(new ObservableHole(child.Shift(partPlacement)));
        }
      }

      return this;
    }

    /// <inheritdoc />
    public IZoomPreviewDrawingContext For(INfp part)
    {
      this.For(new ObservableNfp(part));
      return this;
    }

    /// <inheritdoc />
    public IZoomPreviewDrawingContext For(ObservableNfp part)
    {
      this.Clear();
      this.Width = part.WidthCalculated;
      this.Height = part.HeightCalculated;
      this.Add(part);
      foreach (INfp c in part.Children)
      {
        this.AppendChild(c);
      }

      return this;
    }

    private void AppendChild(INfp c)
    {
      if (c is ObservableHole observableHole)
      {
        this.AppendChild(observableHole);
      }
      else if (c is ObservablePoint observablePoint)
      {
        this.AppendChild(observablePoint);
      }
      else if (c is ObservableFrame observableFrame)
      {
        this.AppendChild(observableFrame);
      }
      else
      {
        this.AppendChild(new ObservableHole(c));
      }
    }

    /// <inheritdoc />
    public void AppendChild(ObservableHole child)
    {
      this.Add(child);
      foreach (INfp c in child.Children)
      {
        this.AppendChild(new ObservableHole(c));
      }
    }

    /// <inheritdoc />
    public void AppendChild(ObservablePoint child)
    {
      this.Add(child);
    }

    /// <inheritdoc />
    public void AppendChild(ObservableFrame child)
    {
      this.Add(child);
    }
  }
}