namespace DeepNestLib.Placement
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class SheetPlacementJsonConverter : JsonConverterFactory
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsAssignableFrom(typeof(ISheetPlacement));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (CanConvert(typeToConvert))
      {
        return new SheetPlacementJsonConverterInner();
      }

      throw new ArgumentException($"Cannot convert {nameof(typeToConvert)}.", nameof(typeToConvert));
    }

    public class SheetPlacementJsonConverterInner : JsonConverter<ISheetPlacement>
    {
      public override ISheetPlacement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        return JsonSerializer.Deserialize<SheetPlacement>(ref reader, options);
      }

      public override void Write(Utf8JsonWriter writer, ISheetPlacement value, JsonSerializerOptions options)
      {
        JsonSerializer.Serialize(writer, (SheetPlacement)value, options);
      }
    }
  }
}
