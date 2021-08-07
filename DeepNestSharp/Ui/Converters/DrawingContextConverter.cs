namespace DeepNestSharp.Ui.Converters
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Windows.Data;
  using DeepNestLib;
  using DeepNestLib.NestProject;
  using DeepNestLib.Placement;
  using DeepNestSharp.Ui.Models;
  using DeepNestSharp.Ui.ViewModels;

  public class DrawingContextConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return null;
      if (value is ZoomPreviewDrawingContext)
      {
        return value;
      }
      else if (value is IEnumerable<ZoomPreviewDrawingContext>)
      {
        return value;
      }
      else if (value is ISheetPlacement sheetPlacement)
      {
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
        var resultProject = new List<ZoomPreviewDrawingContext>(nestProjectViewModel.ProjectInfo.DetailLoadInfos.Count);
        foreach (var detailLoadInfo in nestProjectViewModel.ProjectInfo.DetailLoadInfos)
        {
          if (new FileInfo(detailLoadInfo.Path).Exists &&
             DxfParser.LoadDxfFile(detailLoadInfo.Path).ToNfp() is INfp loadedNfp)
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