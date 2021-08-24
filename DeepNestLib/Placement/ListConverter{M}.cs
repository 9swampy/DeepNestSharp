namespace DeepNestLib.Placement
{
  using System;
  using System.Collections.Generic;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class ListConverter<TItem> : JsonConverter<IList<TItem>>
  {
    public override IList<TItem> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      return JsonSerializer.Deserialize<List<TItem>>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, IList<TItem> value, JsonSerializerOptions options)
    {
      throw new NotImplementedException();
    }
  }
}
