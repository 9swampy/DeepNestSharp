namespace DeepNestLib.NestProject
{
  using System.IO;
  using System.Text.Json;
  using DeepNestLib.IO;

  public class DetailLoadInfo : Saveable, IDetailLoadInfo
  {
    public string Name => new FileInfo(Path).Name;

    public string Path { get; set; }

    public int Quantity { get; set; } = 1;

    public bool IsIncluded { get; set; } = true;

    public bool IsPriority { get; set; } = false;

    public bool IsMultiplied { get; set; } = true;

    public AnglesEnum StrictAngle { get; set; } = AnglesEnum.None;

    public override string ToJson(bool writeIndented = false)
    {
      var options = new JsonSerializerOptions();
      options.WriteIndented = writeIndented;
      return JsonSerializer.Serialize(this, options);
    }
  }
}
