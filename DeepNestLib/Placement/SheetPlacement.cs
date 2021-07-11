namespace DeepNestLib.Placement
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using DeepNestLib.GeneticAlgorithm;

  /// <summary>
  /// Represents a sheet that has had parts placed on it in the nest.
  /// </summary>
  public class SheetPlacement : ISheetPlacement
  {
    private NFP hull;

    public SheetPlacement(PlacementTypeEnum placementType, INfp sheet, IList<PartPlacement> partPlacements)
    {
      this.SheetId = sheet.Id;
      this.SheetSource = sheet.Source;
      this.PlacementType = placementType;
      this.Sheet = sheet;
      this.PartPlacements = partPlacements;
      this.Fitness = new OriginalFitnessSheet(this);
    }

    /// <summary>
    /// Gets memoised sheet.Id; to maintain legacy - monitor if sheet.Id is ever getting updated (may be Liskov breach in Sheet?).
    /// </summary>
    [JsonInclude]
    public int SheetId { get; private set; }

    /// <summary>
    /// Gets memoised sheet.Source; to maintain legacy - monitor if sheet.Id is ever getting updated (may be Liskov breach in Sheet?).
    /// </summary>
    [JsonInclude]
    public int SheetSource { get; private set; }

    [JsonInclude]
    public PlacementTypeEnum PlacementType { get; private set; }

    [JsonInclude]
    public INfp Sheet { get; private set; }

    [JsonInclude]
    public IList<PartPlacement> PartPlacements { get; private set; } = new List<PartPlacement>();

    [JsonIgnore]
    public PolygonBounds RectBounds
    {
      get
      {
        return CombinedRectBounds(this.PartPlacements);
      }
    }

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
        clipper.AddPaths(clipperNfp.Select(z => z.ToList()).ToList(), ClipperLib.PolyType.ptSubject, true);
        clipper.Execute(ClipperLib.ClipType.ctUnion, combinedNfp, ClipperLib.PolyFillType.pftNonZero, ClipperLib.PolyFillType.pftNonZero);

        return SvgNest.simplifyFunction(combinedNfp[0].ToArray().ToNestCoordinates(clipperScale), false, 1, false);
      }
    }

    [JsonIgnore]
    public float TotalPartsArea => this.PartPlacements.Sum(p => p.Part.Area);

    [JsonIgnore]
    public float MaterialUtilization
    {
      get
      {
        return Math.Abs(TotalPartsArea / this.Sheet.Area);
      }
    }

    internal static PolygonBounds CombinedRectBounds(IList<PartPlacement> partPlacements)
    {
      NFP allpoints = CombinedPoints(partPlacements);
      return GeometryUtil.getPolygonBounds(allpoints);
    }

    internal static NFP CombinedPoints(IList<PartPlacement> partPlacements)
    {
      NFP allpoints = new NFP();
      for (int partIndex = 0; partIndex < partPlacements.Count; partIndex++)
      {
        var length = partPlacements[partIndex].Part.Points.Length;
        for (int pointIndex = 0; pointIndex < length; pointIndex++)
        {
          var part = partPlacements[partIndex].Part.Points[pointIndex];
          var placement = partPlacements[partIndex];
          allpoints.AddPoint(
              new SvgPoint(
               part.x + placement.x,
               part.y + placement.y));
        }
      }

      return allpoints;
    }

    [JsonIgnore]
    public OriginalFitnessSheet Fitness { get; }

    public string ToJson()
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new NfpJsonConverter());
      return JsonSerializer.Serialize(this, options);
    }

    public static SheetPlacement FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new NfpJsonConverter());
      return JsonSerializer.Deserialize<SheetPlacement>(json, options);
    }

    public override string ToString()
    {
      return this.Fitness.ToString();
    }
  }
}
