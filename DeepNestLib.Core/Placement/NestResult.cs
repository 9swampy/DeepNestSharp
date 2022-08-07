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

  public class NestResult : Saveable, INestResult
  {
    public const string FileDialogFilter = "DeepNest Result (*.dnr)|*.dnr|Json (*.json)|*.json|All files (*.*)|*.*";

    private readonly OriginalFitness fitness;

    private SheetPlacementCollection sheetPlacementCollection;

    [Obsolete("Use only for deserialization.")]
    public NestResult()
    {
      this.fitness = new OriginalFitness(this);
    }

    public NestResult(
      int totalParts,
      SheetPlacementCollection allPlacements,
      IList<INfp> unplacedParts,
      PlacementTypeEnum placementType,
      long placePartTime,
      long backgroundTime)
#pragma warning disable CS0618 // Type or member is obsolete
      : this()
#pragma warning restore CS0618 // Type or member is obsolete
    {
      this.TotalParts = totalParts;
      this.UsedSheets = allPlacements;
      this.UnplacedParts = unplacedParts;
      this.PlacementType = placementType;
      this.PlacePartTime = placePartTime;
      this.BackgroundTime = backgroundTime;
    }

    public DateTime CreatedAt { get; } = DateTime.Now;

    [JsonIgnore]
    public double FitnessTotal
    {
      get
      {
        return this.fitness.Evaluate();
      }
    }

    public double[] Rotation { get; set; }

    [JsonInclude]
    public IList<ISheetPlacement, SheetPlacement> UsedSheets
    {
      get
      {
        return sheetPlacementCollection;
      }

      private set
      {
        if (value is SheetPlacementCollection sheetPlacementCollection)
        {
          this.sheetPlacementCollection = sheetPlacementCollection;
        }
        else
        {
          var newSheetPlacementCollection = new SheetPlacementCollection(value);
          this.sheetPlacementCollection = newSheetPlacementCollection;
        }
      }
    }

    [JsonInclude]
    public IList<INfp> UnplacedParts { get; private set; }

    [JsonInclude]
    public double MergedLength => UsedSheets.Sum(o => o.MergedLength);

    [JsonIgnore]
    public double FitnessSheets
    {
      get
      {
        return this.SheetPlacementFitness.Sheets;
      }
    }

    public int TotalParts { get; }

    [JsonIgnore]
    public double FitnessWastage
    {
      get
      {
        return this.SheetPlacementFitness.Wasted;
      }
    }

    [JsonIgnore]
    public double FitnessBounds
    {
      get
      {
        return this.SheetPlacementFitness.Bounds;
      }
    }

    [JsonIgnore]
    public double FitnessUnplaced
    {
      get
      {
        return this.fitness.Unplaced;
      }
    }

    [JsonIgnore]
    public double FitnessUtilization
    {
      get
      {
        return this.SheetPlacementFitness.Utilization;
      }
    }

    internal int Index { get; set; }

    private ISheetPlacementFitness SheetPlacementFitness => this.fitness;

    public PlacementTypeEnum PlacementType { get; }

    public long PlacePartTime { get; }

    public long BackgroundTime { get; }

    [JsonIgnore]
    public int TotalPlacedCount => this.UsedSheets.Sum(o => o.PartPlacements.Count);

    public int TotalUnplacedCount => this.UnplacedParts.Count;

    [JsonIgnore]
    public int TotalPartsCount => this.TotalPlacedCount + this.UnplacedParts.Count;

    [JsonIgnore]
    public double PartsPlacedPercent => (double)this.TotalPlacedCount / this.TotalPartsCount;

    [JsonIgnore]
    public double TotalPartsArea => this.UsedSheets.Sum(s => s.PartPlacements.Sum(p => p.Part.NetArea));

    [JsonIgnore]
    public double TotalSheetsArea => this.UsedSheets.Sum(s => s.Sheet.Area);

    [JsonIgnore]
    public double MaterialUtilization => Math.Abs(TotalPartsArea / TotalSheetsArea);

    public bool IsValid => !(double.IsNaN(this.FitnessTotal) || this.TotalPartsCount > this.TotalParts);

    public static NestResult FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new InterfaceConverterFactory(typeof(NoFitPolygon), typeof(INfp)));
      options.Converters.Add(new IListInterfaceConverterFactory(typeof(INfp)));
      options.Converters.Add(new WrappableListJsonConverter<ISheetPlacement, SheetPlacement>());
      options.Converters.Add(new SheetPlacementJsonConverter());
      options.Converters.Add(new SheetJsonConverter());
      options.Converters.Add(new NfpJsonConverter());
      options.Converters.Add(new PartPlacementJsonConverter());
      return JsonSerializer.Deserialize<NestResult>(json, options);
    }

    public static NestResult LoadFromFile(string fileName)
    {
      using (StreamReader inputFile = new StreamReader(fileName))
      {
        return FromJson(inputFile.ReadToEnd());
      }
    }

    public override string ToString()
    {
      return $"{fitness.Evaluate()}=ƩB{this.SheetPlacementFitness.Bounds:N0}+ƩS{this.SheetPlacementFitness.Sheets:N0}+ƩW{this.SheetPlacementFitness.Wasted:N0}+ƩU{this.SheetPlacementFitness.Utilization:N0}+U{this.fitness.Unplaced:N0}";
    }

    public override string ToJson(bool writeIndented = true)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new SheetJsonConverter());
      options.Converters.Add(new NfpJsonConverter());
      options.Converters.Add(new PartPlacementJsonConverter());
      options.WriteIndented = writeIndented;
      return JsonSerializer.Serialize(this, options);
    }
  }
}
