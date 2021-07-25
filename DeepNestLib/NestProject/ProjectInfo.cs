namespace DeepNestLib.NestProject
{
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class ProjectInfo : IProjectInfo
  {
    public const string FileDialogFilter = "DeepNest Projects (*.dnest)|*.dnest|Json (*.json)|*.json|All files (*.*)|*.*";

    [JsonInclude]
    public IList<IDetailLoadInfo> DetailLoadInfos { get; private set; } = new List<IDetailLoadInfo>();

    [JsonInclude]
    public IList<ISheetLoadInfo> SheetLoadInfos { get; private set; } = new List<ISheetLoadInfo>() { new SheetLoadInfo() };

    public static ProjectInfo FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new DetailLoadInfoJsonConverter());
      options.Converters.Add(new SheetLoadInfoJsonConverter());
      return JsonSerializer.Deserialize<ProjectInfo>(json, options);
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
      options.Converters.Add(new SheetLoadInfoJsonConverter());
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
  }
}