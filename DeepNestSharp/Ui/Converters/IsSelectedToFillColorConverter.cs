namespace DeepNestSharp.Ui.Converters
{
  using System;
  using System.Windows.Data;
  using System.Windows.Media;
  using DeepNestSharp.Ui.Models;

  public class IsSelectedToFillColorConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      var firstValue = values[0] as ObservablePartPlacement;
      var secondValue = values[1] as ObservablePartPlacement;

      return firstValue != null && firstValue == secondValue ? Brushes.Aquamarine : Brushes.AliceBlue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException("Going back to what you had isn't supported.");
    }
  }
}