namespace DeepNestLib.Converters
{
  using System;
  using System.ComponentModel;
  using System.Linq;

  public class FormattedDoubleConverter : TypeConverter
  {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
      return sourceType == typeof(string) || sourceType == typeof(double);
    }

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
      return destinationType == typeof(string) || destinationType == typeof(double);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture,
                                       object value)
    {
      if (value is double)
      {
        return value;
      }

      var str = value as string;
      if (str != null)
      {
        return double.Parse(str);
      }

      return null;
    }

    public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture,
                                     object value, Type destinationType)
    {
      if (destinationType != typeof(string))
      {
        return null;
      }

      if (value is double)
      {
        var property = context.PropertyDescriptor;
        if (property != null)
        {
          // Analyze the property for a second attribute that gives the format string
          var formatStrAttr = property.Attributes.OfType<FormattedDoubleFormatString>().FirstOrDefault();
          if (formatStrAttr != null)
          {
            return ((double)value).ToString(formatStrAttr.FormatString);
          }
          else
          {
            return ((double)value).ToString();
          }
        }
      }

      return null;
    }
  }
}