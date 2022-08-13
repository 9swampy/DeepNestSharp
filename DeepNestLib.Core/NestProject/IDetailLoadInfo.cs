namespace DeepNestLib.NestProject
{
  public interface IDetailLoadInfo
  {
    bool IsDifferentiated { get; set; }

    bool IsIncluded { get; set; }

    bool IsMultiplied { get; set; }

    bool IsPriority { get; set; }

    /// <summary>
    /// Gets the name of the file (excluding path).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets or sets the full name of the file (including path).
    /// </summary>
    string Path { get; set; }

    int Quantity { get; set; }

    AnglesEnum StrictAngle { get; set; }

    bool IsExists { get; }

    IDetailLoadInfo Clone();
  }
}