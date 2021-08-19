namespace DeepNestLib.Placement
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.IO;
  using DeepNestLib.NestProject;

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
