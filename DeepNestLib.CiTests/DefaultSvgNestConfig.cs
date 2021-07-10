namespace DeepNestLib.CiTests
{
  public class DefaultSvgNestConfig : ISvgNestConfig
  {
    public double ClipperScale { get; set; } = 10000000;

    public double CurveTolerance { get; set; } = 0.72;

    public bool ExploreConcave { get; set; } = false;

    public bool MergeLines { get; set; } = false;

    public int MutationRate { get; set; } = 10;

    public bool OffsetTreePhase { get; set; } = true;

    public PlacementTypeEnum PlacementType { get; set; } = PlacementTypeEnum.Gravity;

    public int PopulationSize { get; set; } = 10;

    public int Rotations { get; set; } = 4;

    public int SaveAsFileTypeIndex { get; set; } = 1;

    public double Scale { get; set; } = 25;

    public int SheetHeight { get; set; } = 395;

    public int SheetQuantity { get; set; } = 10;

    public double SheetSpacing { get; set; } = 0;

    public int SheetWidth { get; set; } = 595;

    public bool Simplify { get; set; } = false;

    public double Spacing { get; set; } = 0;

    public double TimeRatio { get; set; } = 0.5;

    public bool UseHoles { get; set; } = false;

    public bool ClipByHull { get; set; } = true;

    public bool DrawSimplification { get; set; } = true;

    public bool UseParallel { get; set; } = false;

    public float Tolerance { get; set; } = 2f;

    public float ToleranceSvg { get; set; } = 0.005f;

    public bool StrictAngles { get; set; } = false;

    public int Multiplier { get; set; } = 1;

    public int ParallelNests => 1;

    public bool ShowPartPositions { get; set; } = false;
  }
}
