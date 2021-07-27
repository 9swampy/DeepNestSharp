namespace DeepNestLib.NestProject
{
  public class DetailLoadInfo : IDetailLoadInfo
  {
    public string Name { get; set; }

    public string Path { get; set; }

    public int Quantity { get; set; }

    public bool IsIncluded { get; set; }

    public bool IsPriority { get; set; }

    public bool IsMultiplied { get; set; }

    public AnglesEnum StrictAngle { get; set; }
  }
}
