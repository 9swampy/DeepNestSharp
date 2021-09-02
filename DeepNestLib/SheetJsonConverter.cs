namespace DeepNestLib
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class SheetJsonConverter : JsonConverterFactory
  {
    public override bool CanConvert(Type typeToConvert)
    {
      // return typeToConvert == typeof(ISheet) || typeToConvert == typeof(Sheet);
      if (typeToConvert.IsAssignableFrom(typeof(ISheet)) &&
          typeToConvert != typeof(INfp))
      {
        return true;
      }

      return false;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (CanConvert(typeToConvert))
      {
        return new SheetJsonConverterInner();
      }

      throw new ArgumentException($"Cannot convert {nameof(typeToConvert)}.", nameof(typeToConvert));
    }

    private class SheetJsonConverterInner : JsonConverter<ISheet>
    {
      public override ISheet Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        return JsonSerializer.Deserialize<Sheet>(ref reader, options);
      }

      public override void Write(Utf8JsonWriter writer, ISheet value, JsonSerializerOptions options)
      {
        try
        {
          JsonSerializer.Serialize<Sheet>(writer, (Sheet)value, options);
        }
        catch (Exception ex)
        {
          throw;
        }
      }
    }
  }
}