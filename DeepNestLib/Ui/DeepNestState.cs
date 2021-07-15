namespace DeepNestLib.Ui
{
  using System.Collections.Generic;
  using System.Text.Json;

  public class DeepNestState : IDeepNestState
  {
    public readonly static DeepNestState Default = new DeepNestState();

    public List<DetailLoadInfo> PartInfos { get; set; }

    public List<ISheetLoadInfo> SheetInfos { get; set; }

    public TopNestResultsCollection NestResults { get; set; }

    public ICollection<INfp> Polygons { get; set; }

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
