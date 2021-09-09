namespace DeepNestSharp.Ui.Converters
{
  using System;
  using System.Globalization;
  using System.Windows;
  using System.Windows.Data;

  public class BooleanToWidthConverter : IValueConverter
  {
    private const double ColumnWidth = 40.0;
    private const string ColumnWidthAuto = "auto";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value != null && value != DependencyProperty.UnsetValue)
      {
        bool isVisible = (bool)value;

        return isVisible ? ColumnWidth : 0;
      }

      return ColumnWidthAuto;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}