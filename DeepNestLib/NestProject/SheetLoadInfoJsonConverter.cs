namespace DeepNestLib.NestProject
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class SheetLoadInfoJsonConverter : JsonConverterFactory
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsAssignableFrom(typeof(ISheetLoadInfo));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (CanConvert(typeToConvert))
      {
        return new SheetLoadInfoJsonConverterInner();
      }

      throw new ArgumentException($"Cannot convert {nameof(typeToConvert)}.", nameof(typeToConvert));
    }

    public class SheetLoadInfoJsonConverterInner : JsonConverter<ISheetLoadInfo>
    {
      public override ISheetLoadInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        return JsonSerializer.Deserialize<SheetLoadInfo>(ref reader, options);
      }

      public override void Write(Utf8JsonWriter writer, ISheetLoadInfo value, JsonSerializerOptions options)
      {
        JsonSerializer.Serialize<SheetLoadInfo>(writer, (SheetLoadInfo)value, options);
      }
    }
  }
}