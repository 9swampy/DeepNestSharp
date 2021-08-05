namespace DeepNestLib.NestProject
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class SvgNestConfigJsonConverter : JsonConverterFactory
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsAssignableFrom(typeof(ISvgNestConfig));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (CanConvert(typeToConvert))
      {
        return new SvgNestConfigJsonConverterInner();
      }

      throw new ArgumentException($"Cannot convert {nameof(typeToConvert)}.", nameof(typeToConvert));
    }

    public class SvgNestConfigJsonConverterInner : JsonConverter<ISvgNestConfig>
    {
      public override ISvgNestConfig Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        return JsonSerializer.Deserialize<SvgNestConfig>(ref reader, options);
      }

      public override void Write(Utf8JsonWriter writer, ISvgNestConfig value, JsonSerializerOptions options)
      {
        JsonSerializer.Serialize(writer, (SvgNestConfig)value, options);
      }
    }
  }
}