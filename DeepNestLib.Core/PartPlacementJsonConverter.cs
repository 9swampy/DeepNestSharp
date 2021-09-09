namespace DeepNestLib
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using DeepNestLib.Placement;

  public class PartPlacementJsonConverter : JsonConverterFactory
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsAssignableFrom(typeof(IPartPlacement));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (CanConvert(typeToConvert))
      {
        return new PartPlacementJsonConverterInner();
      }

      throw new ArgumentException($"Cannot convert {nameof(typeToConvert)}.", nameof(typeToConvert));
    }

    public class PartPlacementJsonConverterInner : JsonConverter<IPartPlacement>
    {
      public override IPartPlacement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        return JsonSerializer.Deserialize<PartPlacement>(ref reader, options);
      }

      public override void Write(Utf8JsonWriter writer, IPartPlacement value, JsonSerializerOptions options)
      {
        JsonSerializer.Serialize<PartPlacement>(writer, (PartPlacement)value, options);
      }
    }
  }
}