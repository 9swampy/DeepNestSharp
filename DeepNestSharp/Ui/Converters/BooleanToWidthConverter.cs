namespace DeepNestSharp.Ui.Converters
{
  using System;
  using System.Globalization;
  using System.Windows;
  using System.Windows.Data;

  public class BooleanToWidthConverter : IValueConverter
  {
    private const double Column_Width = 40.0;
    private const string Column_Width_Auto = "auto";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value != null && value != DependencyProperty.UnsetValue)
      {
        bool isVisible = (bool)value;

        return isVisible ? Column_Width : 0;
      }

      return Column_Width_Auto;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}