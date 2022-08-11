namespace DeepNestLib
{
  using DeepNestLib.Placement;
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class MinkowskiSumJsonConverter : JsonConverterFactory
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsAssignableFrom(typeof(IMinkowskiSumService));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (CanConvert(typeToConvert))
      {
        return new MinkowskiSumJsonConverterInner();
      }

      throw new ArgumentException($"Cannot convert {nameof(typeToConvert)}.", nameof(typeToConvert));
    }

    private class MinkowskiSumJsonConverterInner : JsonConverter<IMinkowskiSumService>
    {
      public override IMinkowskiSumService Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        return JsonSerializer.Deserialize<MinkowskiSum>(ref reader, options);
      }

      public override void Write(Utf8JsonWriter writer, IMinkowskiSumService value, JsonSerializerOptions options)
      {
        JsonSerializer.Serialize<MinkowskiSum>(writer, (MinkowskiSum)value, options);
      }
    }
  }
}