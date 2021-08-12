namespace DeepNestLib.NestProject
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class SvgNestConfigJsonConverter : JsonConverterFactory
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsAssignableFrom(typeof(ISvgNestConfig));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (CanConvert(typeToConvert))
      {
        return new SvgNestConfigJsonConverterInner();
      }

      throw new ArgumentException($"Cannot convert {nameof(typeToConvert)}.", nameof(typeToConvert));
    }

    public class SvgNestConfigJsonConverterInner : JsonConverter<ISvgNestConfig>
    {
      public override ISvgNestConfig Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        return JsonSerializer.Deserialize<SvgNestConfig>(ref reader, options);
      }

      public override void Write(Utf8JsonWriter writer, ISvgNestConfig value, JsonSerializerOptions options)
      {
        JsonSerializer.Serialize<SvgNestConfig>(writer, (SvgNestConfig)value, options);
      }
    }

    internal static ISvgNestConfig FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new SvgNestConfigJsonConverter());
      return JsonSerializer.Deserialize<SvgNestConfig>(json, options);
    }

    internal static string ToJson(SvgNestConfig config)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new SvgNestConfigJsonConverter());
      return JsonSerializer.Serialize<SvgNestConfig>(config, options);
    }

    /// <summary>
    /// Long hand copy of every serializable property from source to target config instances.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    internal static void FullCopy(ISvgNestConfig source, ISvgNestConfig target)
    {
      target.ClipByHull = source.ClipByHull;
      target.ClipperScale = source.ClipperScale;
      target.CurveTolerance = source.CurveTolerance;
      target.DrawSimplification = source.DrawSimplification;
      target.ExploreConcave = source.ExploreConcave;
      target.MergeLines = source.MergeLines;
      target.Multiplier = source.Multiplier;
      target.MutationRate = source.MutationRate;
      target.OffsetTreePhase = source.OffsetTreePhase;
      target.ParallelNests = source.ParallelNests;
      target.PlacementType = source.PlacementType;
      target.PopulationSize = source.PopulationSize;
      target.Rotations = source.Rotations;
      target.SaveAsFileTypeIndex = source.SaveAsFileTypeIndex;
      target.Scale = source.Scale;
      target.SheetHeight = source.SheetHeight;
      target.SheetQuantity = source.SheetQuantity;
      target.SheetSpacing = source.SheetSpacing;
      target.SheetWidth = source.SheetWidth;
      target.ShowPartPositions = source.ShowPartPositions;
      target.Simplify = source.Simplify;
      target.Spacing = source.Spacing;
      target.StrictAngles = source.StrictAngles;
      target.TimeRatio = source.TimeRatio;
      target.Tolerance = source.Tolerance;
      target.ToleranceSvg = source.ToleranceSvg;
      target.UseHoles = source.UseHoles;
      target.UseParallel = source.UseParallel;
    }
  }
}