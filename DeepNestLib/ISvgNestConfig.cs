namespace DeepNestLib
{
  public interface ISvgNestConfig
  {
    double ClipperScale { get; set; }

    bool ClipByHull { get; set; }

    double CurveTolerance { get; set; }

    bool DrawSimplification { get; set; }

    bool ExploreConcave { get; set; }

    bool MergeLines { get; set; }

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

    float Tolerance { get; set; }

    float ToleranceSvg { get; set; }

    bool UseHoles { get; set; }

    bool UseParallel { get; set; }

    bool StrictAngles { get; set; }

    int Multiplier { get; set; }

    int ParallelNests { get; }

    bool ShowPartPositions { get; set; }
  }
}