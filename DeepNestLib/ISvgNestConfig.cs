namespace DeepNestLib
{
  using DeepNestLib.NestProject;

  public interface ISvgNestConfig
  {
    double ClipperScale { get; set; }

    /// <summary>
    /// Gets or sets whether to clip the simplified polygon used in nesting by the hull.
    /// This often improves the fit to the original part but may slightly increase the number
    /// of points in the simplification and accordingly may marginally slow the nest.
    /// Requires a restart of the application because it's not a part of the cache key so
    /// you have to restart to reinitialise the cache.
    /// </summary>
    bool ClipByHull { get; set; }

    double CurveTolerance { get; set; }

    bool DrawSimplification { get; set; }

    bool ExploreConcave { get; set; }

    bool ExportExecutions { get; set; }

    string ExportExecutionPath { get; set; }

    /// <summary>
    /// Gets or sets the last path used for Nest files (dnest, dnr, dxf).
    /// </summary>
    string LastNestFilePath { get; set; }

    /// <summary>
    /// Gets or sets the last path used for Debugging files (dnsp, dnsnfp, dnnfps).
    /// </summary>
    string LastDebugFilePath { get; set; }

    bool MergeLines { get; set; }

    bool UseMinkowskiCache { get; set; }

    int MutationRate { get; set; }

    bool OffsetTreePhase { get; set; }

    PlacementTypeEnum PlacementType { get; set; }

    int PopulationSize { get; set; }

    int Rotations { get; set; }

    int SaveAsFileTypeIndex { get; set; }

    double Scale { get; set; }

    int SheetHeight { get; set; }

    int SheetQuantity { get; set; }

    double SheetSpacing { get; set; }

    int SheetWidth { get; set; }

    bool Simplify { get; set; }

    double Spacing { get; set; }

    double TimeRatio { get; set; }

    /// <summary>
    /// Gets or sets max bound for bezier->line segment conversion, in native SVG units.
    /// </summary>
    double Tolerance { get; set; }

    /// <summary>
    /// Fudge factor for browser inaccuracy in SVG unit handling.
    /// </summary>

    double ToleranceSvg { get; set; }

    bool UseHoles { get; set; }

    bool UseParallel { get; set; }

    /// <summary>
    /// Gets a value that indicates whether Priority settings should be applied.
    /// </summary>
    bool UsePriority { get; set; }

    /// <summary>
    /// If set then parts will be restricted to <see cref="StrictAngles"/>.
    /// If also set on an individual part, part wins.
    /// </summary>
    AnglesEnum StrictAngles { get; set; }

    int Multiplier { get; set; }

    int ParallelNests { get; set; }

    bool ShowPartPositions { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the C++ imported Minkowski implementation should be used.
    /// </summary>
    bool UseDllImport { get; set; }

    string ToJson();
  }
}