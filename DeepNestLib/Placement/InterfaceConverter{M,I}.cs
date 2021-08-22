namespace DeepNestLib.Placement
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class InterfaceConverter<TConcrete, TInterface> : JsonConverter<TInterface>
    where TConcrete : class, TInterface
  {
    public override TInterface Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      return JsonSerializer.Deserialize<TConcrete>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options) { }
  }
}
