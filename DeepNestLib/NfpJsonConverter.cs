namespace DeepNestLib
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class NfpJsonConverter : JsonConverterFactory
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsAssignableFrom(typeof(INfp));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (CanConvert(typeToConvert))
      {
        return new NfpJsonConverterInner();
      }

      throw new ArgumentException($"Cannot convert {nameof(typeToConvert)}.", nameof(typeToConvert));
    }

    public class NfpJsonConverterInner : JsonConverter<INfp>
    {
      public override INfp Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        return JsonSerializer.Deserialize<NFP>(ref reader, options);
      }

      public override void Write(Utf8JsonWriter writer, INfp value, JsonSerializerOptions options)
      {
        JsonSerializer.Serialize<NFP>(writer, (NFP)value, options);
      }
    }
  }
}