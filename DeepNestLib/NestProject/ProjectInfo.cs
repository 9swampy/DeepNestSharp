namespace DeepNestLib.NestProject
{
  using System.Collections.Generic;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class ProjectInfo
  {
    [JsonInclude]
    public IList<IDetailLoadInfo> DetailLoadInfos { get; private set; } = new List<IDetailLoadInfo>();

    [JsonInclude]
    public IList<ISheetLoadInfo> SheetLoadInfos { get; private set; } = new List<ISheetLoadInfo>();

    public static ProjectInfo FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new DetailLoadInfoJsonConverter());
      options.Converters.Add(new SheetLoadInfoJsonConverter());
      return JsonSerializer.Deserialize<ProjectInfo>(json, options);
    }

    public string ToJson()
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new DetailLoadInfoJsonConverter());
      options.Converters.Add(new SheetLoadInfoJsonConverter());
      return JsonSerializer.Serialize(this, options);
    }
  }
}