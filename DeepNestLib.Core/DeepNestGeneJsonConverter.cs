namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using DeepNestLib.GeneticAlgorithm;

  public class DeepNestGeneJsonConverter : JsonConverterFactory
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsAssignableFrom(typeof(DeepNestGene));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (CanConvert(typeToConvert))
      {
        return new DeepNestGeneJsonConverterInner();
      }

      throw new ArgumentException($"Cannot convert {nameof(typeToConvert)}.", nameof(typeToConvert));
    }

    private class DeepNestGeneJsonConverterInner : JsonConverter<DeepNestGene>
    {
      public override DeepNestGene Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        var chromosomes = JsonSerializer.Deserialize<List<Chromosome>>(ref reader, options);
        return new DeepNestGene(chromosomes);
      }

      public override void Write(Utf8JsonWriter writer, DeepNestGene value, JsonSerializerOptions options)
      {
        JsonSerializer.Serialize(writer, value, options);
      }
    }
  }
}