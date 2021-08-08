using System.IO;

namespace DeepNestLib.NestProject
{
  public class DetailLoadInfo : IDetailLoadInfo
  {
    public string Name => new FileInfo(Path).Name;

    public string Path { get; set; }

    public int Quantity { get; set; } = 1;

    public bool IsIncluded { get; set; } = true;

    public bool IsPriority { get; set; } = false;

    public bool IsMultiplied { get; set; } = true;

    public AnglesEnum StrictAngle { get; set; } = AnglesEnum.None;
  }
}
