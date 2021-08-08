namespace DeepNestLib.NestProject
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class ProjectInfo : IProjectInfo
  {
    public const string FileDialogFilter = "DeepNest Projects (*.dnest)|*.dnest|Json (*.json)|*.json|All files (*.*)|*.*";

    [JsonInclude]
    public IList<IDetailLoadInfo, DetailLoadInfo> DetailLoadInfos { get; private set; } = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();

    [JsonInclude]
    public IList<ISheetLoadInfo, SheetLoadInfo> SheetLoadInfos { get; private set; } = new WrappableList<ISheetLoadInfo, SheetLoadInfo>() { new SheetLoadInfo() };

    [JsonInclude]
    public ISvgNestConfig Config => SvgNest.Config;

    public static ProjectInfo FromJson(string json)
    {
      try
      {
        if (!string.IsNullOrWhiteSpace(json))
        {
          var options = new JsonSerializerOptions();
          options.Converters.Add(new DetailLoadInfoJsonConverter());
          options.Converters.Add(new WrappableListJsonConverter<IDetailLoadInfo, DetailLoadInfo>());
          options.Converters.Add(new WrappableListJsonConverter<ISheetLoadInfo, SheetLoadInfo>());
          options.Converters.Add(new SheetLoadInfoJsonConverter());
          options.Converters.Add(new SvgNestConfigJsonConverter());
          return JsonSerializer.Deserialize<ProjectInfo>(json, options);
        }

        return new ProjectInfo();
      }
      catch (Exception ex)
      {
        return new ProjectInfo();
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

      var sheetInfoSource = source.SheetLoadInfos.SingleOrDefault();
      if (sheetInfoSource != null)
      {
        var sheetInfoTarget = this.SheetLoadInfos.Single();
        sheetInfoTarget.Width = sheetInfoSource.Width;
        sheetInfoTarget.Height = sheetInfoSource.Height;
        sheetInfoTarget.Quantity = sheetInfoSource.Quantity;
      }
    }

    public string ToJson()
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new DetailLoadInfoJsonConverter());
      options.Converters.Add(new WrappableListJsonConverter<IDetailLoadInfo, DetailLoadInfo>());
      options.Converters.Add(new WrappableListJsonConverter<ISheetLoadInfo, SheetLoadInfo>());
      options.Converters.Add(new SheetLoadInfoJsonConverter());
      options.Converters.Add(new SvgNestConfigJsonConverter());
      return JsonSerializer.Serialize(this, options);
    }

    public static ProjectInfo LoadFromFile(string fileName)
    {
      using (StreamReader inputFile = new StreamReader(fileName))
      {
        return FromJson(inputFile.ReadToEnd());
      }
    }

    public void Load(string filePath)
    {
      var source = LoadFromFile(filePath);
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