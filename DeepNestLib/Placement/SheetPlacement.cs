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

  /// <summary>
  /// Represents a sheet that has had parts placed on it in the nest.
  /// </summary>
  public class SheetPlacement : Saveable, ISheetPlacement
  {
    public const string FileDialogFilter = "DeepNest SheetPlacement (*.dnsp)|*.dnsp|Json (*.json)|*.json|All files (*.*)|*.*";

    private NFP hull;

    public SheetPlacement(PlacementTypeEnum placementType, ISheet sheet, IReadOnlyList<IPartPlacement> partPlacements)
    {
      this.PlacementType = placementType;
      this.Sheet = sheet;
      this.PartPlacements = partPlacements;
      this.Fitness = new OriginalFitnessSheet(this);
    }

    /// <summary>
    /// Gets memoised sheet.Id; to maintain legacy - monitor if sheet.Id is ever getting updated (may be Liskov breach in Sheet?).
    /// </summary>
    public int SheetId => Sheet.Id;

    /// <summary>
    /// Gets memoised sheet.Source; to maintain legacy - monitor if sheet.Id is ever getting updated (may be Liskov breach in Sheet?).
    /// </summary>
    public int SheetSource => Sheet.Source;

    [JsonInclude]
    public PlacementTypeEnum PlacementType { get; private set; }

    [JsonInclude]
    public ISheet Sheet { get; private set; }

    [JsonInclude]
    public IReadOnlyList<IPartPlacement> PartPlacements { get; private set; } = new List<IPartPlacement>();

    [JsonIgnore]
    public PolygonBounds RectBounds => CombinedRectBounds(this.PartPlacements);

    [JsonIgnore]
    public INfp Hull
    {
      get
      {
        if (hull == null)
        {
          hull = CombinedPoints(this.PartPlacements).GetHull();
        }

        return hull;
      }
    }

    [JsonIgnore]
    public INfp Simplify
    {
      get
      {
        var clipperScale = new SvgNestConfig().ClipperScale;
        var allpoints = CombinedPoints(this.PartPlacements);
        var clipperNfp = Background.NfpToClipperCoordinates(allpoints, clipperScale);

        var combinedNfp = new List<List<ClipperLib.IntPoint>>();
        var clipper = new ClipperLib.Clipper();
        _ = clipper.AddPaths(clipperNfp.Select(z => z.ToList()).ToList(), ClipperLib.PolyType.ptSubject, true);
        _ = clipper.Execute(ClipperLib.ClipType.ctUnion, combinedNfp, ClipperLib.PolyFillType.pftNonZero, ClipperLib.PolyFillType.pftNonZero);

        return SvgNest.SimplifyFunction(combinedNfp[0].ToArray().ToNestCoordinates(clipperScale), false, 1, false);
      }
    }

    [JsonIgnore]
    public double TotalPartsArea => this.PartPlacements.Sum(p => p.Part.Area);

    [JsonIgnore]
    public double MaterialUtilization => Math.Abs(TotalPartsArea / this.Sheet.Area);

    [JsonIgnore]
    public OriginalFitnessSheet Fitness { get; }

    [JsonIgnore]
    public double MaxX => PartPlacements.Max(pp => pp.MaxX);

    [JsonIgnore]
    public double MaxY => PartPlacements.Max(pp => pp.MaxY);

    [JsonIgnore]
    public double MinX => PartPlacements.Min(pp => pp.MinX);

    [JsonIgnore]
    public double MinY => PartPlacements.Min(pp => pp.MinY);

    public static SheetPlacement LoadFromFile(string fileName)
    {
      using (StreamReader inputFile = new StreamReader(fileName))
      {
        return FromJson(inputFile.ReadToEnd());
      }
    }

    public static SheetPlacement FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new SheetJsonConverter());
      options.Converters.Add(new NfpJsonConverter());
      options.Converters.Add(new PartPlacementJsonConverter());
      return JsonSerializer.Deserialize<SheetPlacement>(json, options);
    }

    public override string ToJson()
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new SheetJsonConverter());
      options.Converters.Add(new NfpJsonConverter());
      options.Converters.Add(new PartPlacementJsonConverter());
      return JsonSerializer.Serialize(this, options);
    }

    public override string ToString()
    {
      return this.Fitness.ToString();
    }

    internal static PolygonBounds CombinedRectBounds(IReadOnlyList<IPartPlacement> partPlacements)
    {
      NFP allpoints = CombinedPoints(partPlacements);
      return GeometryUtil.getPolygonBounds(allpoints);
    }

    internal static NFP CombinedPoints(IReadOnlyList<IPartPlacement> partPlacements)
    {
      NFP allpoints = new NFP();
      for (var partIndex = 0; partIndex < partPlacements.Count; partIndex++)
      {
        var length = partPlacements[partIndex].Part.Points.Length;
        for (var pointIndex = 0; pointIndex < length; pointIndex++)
        {
          var part = partPlacements[partIndex].Part.Points[pointIndex];
          var placement = partPlacements[partIndex];
          allpoints.AddPoint(
              new SvgPoint(
               part.X + placement.X,
               part.Y + placement.Y));
        }
      }

      return allpoints;
    }
  }

  public class SheetPlacementJsonConverter : JsonConverterFactory
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsAssignableFrom(typeof(ISheetPlacement));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (CanConvert(typeToConvert))
      {
        return new SheetPlacementJsonConverterInner();
      }

      throw new ArgumentException($"Cannot convert {nameof(typeToConvert)}.", nameof(typeToConvert));
    }

    public class SheetPlacementJsonConverterInner : JsonConverter<ISheetPlacement>
    {
      public override ISheetPlacement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        return JsonSerializer.Deserialize<SheetPlacement>(ref reader, options);
      }

      public override void Write(Utf8JsonWriter writer, ISheetPlacement value, JsonSerializerOptions options)
      {
        JsonSerializer.Serialize<SheetPlacement>(writer, (SheetPlacement)value, options);
      }
    }
  }
}
