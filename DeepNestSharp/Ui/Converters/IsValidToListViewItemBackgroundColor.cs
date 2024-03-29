﻿namespace DeepNestSharp.Ui.Converters
{
  using System;
  using System.Globalization;
  using System.Windows.Data;
  using System.Windows.Media;

  public class IsValidToListViewItemBackgroundColor : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool isValid && !isValid)
      {
        return Brushes.PaleVioletRed;
      }

      return Brushes.White;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException("Going back to what you had isn't supported.");
    }
  }
}