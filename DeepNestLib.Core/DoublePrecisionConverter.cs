namespace DeepNestLib
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class DoublePrecisionConverter : JsonConverter<double>
  {
    public override bool HandleNull => true;

    public override double Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) => reader.GetDouble();

    public override void Write(
        Utf8JsonWriter writer,
        double value,
        JsonSerializerOptions options)
    {
      var outValue = (decimal)Math.Round(value, 4);
      writer.WriteNumberValue(outValue);
    }
  }
}
