namespace DeepNestLib
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class WindowUnkJsonConverter : JsonConverterFactory
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsAssignableFrom(typeof(IWindowUnk));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (CanConvert(typeToConvert))
      {
        return new WindowUnkJsonConverterInner();
      }

      throw new ArgumentException($"Cannot convert {nameof(typeToConvert)}.", nameof(typeToConvert));
    }

    private class WindowUnkJsonConverterInner : JsonConverter<IWindowUnk>
    {
      public override IWindowUnk Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        return JsonSerializer.Deserialize<WindowUnk>(ref reader, options);
      }

      public override void Write(Utf8JsonWriter writer, IWindowUnk value, JsonSerializerOptions options)
      {
        JsonSerializer.Serialize<WindowUnk>(writer, (WindowUnk)value, options);
      }
    }
  }
}