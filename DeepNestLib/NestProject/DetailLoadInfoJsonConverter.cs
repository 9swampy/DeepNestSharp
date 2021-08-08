namespace DeepNestLib.NestProject
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class DetailLoadInfoJsonConverter : JsonConverterFactory
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsAssignableFrom(typeof(IDetailLoadInfo));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (CanConvert(typeToConvert))
      {
        return new DetailLoadInfoJsonConverterInner();
      }

      throw new ArgumentException($"Cannot convert {nameof(typeToConvert)}.", nameof(typeToConvert));
    }

    public class DetailLoadInfoJsonConverterInner : JsonConverter<IDetailLoadInfo>
    {
      public override IDetailLoadInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        return JsonSerializer.Deserialize<DetailLoadInfo>(ref reader, options);
      }

      public override void Write(Utf8JsonWriter writer, IDetailLoadInfo value, JsonSerializerOptions options)
      {
        JsonSerializer.Serialize<DetailLoadInfo>(writer, (DetailLoadInfo)value, options);
      }
    }
  }
}