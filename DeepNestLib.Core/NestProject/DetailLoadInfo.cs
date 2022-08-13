namespace DeepNestLib.NestProject
{
  using System.IO;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using DeepNestLib.IO;

  public class DetailLoadInfo : Saveable, IDetailLoadInfo
  {
    [JsonIgnore]
    public string Name
    {
      get
      {
        if (string.IsNullOrWhiteSpace(Path))
        {
          return "N/A";
        }

        return new FileInfo(Path).Name;
      }
    }

    public string Path { get; set; }

    public int Quantity { get; set; } = 1;

    public bool IsDifferentiated { get; set; } = false;

    [JsonIgnore]
    public bool IsExists
    {
      get
      {
        if (string.IsNullOrWhiteSpace(Path))
        {
          return false;
        }

        return new FileInfo(this.Path).Exists;
      }
    }

    public bool IsIncluded { get; set; } = true;

    public bool IsPriority { get; set; } = false;

    public bool IsMultiplied { get; set; } = true;

    public AnglesEnum StrictAngle { get; set; } = AnglesEnum.None;

    public static DetailLoadInfo FromJson(string json)
    {
      return JsonSerializer.Deserialize<DetailLoadInfo>(json);
    }

    public IDetailLoadInfo Clone()
    {
      return FromJson(this.ToJson());
    }

    public override string ToJson(bool writeIndented = false)
    {
      var options = new JsonSerializerOptions();
      options.WriteIndented = writeIndented;
      return JsonSerializer.Serialize(this, options);
    }
  }
}
