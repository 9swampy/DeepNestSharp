namespace DeepNestSharp.Ui.Converters
{
  using System;
  using System.Globalization;
  using System.Windows.Data;
  using System.Windows.Media;
  using DeepNestLib;
  using DeepNestSharp.Domain.Models;

  public class StrokeConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (SvgNest.Config.DifferentiateChildren && value is ObservableHole)
      {
        return Brushes.DeepPink;
      }
      else if ((value is IPolygon poly && poly.IsDifferentiated) ||
               (value is ObservablePartPlacement partPlacement && partPlacement.Part.IsDifferentiated))
      {
        return Brushes.Blue;
      }

      return Brushes.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return Binding.DoNothing;
    }
  }
}