namespace DeepNestLib
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class ClipCacheItemJsonConverter : JsonConverter<ClipCacheItem>
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert == typeof(ClipCacheItem);
    }

    public override ClipCacheItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      var newOptions = new JsonSerializerOptions(options);
      newOptions.Converters.Clear();
      newOptions.IncludeFields = true;
      return JsonSerializer.Deserialize<ClipCacheItem>(ref reader, newOptions);
    }

    public override void Write(Utf8JsonWriter writer, ClipCacheItem value, JsonSerializerOptions options)
    {
      var newOptions = new JsonSerializerOptions(options);
      newOptions.Converters.Clear();
      newOptions.IncludeFields = true;
      JsonSerializer.Serialize<ClipCacheItem>(writer, (ClipCacheItem)value, newOptions);
    }
  }

  public class ClipperLibIntPointJsonConverter : JsonConverter<ClipperLib.IntPoint>
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert == typeof(ClipperLib.IntPoint);
    }

    public override ClipperLib.IntPoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      var newOptions = new JsonSerializerOptions(options);
      newOptions.Converters.Clear();
      newOptions.IncludeFields = true;
      return JsonSerializer.Deserialize<ClipperLib.IntPoint>(ref reader, newOptions);
    }

    public override void Write(Utf8JsonWriter writer, ClipperLib.IntPoint value, JsonSerializerOptions options)
    {
      var newOptions = new JsonSerializerOptions(options);
      newOptions.Converters.Clear();
      newOptions.IncludeFields = true;
      JsonSerializer.Serialize<ClipperLib.IntPoint>(writer, (ClipperLib.IntPoint)value, newOptions);
    }
  }
}