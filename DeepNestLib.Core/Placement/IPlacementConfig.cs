namespace DeepNestLib.Placement
{
  public interface IPlacementConfig
  {
    double ClipperScale { get; set; }

    double CurveTolerance { get; set; }

    bool ExportExecutions { get; set; }

    string ExportExecutionPath { get; set; }

    bool MergeLines { get; set; }

    PlacementTypeEnum PlacementType { get; set; }

    int Rotations { get; set; }

    double Scale { get; set; }

    double TimeRatio { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the C++ imported Minkowski implementation should be used.
    /// </summary>
    bool UseDllImport { get; set; }

    /// <summary>
    /// Gets or sets a flag that indicates whether Priority settings should be applied.
    /// </summary>
    bool UsePriority { get; set; }

    /// <summary>
    /// Gets or sets a flag that indicates whether Overlap Dection should be applied. Slows the nest quite a lot so only use if you're getting overlapping parts.
    /// </summary>
    bool OverlapDetection { get; set; }
  }
}