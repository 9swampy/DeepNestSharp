namespace DeepNestLib.NestProject
{
  using System;
  using System.IO;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using Light.GuardClauses;

  public class ProjectInfo : IProjectInfo
  {
    public const string FileDialogFilter = "DeepNest Projects (*.dnest)|*.dnest|Json (*.json)|*.json|All files (*.*)|*.*";
    private ISvgNestConfig config;
    private WrappableList<ISheetLoadInfo, SheetLoadInfo> wrappableSheetLoadInfos;

    public ProjectInfo(ISvgNestConfig config) => this.config = config;

    [JsonInclude]
    public IList<IDetailLoadInfo, DetailLoadInfo> DetailLoadInfos { get; private set; } = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();

    [JsonInclude]
    public IList<ISheetLoadInfo, SheetLoadInfo> SheetLoadInfos
    {
      get
      {
        if (this.wrappableSheetLoadInfos == null)
        {
          ISheetLoadInfo sheetLoadInfo;
          if (this.config == null)
          {
            // This is a bit of a fudge for during deserialisation; ultimately this should get set with the deserialised object, just need to get the deserializer past this
            sheetLoadInfo = new SheetLoadInfo(SvgNest.Config);
          }
          else
          {
            sheetLoadInfo = new ConfigSheetLoadInfo(this.Config);
          }

          this.wrappableSheetLoadInfos = new WrappableList<ISheetLoadInfo, SheetLoadInfo>() { sheetLoadInfo };
        }

        return this.wrappableSheetLoadInfos;
      }

      private set
      {
        this.wrappableSheetLoadInfos = (WrappableList<ISheetLoadInfo, SheetLoadInfo>)value;
      }
    }

    [JsonInclude]
    public ISvgNestConfig Config
    {
      get
      {
        return this.config ?? (this.config = SvgNest.Config);
      }

      private set
      {
        this.config = value;
      }
    }

    public static ProjectInfo FromJson(ISvgNestConfig config, string json)
    {
      try
      {
        if (!string.IsNullOrWhiteSpace(json))
        {
          JsonSerializerOptions options = new JsonSerializerOptions();
          options.Converters.Add(new DetailLoadInfoJsonConverter());
          options.Converters.Add(new WrappableListJsonConverter<IDetailLoadInfo, DetailLoadInfo>());
          options.Converters.Add(new WrappableListJsonConverter<ISheetLoadInfo, SheetLoadInfo>());
          options.Converters.Add(new SheetLoadInfoJsonConverter());
          options.Converters.Add(new SvgNestConfigJsonConverter());
          ProjectInfo result = JsonSerializer.Deserialize<ProjectInfo>(json, options);
          ISvgNestConfig deserialized = result.config;
          result.config = config;
          if (deserialized != null)
          {
            SvgNestConfigJsonConverter.FullCopy(deserialized, config);
          }

          return result;
        }

        return new ProjectInfo(config);
      }
      catch (Exception ex)
      {
        return new ProjectInfo(config);
      }
    }

    /// <summary>
    /// Loads the current instance with <see cref="ProjectInfo"/> passed in, maintaining collection instances.
    /// </summary>
    /// <param name="source">Source of ProjectInfo to set.</param>
    public void Load(ProjectInfo source)
    {
      this.DetailLoadInfos.Clear();
      foreach (IDetailLoadInfo p in source.DetailLoadInfos)
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
      foreach (ISheetLoadInfo s in source.SheetLoadInfos)
      {
        this.SheetLoadInfos.Add(s);
      }
#endif
    }

    public string ToJson()
    {
      this.Config.MustBe(this.config);
      this.SheetLoadInfos.MustNotBeNull();

      JsonSerializerOptions options = new JsonSerializerOptions();
      options.Converters.Add(new DetailLoadInfoJsonConverter());
      options.Converters.Add(new WrappableListJsonConverter<IDetailLoadInfo, DetailLoadInfo>());
      options.Converters.Add(new WrappableListJsonConverter<ISheetLoadInfo, SheetLoadInfo>());
      options.Converters.Add(new SheetLoadInfoJsonConverter());
      options.WriteIndented = true;
      options.Converters.Add(new SvgNestConfigJsonConverter());
      return JsonSerializer.Serialize(this, options);
    }

    public static ProjectInfo LoadFromFile(ISvgNestConfig config, string fileName)
    {
      using (StreamReader inputFile = new StreamReader(fileName))
      {
        return FromJson(config, inputFile.ReadToEnd());
      }
    }

    public void Load(ISvgNestConfig config, string filePath)
    {
      ProjectInfo source = LoadFromFile(config, filePath);
      this.Load(source);
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