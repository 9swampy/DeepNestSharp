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
#pragma warning disable CS0612 // Type or member is obsolete
      : this()
#pragma warning restore CS0612 // Type or member is obsolete
    {
      this.TotalParts = totalParts;
      this.UsedSheets = allPlacements;
      this.UnplacedParts = unplacedParts;
      this.PlacementType = placementType;
      this.PlacePartTime = placePartTime;
      this.BackgroundTime = backgroundTime;
    }

    private ISheetPlacementFitness SheetPlacementFitness => this.fitness;

    public DateTime CreatedAt { get; } = DateTime.Now;

    [JsonIgnore]
    public double Fitness
    {
      get
      {
        return this.fitness.Evaluate();
      }
    }

    public double[] Rotation { get; set; }

    [JsonInclude]
    public IList<ISheetPlacement, SheetPlacement> UsedSheets { get; private set; }

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
    public double MaterialWasted
    {
      get
      {
        return this.SheetPlacementFitness.MaterialWasted;
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

    internal int index { get; set; }

    public static NestResult FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      // options.Converters.Add(new ListJsonConverter<INfp>());
      options.Converters.Add(new InterfaceConverterFactory(typeof(NFP), typeof(INfp)));
      options.Converters.Add(new IListInterfaceConverterFactory(typeof(INfp)));
      // options.Converters.Add(new IListInterfaceConverterFactory(typeof(NFP)));
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

    public PlacementTypeEnum PlacementType { get; }

    public long PlacePartTime { get; }
    public long BackgroundTime { get; }

    [JsonIgnore]
    public int TotalPlacedCount => this.UsedSheets.Sum(o => o.PartPlacements.Count);

    [JsonIgnore]
    public int TotalPartsCount => this.TotalPlacedCount + this.UnplacedParts.Count;

    [JsonIgnore]
    public double PartsPlacedPercent => (double)this.TotalPlacedCount / this.TotalPartsCount;

    [JsonIgnore]
    public double TotalPartsArea => this.UsedSheets.Sum(s => s.PartPlacements.Sum(p => p.Part.Area));

    [JsonIgnore]
    public double TotalSheetsArea => this.UsedSheets.Sum(s => s.Sheet.Area);

    [JsonIgnore]
    public double MaterialUtilization => Math.Abs(TotalPartsArea / TotalSheetsArea);

    public bool IsValid => !(double.IsNaN(this.Fitness) || this.TotalPartsCount > this.TotalParts);

    public override string ToString()
    {
      return $"{fitness.Evaluate()}=ƩB{this.SheetPlacementFitness.Bounds:N0}+ƩS{this.SheetPlacementFitness.Sheets:N0}+ƩW{this.SheetPlacementFitness.MaterialWasted:N0}+ƩU{this.SheetPlacementFitness.MaterialUtilization:N0}+U{this.fitness.Unplaced:N0}";
    }

    public override string ToJson()
    {
      var options = new JsonSerializerOptions();
      //options.Converters.Add(new InterfaceConverterFactory(typeof(NFP), typeof(INfp)));
      //options.Converters.Add(new ListJsonConverter<INfp>());
      //options.Converters.Add(new IListInterfaceConverterFactory(typeof(NFP)));
      //options.Converters.Add(new WrappableListJsonConverter<ISheetPlacement, SheetPlacement>());
      options.Converters.Add(new SheetJsonConverter());
      options.Converters.Add(new NfpJsonConverter());
      options.Converters.Add(new PartPlacementJsonConverter());
      return JsonSerializer.Serialize(this, options);
    }
  }

  public class InterfaceConverter<M, I> : JsonConverter<I> where M : class, I
  {
    public override I Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      return JsonSerializer.Deserialize<M>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, I value, JsonSerializerOptions options) { }
  }

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

  public class IListInterfaceConverterFactory : JsonConverterFactory
  {
    public IListInterfaceConverterFactory(Type interfaceType)
    {
      this.InterfaceType = interfaceType;
    }

    public Type InterfaceType { get; }

    public override bool CanConvert(Type typeToConvert)
    {
      if (typeToConvert.Equals(typeof(IList<>).MakeGenericType(this.InterfaceType))
       && typeToConvert.GenericTypeArguments[0].Equals(this.InterfaceType))
      {
        return true;
      }

      return false;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      return (JsonConverter)Activator.CreateInstance(
          typeof(ListConverter<>).MakeGenericType(this.InterfaceType));
    }
  }

  public class InterfaceConverterFactory : JsonConverterFactory
  {
    public InterfaceConverterFactory(Type concrete, Type interfaceType)
    {
      this.ConcreteType = concrete;
      this.InterfaceType = interfaceType;
    }

    public Type ConcreteType { get; }

    public Type InterfaceType { get; }

    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert == this.InterfaceType;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      var converterType = typeof(InterfaceConverter<,>).MakeGenericType(this.ConcreteType, this.InterfaceType);

      return (JsonConverter)Activator.CreateInstance(converterType);
    }
  }
}
