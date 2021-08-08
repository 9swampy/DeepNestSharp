namespace DeepNestSharp.Ui.Converters
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Windows.Data;
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Ui.ViewModels;

  public class DrawingContextConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value is ZoomPreviewDrawingContext)
      {
        throw new InvalidOperationException("Thought this was no longer needed.");
        return value;
      }
      else if (value is IEnumerable<ZoomPreviewDrawingContext>)
      {
        throw new InvalidOperationException("Thought this was no longer needed.");
        return value;
      }
      else if (value is ISheetPlacement sheetPlacement)
      {
        throw new InvalidOperationException("Thought this was no longer needed.");
        return new ZoomPreviewDrawingContext().For(sheetPlacement);
      }
      else if (value is SheetPlacementCollection sheetPlacementCollection)
      {
        var result = new List<ZoomPreviewDrawingContext>(sheetPlacementCollection.Count);
        foreach (var sp in sheetPlacementCollection)
        {
          result.Add(new ZoomPreviewDrawingContext().For(sp));
        }

        return result;
      }
      else if (value is NestProjectViewModel nestProjectViewModel)
      {
        throw new InvalidOperationException("Thought this was no longer needed.");
        var resultProject = new List<ZoomPreviewDrawingContext>(nestProjectViewModel.ProjectInfo.DetailLoadInfos.Count);
        foreach (var detailLoadInfo in nestProjectViewModel.ProjectInfo.DetailLoadInfos)
        {
          if (new FileInfo(detailLoadInfo.Path).Exists &&
             DxfParser.LoadDxfFile(detailLoadInfo.Path).Result.ToNfp() is INfp loadedNfp)
          {
            var shiftedPart = Background.ShiftPolygon(loadedNfp, -loadedNfp.MinX, -loadedNfp.MinY);
            resultProject.Add(new ZoomPreviewDrawingContext().For(shiftedPart));
          }

          return resultProject;
        }
      }

      return Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value is ISheetPlacement)
      {
        return value;
      }

      return Binding.DoNothing;
    }
  }
}