namespace DeepNestLib.CiTests
{
  using System;
  using DeepNestLib.NestProject;

  public class DefaultSvgNestConfig : SvgNestConfig, ISvgNestConfig
  {
    public new double ClipperScale { get; set; } = 10000000;

    public new double CurveTolerance { get; set; } = 0.72D;

    public new bool ExploreConcave { get; set; } = false;

    public new bool MergeLines { get; set; } = false;

    public new int MutationRate { get; set; } = 10;

    public new bool OffsetTreePhase { get; set; } = true;

    public new PlacementTypeEnum PlacementType { get; set; } = PlacementTypeEnum.Gravity;

    public new int PopulationSize { get; set; } = 10;

    public new int Rotations { get; set; } = 4;

    public new int SaveAsFileTypeIndex { get; set; } = 1;

    public new double Scale { get; set; } = 25;

    public new int SheetHeight { get; set; } = 395;

    public new int SheetQuantity { get; set; } = 10;

    public new double SheetSpacing { get; set; } = 0;

    public new int SheetWidth { get; set; } = 595;

    public new bool Simplify { get; set; } = false;

    public new double Spacing { get; set; } = 0;

    public new double TimeRatio { get; set; } = 0.5;

    public new bool UseHoles { get; set; } = false;

    public new bool ClipByHull { get; set; } = true;

    public new bool DrawSimplification { get; set; } = true;

    public new bool UseParallel { get; set; } = false;

    public new double Tolerance { get; set; } = 2;

    public new double ToleranceSvg { get; set; } = 0.005;

    public new AnglesEnum StrictAngles { get; set; } = AnglesEnum.None;

    public new int Multiplier { get; set; } = 1;

    public new int ParallelNests { get; set; } = 10;

    public new bool ShowPartPositions { get; set; } = false;

    public new string ToJson()
    {
      throw new NotImplementedException();
    }
  }
}
