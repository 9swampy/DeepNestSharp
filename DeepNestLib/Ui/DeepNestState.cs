namespace DeepNestLib.Ui
{
  using System.Collections.Generic;
  using System.Text.Json;

  public class DeepNestState
  {
    public List<DetailLoadInfo> PartInfos { get; set; }

    public List<SheetLoadInfoDto> SheetInfos { get; set; }

    public TopNestResultsCollection NestResults { get; set; }

    public List<INfp> Polygons { get; set; }

    public List<INfp> Sheets { get; set; }

    public string ToJson()
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new NfpJsonConverter());
      return JsonSerializer.Serialize(this, options);
    }

    public static DeepNestState FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new NfpJsonConverter());
      return JsonSerializer.Deserialize<DeepNestState>(json, options);
    }
  }
}
