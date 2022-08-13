namespace DeepNestLib.Placement
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using System.Threading.Tasks;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Geometry;
  using DeepNestLib.IO;

  /// <summary>
  /// Represents a sheet that has had parts placed on it in the nest.
  /// </summary>
  public class SheetPlacement : Saveable, ISheetPlacement
  {
    public const string FileDialogFilter = "DeepNest SheetPlacement (*.dnsp)|*.dnsp|Json (*.json)|*.json|All files (*.*)|*.*";

    private NoFitPolygon hull;
    private double clipperScale;

    [JsonConstructor]
    public SheetPlacement(PlacementTypeEnum placementType, ISheet sheet, IReadOnlyList<IPartPlacement> partPlacements, double mergedLength)
      : this(placementType, sheet, partPlacements, mergedLength, new TestSvgNestConfig().ClipperScale)
    {
    }

    public SheetPlacement(PlacementTypeEnum placementType, ISheet sheet, IReadOnlyList<IPartPlacement> partPlacements, double mergedLength, double clipperScale)
    {
      this.PlacementType = placementType;
      this.Sheet = sheet;
      this.PartPlacements = partPlacements;
      this.MergedLength = mergedLength;
      this.clipperScale = clipperScale;
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
    /// <inheritdoc />
    public PlacementTypeEnum PlacementType { get; private set; }

    [JsonInclude]
    /// <inheritdoc />
    public ISheet Sheet { get; private set; }

    [JsonInclude]
    /// <inheritdoc />
    public IReadOnlyList<IPartPlacement> PartPlacements { get; private set; } = new List<IPartPlacement>();

    [JsonIgnore]
    /// <inheritdoc />
    public PolygonBounds RectBounds => CombinedRectBounds(this.PartPlacements);

    [JsonIgnore]
    /// <inheritdoc />
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
    /// <inheritdoc />
    public INfp Simplify
    {
      get
      {
#if NCRUNCH
        var clipperScale = this.clipperScale == 0 ? 10000000 : this.clipperScale;
#else
        var clipperScale = this.clipperScale;
#endif
        var allpoints = CombinedPoints(this.PartPlacements);
        var clipperNfp = NfpHelper.NfpToClipperCoordinates(allpoints, clipperScale);

        var combinedNfp = new List<List<ClipperLib.IntPoint>>();
        var clipper = new ClipperLib.Clipper();
        _ = clipper.AddPaths(clipperNfp, ClipperLib.PolyType.ptSubject, true);
        _ = clipper.Execute(ClipperLib.ClipType.ctUnion, combinedNfp, ClipperLib.PolyFillType.pftNonZero, ClipperLib.PolyFillType.pftNonZero);

        return NfpSimplifier.SimplifyFunction(combinedNfp[0].ToArray().ToNestCoordinates(clipperScale), false, 1, false);
      }
    }

    [JsonIgnore]
    /// <inheritdoc />
    public double TotalPartsArea => this.PartPlacements.Sum(p => p.Part.NetArea);

    [JsonIgnore]
    /// <inheritdoc />
    public double MaterialUtilization
    {
      get
      {
        try
        {
          return Math.Min(1, Math.Abs(TotalPartsArea / this.Sheet.Area));
        }
        catch (Exception ex)
        {
          System.Diagnostics.Debug.Print(ex.Message);
          System.Diagnostics.Debug.Print(ex.StackTrace);
          throw;
        }
      }
    }

    [JsonIgnore]
    /// <inheritdoc />
    public OriginalFitnessSheet Fitness { get; }

    [JsonIgnore]
    /// <inheritdoc />
    public double MaxX => PartPlacements.Max(pp => pp.MaxX);

    [JsonIgnore]
    /// <inheritdoc />
    public double MaxY => PartPlacements.Max(pp => pp.MaxY);

    [JsonIgnore]
    /// <inheritdoc />
    public double MinX => PartPlacements.Min(pp => pp.MinX);

    [JsonIgnore]
    /// <inheritdoc />
    public double MinY => PartPlacements.Min(pp => pp.MinY);

    /// <inheritdoc />
    public double MergedLength { get; }

    [JsonIgnore]
    /// <inheritdoc />
    public IEnumerable<NoFitPolygon> PolygonsForExport
    {
      get
      {
        return this.PartPlacements.Select(
                o =>
                {
                  var result = new NoFitPolygon(o.Part, WithChildren.Included);
                  result.Sheet = this.Sheet;
                  result.X = o.X;
                  result.Y = o.Y;
                  return result;
                });
      }
    }

    public async Task ExportDxf(Stream stream, bool mergeLines, bool differentiateChildren)
    {
      await new DxfExporter().Export(stream, this, mergeLines, differentiateChildren);
    }

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

    public override string ToJson(bool writeIndented = false)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new SheetJsonConverter());
      options.Converters.Add(new NfpJsonConverter());
      options.Converters.Add(new PartPlacementJsonConverter());
      options.WriteIndented = writeIndented;
      return JsonSerializer.Serialize(this, options);
    }

    public override string ToString()
    {
      return this.Fitness.ToString();
    }

    internal static PolygonBounds CombinedRectBounds(IReadOnlyList<IPartPlacement> partPlacements)
    {
      NoFitPolygon allpoints = CombinedPoints(partPlacements);
      return GeometryUtil.GetPolygonBounds(allpoints);
    }

    internal static NoFitPolygon CombinedPoints(IReadOnlyList<IPartPlacement> partPlacements)
    {
      var allPoints = new List<SvgPoint>();
      for (var partIndex = 0; partIndex < partPlacements.Count; partIndex++)
      {
        var length = partPlacements[partIndex].Part.Points.Length;
        for (var pointIndex = 0; pointIndex < length; pointIndex++)
        {
          var part = partPlacements[partIndex].Part.Points[pointIndex];
          var placement = partPlacements[partIndex];
          allPoints.Add(
              new SvgPoint(
               part.X + placement.X,
               part.Y + placement.Y));
        }
      }

      return new NoFitPolygon(allPoints);
    }
  }
}
