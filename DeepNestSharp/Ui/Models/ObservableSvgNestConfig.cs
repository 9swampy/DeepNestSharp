namespace DeepNestSharp.Ui.Models
{
  using DeepNestLib;
  using DeepNestLib.NestProject;

  public class ObservableSvgNestConfig : ObservablePropertyObject, ISvgNestConfig
  {
    private readonly ISvgNestConfig svgNestConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableSvgNestConfig"/> class.
    /// </summary>
    /// <param name="svgNestConfig">The ISvgNestConfig to wrap.</param>
    public ObservableSvgNestConfig(ISvgNestConfig svgNestConfig) => this.svgNestConfig = svgNestConfig;

    public double ClipperScale
    {
      get => svgNestConfig.ClipperScale;
      set => SetProperty(nameof(ClipperScale), () => svgNestConfig.ClipperScale, v => svgNestConfig.ClipperScale = v, value);
    }

    public bool ClipByHull
    {
      get => svgNestConfig.ClipByHull;
      set => SetProperty(nameof(ClipByHull), () => svgNestConfig.ClipByHull, v => svgNestConfig.ClipByHull = v, value);
    }

    public double CurveTolerance
    {
      get => svgNestConfig.CurveTolerance;
      set => SetProperty(nameof(CurveTolerance), () => svgNestConfig.CurveTolerance, v => svgNestConfig.CurveTolerance = v, value);
    }

    public bool DrawSimplification
    {
      get => svgNestConfig.DrawSimplification;
      set => SetProperty(nameof(DrawSimplification), () => svgNestConfig.DrawSimplification, v => svgNestConfig.DrawSimplification = v, value);
    }

    public bool ExploreConcave
    {
      get => svgNestConfig.ExploreConcave;
      set => SetProperty(nameof(ExploreConcave), () => svgNestConfig.ExploreConcave, v => svgNestConfig.ExploreConcave = v, value);
    }

    public bool MergeLines
    {
      get => svgNestConfig.MergeLines;
      set => SetProperty(nameof(MergeLines), () => svgNestConfig.MergeLines, v => svgNestConfig.MergeLines = v, value);
    }

    public int MutationRate
    {
      get => svgNestConfig.MutationRate;
      set => SetProperty(nameof(MutationRate), () => svgNestConfig.MutationRate, v => svgNestConfig.MutationRate = v, value);
    }

    public bool OffsetTreePhase
    {
      get => svgNestConfig.OffsetTreePhase;
      set => SetProperty(nameof(OffsetTreePhase), () => svgNestConfig.OffsetTreePhase, v => svgNestConfig.OffsetTreePhase = v, value);
    }

    public PlacementTypeEnum PlacementType
    {
      get => svgNestConfig.PlacementType;
      set => SetProperty(nameof(PlacementType), () => svgNestConfig.PlacementType, v => svgNestConfig.PlacementType = v, value);
    }

    public int PopulationSize
    {
      get => svgNestConfig.PopulationSize;
      set => SetProperty(nameof(PopulationSize), () => svgNestConfig.PopulationSize, v => svgNestConfig.PopulationSize = v, value);
    }

    public int Rotations
    {
      get => svgNestConfig.Rotations;
      set => SetProperty(nameof(Rotations), () => svgNestConfig.Rotations, v => svgNestConfig.Rotations = v, value);
    }

    public int SaveAsFileTypeIndex
    {
      get => svgNestConfig.SaveAsFileTypeIndex;
      set => SetProperty(nameof(SaveAsFileTypeIndex), () => svgNestConfig.SaveAsFileTypeIndex, v => svgNestConfig.SaveAsFileTypeIndex = v, value);
    }

    public double Scale
    {
      get => svgNestConfig.Scale;
      set => SetProperty(nameof(Scale), () => svgNestConfig.Scale, v => svgNestConfig.Scale = v, value);
    }

    public int SheetHeight
    {
      get => svgNestConfig.SheetHeight;
      set => SetProperty(nameof(SheetHeight), () => svgNestConfig.SheetHeight, v => svgNestConfig.SheetHeight = v, value);
    }

    public int SheetQuantity
    {
      get => svgNestConfig.SheetQuantity;
      set => SetProperty(nameof(SheetQuantity), () => svgNestConfig.SheetQuantity, v => svgNestConfig.SheetQuantity = v, value);
    }

    public double SheetSpacing
    {
      get => svgNestConfig.SheetSpacing;
      set => SetProperty(nameof(SheetSpacing), () => svgNestConfig.SheetSpacing, v => svgNestConfig.SheetSpacing = v, value);
    }

    public int SheetWidth
    {
      get => svgNestConfig.SheetWidth;
      set => SetProperty(nameof(SheetWidth), () => svgNestConfig.SheetWidth, v => svgNestConfig.SheetWidth = v, value);
    }

    public bool Simplify
    {
      get => svgNestConfig.Simplify;
      set => SetProperty(nameof(Simplify), () => svgNestConfig.Simplify, v => svgNestConfig.Simplify = v, value);
    }

    public double Spacing
    {
      get => svgNestConfig.Spacing;
      set => SetProperty(nameof(Spacing), () => svgNestConfig.Spacing, v => svgNestConfig.Spacing = v, value);
    }

    public double TimeRatio
    {
      get => svgNestConfig.TimeRatio;
      set => SetProperty(nameof(TimeRatio), () => svgNestConfig.TimeRatio, v => svgNestConfig.TimeRatio = v, value);
    }

    public double Tolerance
    {
      get => svgNestConfig.Tolerance;
      set => SetProperty(nameof(Tolerance), () => svgNestConfig.Tolerance, v => svgNestConfig.Tolerance = v, value);
    }

    public double ToleranceSvg
    {
      get => svgNestConfig.ToleranceSvg;
      set => SetProperty(nameof(ToleranceSvg), () => svgNestConfig.ToleranceSvg, v => svgNestConfig.ToleranceSvg = v, value);
    }

    public bool UseHoles
    {
      get => svgNestConfig.UseHoles;
      set => SetProperty(nameof(UseHoles), () => svgNestConfig.UseHoles, v => svgNestConfig.UseHoles = v, value);
    }

    public bool UseParallel
    {
      get => svgNestConfig.UseParallel;
      set => SetProperty(nameof(UseParallel), () => svgNestConfig.UseParallel, v => svgNestConfig.UseParallel = v, value);
    }

    public AnglesEnum StrictAngles
    {
      get => svgNestConfig.StrictAngles;
      set => SetProperty(nameof(StrictAngles), () => svgNestConfig.StrictAngles, v => svgNestConfig.StrictAngles = v, value);
    }

    public int Multiplier
    {
      get => svgNestConfig.Multiplier;
      set => SetProperty(nameof(Multiplier), () => svgNestConfig.Multiplier, v => svgNestConfig.Multiplier = v, value);
    }

    public int ParallelNests
    {
      get => svgNestConfig.ParallelNests;
      set => SetProperty(nameof(ParallelNests), () => svgNestConfig.ParallelNests, v => svgNestConfig.ParallelNests = v, value);
    }

    public bool ShowPartPositions
    {
      get => svgNestConfig.ShowPartPositions;
      set => SetProperty(nameof(ShowPartPositions), () => svgNestConfig.ShowPartPositions, v => svgNestConfig.ShowPartPositions = v, value);
    }
  }
}
