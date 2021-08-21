namespace DeepNestLib.Placement
{
  using System;
  using System.Collections.Generic;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class ListConverter<M> : JsonConverter<IList<M>>
  {
    public override IList<M> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      return JsonSerializer.Deserialize<List<M>>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, IList<M> value, JsonSerializerOptions options)
    {
      throw new NotImplementedException();
    }
  }
}
