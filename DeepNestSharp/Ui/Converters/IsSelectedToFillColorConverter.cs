namespace DeepNestSharp.Ui.Converters
{
  using System;
  using System.Windows.Data;
  using System.Windows.Media;
  using DeepNestSharp.Domain.Models;

  public class IsSelectedToFillColorConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      ObservablePartPlacement? boundValue = values[0] as ObservablePartPlacement;
      ObservablePartPlacement? selectedValue = values[1] as ObservablePartPlacement;
      ObservablePartPlacement? hoverValue = values[2] as ObservablePartPlacement;
      if (boundValue == null)
      {
        return Brushes.AliceBlue;
      }
      else if (hoverValue == boundValue)
      {
        return Brushes.LightSteelBlue;
      }
      else if (selectedValue == boundValue)
      {
        return Brushes.Aquamarine;
      }

      return Brushes.AliceBlue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException("Going back to what you had isn't supported.");
    }
  }
}