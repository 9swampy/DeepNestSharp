namespace DeepNestLib
{
  using System;
  using DeepNestLib.NestProject;

  public class TestSvgNestConfig : ISvgNestConfig
  {
    internal static ISvgNestConfig Default => new TestSvgNestConfig();

    public double ClipperScale { get; set; } = 10000000;

    public double CurveTolerance { get; set; } = 0.72D;

    public bool ExploreConcave { get; set; } = false;

    public bool MergeLines { get; set; } = false;

    public int MutationRate { get; set; } = 10;

    public bool OffsetTreePhase { get; set; } = true;

    public bool OverlapDetection { get; set; } = true;

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

    public bool DifferentiateChildren { get; set; } = true;

    public bool DrawSimplification { get; set; } = true;

    public bool UseParallel { get; set; } = false;

    public double Tolerance { get; set; } = 2;

    public double ToleranceSvg { get; set; } = 0.005;

    public double TopDiversity { get; set; } = 0.0001;

    public AnglesEnum StrictAngles { get; set; } = AnglesEnum.None;

    public int Multiplier { get; set; } = 1;

    public int ParallelNests { get; set; } = 10;

    public bool ShowPartPositions { get; set; } = false;

    public string LastNestFilePath { get; set; }

    public string LastDebugFilePath { get; set; }

    public bool UseMinkowskiCache { get; set; } = false;

    public int ProcreationTimeout { get; set; } = 1000;

    public bool ExportExecutions { get; set; } = false;

    public string ExportExecutionPath { get; set; }

    public bool UseDllImport { get; set; } = false;

    public bool UsePriority { get; set; } = false;

    public int MutationRateMin => 1;

    public int MutationRateMax => 60;

    public string ToJson()
    {
      return SvgNestConfigJsonConverter.ToJson(this);
    }
  }
}
