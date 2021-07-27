namespace DeepNestLib.NestProject
{
  public interface IDetailLoadInfo
  {
    bool IsIncluded { get; set; }

    bool IsMultiplied { get; set; }

    bool IsPriority { get; set; }

    string Name { get; set; }

    string Path { get; set; }

    int Quantity { get; set; }

    AnglesEnum StrictAngle { get; set; }
  }
}