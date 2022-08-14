namespace DeepNestLib.Converters
{
  using System;

  [AttributeUsage(AttributeTargets.Property)]
  public class FormattedDoubleFormatString : Attribute
  {
    public FormattedDoubleFormatString(string formatString)
    {
      FormatString = formatString;
    }

    public string FormatString { get; private set; }
  }
}