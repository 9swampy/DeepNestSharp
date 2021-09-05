namespace DeepNestSharp.Ui.Converters
{
  using System;
  using System.Globalization;
  using System.Windows.Data;
  using System.Windows.Media;
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Models;

  public class PointsConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is ObservablePoint obsPoint)
      {
        return new System.Windows.Point(obsPoint[0].X, obsPoint[0].Y);
      }
      else if (value is INfp item)
      {
        var result = new PointCollection(item.Length);
        for (int i = 0; i < item.Length; i++)
        {
          result.Add(new System.Windows.Point(item[i].X, item[i].Y));
        }

        return result;
      }
      else if (value is IPartPlacement partPlacement)
      {
        var result = new PointCollection(partPlacement.Part.Length);
        for (int i = 0; i < partPlacement.Part.Length; i++)
        {
          result.Add(new System.Windows.Point(partPlacement.Part[i].X, partPlacement.Part[i].Y));
        }

        return result;
      }

      return Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is INfp)
      {
        return value;
      }

      return Binding.DoNothing;
    }
  }
}