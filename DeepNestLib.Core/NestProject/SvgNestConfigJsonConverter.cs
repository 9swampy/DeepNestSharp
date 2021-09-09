namespace DeepNestLib.NestProject
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class SvgNestConfigJsonConverter : JsonConverterFactory
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsAssignableFrom(typeof(ISvgNestConfig)) ||
             typeToConvert.IsAssignableFrom(typeof(IPlacementConfig)) ||
             typeToConvert.IsAssignableFrom(typeof(ITopNestResultsConfig));
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
#if NCRUNCH
        return JsonSerializer.Deserialize<TestSvgNestConfig>(ref reader, options);
#else
        return JsonSerializer.Deserialize<SvgNestConfig>(ref reader, options);
#endif
      }

      public override void Write(Utf8JsonWriter writer, ISvgNestConfig value, JsonSerializerOptions options)
      {
#if NCRUNCH
        JsonSerializer.Serialize<TestSvgNestConfig>(writer, (TestSvgNestConfig)value, options);
#else
        if (value is IExportableConfig obs)
        {
          JsonSerializer.Serialize<SvgNestConfig>(writer, (SvgNestConfig)obs.ExportableInstance, options);
        }
        else
        {
          JsonSerializer.Serialize<SvgNestConfig>(writer, (SvgNestConfig)value, options);
        }
#endif
      }
    }

    internal static ISvgNestConfig FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new SvgNestConfigJsonConverter());
#if NCRUNCH
      return JsonSerializer.Deserialize<TestSvgNestConfig>(json, options);
#else
      return JsonSerializer.Deserialize<SvgNestConfig>(json, options);
#endif
    }

    internal static string ToJson(ISvgNestConfig config)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new SvgNestConfigJsonConverter());
#if NCRUNCH
      return JsonSerializer.Serialize<TestSvgNestConfig>((TestSvgNestConfig)config, options);
#else
      return JsonSerializer.Serialize<SvgNestConfig>((SvgNestConfig)config, options);
#endif
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
      target.ExportExecutionPath = source.ExportExecutionPath;
      target.ExportExecutions = source.ExportExecutions;
      target.LastDebugFilePath = source.LastDebugFilePath;
      target.LastNestFilePath = source.LastNestFilePath;
      target.MergeLines = source.MergeLines;
      target.Multiplier = source.Multiplier;
      target.MutationRate = source.MutationRate;
      target.OffsetTreePhase = source.OffsetTreePhase;
      target.ParallelNests = source.ParallelNests;
      target.PlacementType = source.PlacementType;
      target.PopulationSize = source.PopulationSize;
      target.ProcreationTimeout = source.ProcreationTimeout;
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
      target.UseDllImport = source.UseDllImport;
      target.UseHoles = source.UseHoles;
      target.UseMinkowskiCache = source.UseMinkowskiCache;
      target.UseParallel = source.UseParallel;
      target.UsePriority = source.UsePriority;
    }
  }
}