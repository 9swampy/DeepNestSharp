namespace DeepNestLib.NestProject
{
  using System;
  using System.IO;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using DeepNestLib.IO;
  using Light.GuardClauses;

  public class ProjectInfo : IProjectInfo
  {
    public const string FileDialogFilter = "DeepNest Projects (*.dnest)|*.dnest|Json (*.json)|*.json|All files (*.*)|*.*";
    private IRelativePathHelper relativePathHelper;
    private ISvgNestConfig config;
    private WrappableList<ISheetLoadInfo, SheetLoadInfo> wrappableSheetLoadInfos;

    [JsonConstructor]
    public ProjectInfo()
    {
    }

    public ProjectInfo(ISvgNestConfig config, IRelativePathHelper relativePathHelper)
    {
      relativePathHelper.MustNotBeNull();
      this.config = config;
      this.relativePathHelper = relativePathHelper;
    }

    [JsonInclude]
    public IList<IDetailLoadInfo, DetailLoadInfo> DetailLoadInfos { get; private set; } = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();

    [JsonInclude]
    public IList<ISheetLoadInfo, SheetLoadInfo> SheetLoadInfos
    {
      get
      {
        if (wrappableSheetLoadInfos == null)
        {
          ISheetLoadInfo sheetLoadInfo;
          if (config == null)
          {
            //This is a bit of a fudge for during deserialisation; ultimately this should get set with the deserialised object, just need to get the deserializer past this
            sheetLoadInfo = new SheetLoadInfo(SvgNest.Config);
          }
          else
          {
            sheetLoadInfo = new ConfigSheetLoadInfo(Config);
          }
          wrappableSheetLoadInfos = new WrappableList<ISheetLoadInfo, SheetLoadInfo>() { sheetLoadInfo };
        }

        return wrappableSheetLoadInfos;
      }

      private set
      {
        wrappableSheetLoadInfos = (WrappableList<ISheetLoadInfo, SheetLoadInfo>)value;
      }
    }

    [JsonInclude]
    public ISvgNestConfig Config
    {
      get
      {
        return config ?? (config = SvgNest.Config);
      }

      private set
      {
        config = value;
      }
    }

    /// <summary>
    /// Loads json in to a ProjectInfo with config populated. Needs and instance of config?
    /// </summary>
    /// <param name="config">An instance of SvgNestConfig to populate from the json.</param>
    /// <param name="json">Json representation of the ProjectInfo.</param>
    /// <returns>Populated ProjectInfo.</returns>
    public static ProjectInfo FromJson(ISvgNestConfig config, string json, IRelativePathHelper relativePathHelper)
    {
      try
      {
        relativePathHelper.MustNotBeNull();
        if (!string.IsNullOrWhiteSpace(json))
        {
          var options = new JsonSerializerOptions();
          options.Converters.Add(new DetailLoadInfoJsonConverter(relativePathHelper));
          options.Converters.Add(new WrappableListJsonConverter<IDetailLoadInfo, DetailLoadInfo>());
          options.Converters.Add(new WrappableListJsonConverter<ISheetLoadInfo, SheetLoadInfo>());
          options.Converters.Add(new SheetLoadInfoJsonConverter());
          options.Converters.Add(new SvgNestConfigJsonConverter());
          var result = JsonSerializer.Deserialize<ProjectInfo>(json, options);
          result.relativePathHelper = relativePathHelper;
          var deserialized = result.config;
          result.config = config;
          if (deserialized != null)
          {
            SvgNestConfigJsonConverter.FullCopy(deserialized, config);
          }

          return result;
        }

        return new ProjectInfo(config, relativePathHelper);
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.Print(ex.Message);
        return new ProjectInfo(config, relativePathHelper);
      }
    }

    /// <summary>
    /// Loads the current instance with <see cref="ProjectInfo"/> passed in, maintaining collection instances.
    /// </summary>
    /// <param name="source">Source of ProjectInfo to set.</param>
    public void Load(ProjectInfo source)
    {
      this.DetailLoadInfos.Clear();
      foreach (var p in source.DetailLoadInfos)
      {
        this.DetailLoadInfos.Add(p);
      }

#if DeepNestPort
      var sheetInfoSource = source.SheetLoadInfos.FirstOrDefault();
      if (sheetInfoSource != null)
      {
        var sheetInfoTarget = this.SheetLoadInfos.Single();
        sheetInfoTarget.Width = sheetInfoSource.Width;
        sheetInfoTarget.Height = sheetInfoSource.Height;
        sheetInfoTarget.Quantity = sheetInfoSource.Quantity;
      }
#else
      this.SheetLoadInfos.Clear();
      foreach (var s in source.SheetLoadInfos)
      {
        this.SheetLoadInfos.Add(s);
      }
#endif
    }

    public string ToJson()
    {
      relativePathHelper.MustNotBeNull();
      Config.MustBe(this.config);
      SheetLoadInfos.MustNotBeNull();

      var options = new JsonSerializerOptions();
      options.Converters.Add(new DetailLoadInfoJsonConverter(relativePathHelper));
      options.Converters.Add(new WrappableListJsonConverter<IDetailLoadInfo, DetailLoadInfo>());
      options.Converters.Add(new WrappableListJsonConverter<ISheetLoadInfo, SheetLoadInfo>());
      options.Converters.Add(new SheetLoadInfoJsonConverter());
      options.WriteIndented = true;
      options.Converters.Add(new SvgNestConfigJsonConverter());
      return JsonSerializer.Serialize(this, options);
    }

    public static ProjectInfo LoadFromFile(ISvgNestConfig config, string fileName, IRelativePathHelper relativePathHelper)
    {
      using (StreamReader inputFile = new StreamReader(fileName))
      {
        return LoadFromStream(config, inputFile, relativePathHelper);
      }
    }

    public static ProjectInfo LoadFromStream(ISvgNestConfig config, StreamReader stream, IRelativePathHelper relativePathHelper)
    {
      relativePathHelper.MustNotBeNull();
      return FromJson(config, stream.ReadToEnd(), relativePathHelper);
    }

    public void Load(ISvgNestConfig config, IRelativePathHelper relativePathHelperParameter, string filePath)
    {
      relativePathHelperParameter.MustNotBeNull();
      if (relativePathHelper != null)
      {
        relativePathHelper.MustBe(relativePathHelperParameter);
      }

      var source = LoadFromFile(config, filePath, relativePathHelperParameter);
      Load(source);
    }

    public void Save(string fileName)
    {
      using (StreamWriter outputFile = new StreamWriter(fileName))
      {
        outputFile.WriteLine(this.ToJson());
      }
    }
  }
}