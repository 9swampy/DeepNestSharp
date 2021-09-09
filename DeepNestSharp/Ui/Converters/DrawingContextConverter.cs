namespace DeepNestSharp.Ui.Converters
{
  using System;
  using System.Collections.Generic;
  using System.Windows.Data;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.ViewModels;

  public class DrawingContextConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value is SheetPlacementCollection sheetPlacementCollection)
      {
        var result = new List<IZoomPreviewDrawingContext>(sheetPlacementCollection.Count);
        foreach (var sp in sheetPlacementCollection)
        {
          result.Add(new ZoomPreviewDrawingContext().For(sp));
        }

        return result;
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