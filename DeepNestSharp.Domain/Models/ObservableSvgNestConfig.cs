namespace DeepNestSharp.Domain.Models
{
  using DeepNestLib;
  using DeepNestLib.NestProject;
  using System.ComponentModel;

  public class ObservableSvgNestConfig : ObservablePropertyObject, ISvgNestConfig
  {
    private readonly ISvgNestConfig svgNestConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableSvgNestConfig"/> class.
    /// </summary>
    /// <param name="svgNestConfig">The ISvgNestConfig to wrap.</param>
    public ObservableSvgNestConfig(ISvgNestConfig svgNestConfig) => this.svgNestConfig = svgNestConfig;

    /// <inheritdoc />
    [Category("Nest Settings")]
    public double ClipperScale
    {
      get => svgNestConfig.ClipperScale;
      set => SetProperty(nameof(ClipperScale), () => svgNestConfig.ClipperScale, v => svgNestConfig.ClipperScale = v, value);
}

    [Description("Gets or sets whether to clip the simplified polygon used in nesting by the hull. " +
      "This often improves the fit to the original part but may slightly increase the number " +
      "of points in the simplification and accordingly may marginally slow the nest. " +
      "Requires a restart of the application because it's not a part of the cache key so " +
      "you have to restart to reinitialise the cache."), Category("Simplifications")]
    /// <inheritdoc />
    public bool ClipByHull
    {
      get => svgNestConfig.ClipByHull;
      set => SetProperty(nameof(ClipByHull), () => svgNestConfig.ClipByHull, v => svgNestConfig.ClipByHull = v, value);
    }

    /// <inheritdoc />
    [Category("Simplifications")]
    public double CurveTolerance
    {
      get => svgNestConfig.CurveTolerance;
      set => SetProperty(nameof(CurveTolerance), () => svgNestConfig.CurveTolerance, v => svgNestConfig.CurveTolerance = v, value);
    }

    /// <inheritdoc />
    [Category("Simplifications")]
    public bool DrawSimplification
    {
      get => svgNestConfig.DrawSimplification;
      set => SetProperty(nameof(DrawSimplification), () => svgNestConfig.DrawSimplification, v => svgNestConfig.DrawSimplification = v, value);
    }

    /// <inheritdoc />
    [Category("Simplifications")]
    public bool ExploreConcave
    {
      get => svgNestConfig.ExploreConcave;
      set => SetProperty(nameof(ExploreConcave), () => svgNestConfig.ExploreConcave, v => svgNestConfig.ExploreConcave = v, value);
    }

    /// <inheritdoc />
    [Browsable(false)]
    public override bool IsDirty => true;

    /// <inheritdoc />
    [Category("Unimplemented")]
    public bool MergeLines
    {
      get => svgNestConfig.MergeLines;
      set => SetProperty(nameof(MergeLines), () => svgNestConfig.MergeLines, v => svgNestConfig.MergeLines = v, value);
    }

    /// <inheritdoc />
    [Category("Genetic Algorithm")]
    public int MutationRate
    {
      get => svgNestConfig.MutationRate;
      set => SetProperty(nameof(MutationRate), () => svgNestConfig.MutationRate, v => svgNestConfig.MutationRate = v, value);
    }

    /// <inheritdoc />
    [Category("Nest Settings")]
    public bool OffsetTreePhase
    {
      get => svgNestConfig.OffsetTreePhase;
      set => SetProperty(nameof(OffsetTreePhase), () => svgNestConfig.OffsetTreePhase, v => svgNestConfig.OffsetTreePhase = v, value);
    }

    /// <inheritdoc />
    [Category("Nest Settings")]
    public PlacementTypeEnum PlacementType
    {
      get => svgNestConfig.PlacementType;
      set => SetProperty(nameof(PlacementType), () => svgNestConfig.PlacementType, v => svgNestConfig.PlacementType = v, value);
    }

    /// <inheritdoc />
    [Category("Genetic Algorithm")]
    public int PopulationSize
    {
      get => svgNestConfig.PopulationSize;
      set => SetProperty(nameof(PopulationSize), () => svgNestConfig.PopulationSize, v => svgNestConfig.PopulationSize = v, value);
    }

    /// <inheritdoc />
    [Category("Nest Settings")]
    public int Rotations
    {
      get => svgNestConfig.Rotations;
      set => SetProperty(nameof(Rotations), () => svgNestConfig.Rotations, v => svgNestConfig.Rotations = v, value);
    }

    /// <inheritdoc />
    [Browsable(false)]
    public int SaveAsFileTypeIndex
    {
      get => svgNestConfig.SaveAsFileTypeIndex;
      set => SetProperty(nameof(SaveAsFileTypeIndex), () => svgNestConfig.SaveAsFileTypeIndex, v => svgNestConfig.SaveAsFileTypeIndex = v, value);
    }

    /// <inheritdoc />
    [Category("Unimplemented")]
    public double Scale
    {
      get => svgNestConfig.Scale;
      set => SetProperty(nameof(Scale), () => svgNestConfig.Scale, v => svgNestConfig.Scale = v, value);
    }

    /// <inheritdoc />
    [Category("Sheet Defaults")]
    public int SheetHeight
    {
      get => svgNestConfig.SheetHeight;
      set => SetProperty(nameof(SheetHeight), () => svgNestConfig.SheetHeight, v => svgNestConfig.SheetHeight = v, value);
    }

    /// <inheritdoc />
    [Category("Sheet Defaults")]
    public int SheetQuantity
    {
      get => svgNestConfig.SheetQuantity;
      set => SetProperty(nameof(SheetQuantity), () => svgNestConfig.SheetQuantity, v => svgNestConfig.SheetQuantity = v, value);
    }

    /// <inheritdoc />
    [Category("File Settings")]
    public double SheetSpacing
    {
      get => svgNestConfig.SheetSpacing;
      set => SetProperty(nameof(SheetSpacing), () => svgNestConfig.SheetSpacing, v => svgNestConfig.SheetSpacing = v, value);
    }

    /// <inheritdoc />
    [Category("Sheet Defaults")]
    public int SheetWidth
    {
      get => svgNestConfig.SheetWidth;
      set => SetProperty(nameof(SheetWidth), () => svgNestConfig.SheetWidth, v => svgNestConfig.SheetWidth = v, value);
    }

    /// <inheritdoc />
    [Category("Simplifications")]
    public bool Simplify
    {
      get => svgNestConfig.Simplify;
      set => SetProperty(nameof(Simplify), () => svgNestConfig.Simplify, v => svgNestConfig.Simplify = v, value);
    }

    /// <inheritdoc />
    [Category("Nest Settings")]
    public double Spacing
    {
      get => svgNestConfig.Spacing;
      set => SetProperty(nameof(Spacing), () => svgNestConfig.Spacing, v => svgNestConfig.Spacing = v, value);
    }

    /// <inheritdoc />
    [Category("Unimplemented")]
    public double TimeRatio
    {
      get => svgNestConfig.TimeRatio;
      set => SetProperty(nameof(TimeRatio), () => svgNestConfig.TimeRatio, v => svgNestConfig.TimeRatio = v, value);
    }

    /// <inheritdoc />
    [Category("Unimplemented")]
    public double Tolerance
    {
      get => svgNestConfig.Tolerance;
      set => SetProperty(nameof(Tolerance), () => svgNestConfig.Tolerance, v => svgNestConfig.Tolerance = v, value);
    }

    /// <inheritdoc />
    [Category("File Settings")]
    public double ToleranceSvg
    {
      get => svgNestConfig.ToleranceSvg;
      set => SetProperty(nameof(ToleranceSvg), () => svgNestConfig.ToleranceSvg, v => svgNestConfig.ToleranceSvg = v, value);
    }

    /// <inheritdoc />
    [Category("Unimplemented")]
    public bool UseHoles
    {
      get => svgNestConfig.UseHoles;
      set => SetProperty(nameof(UseHoles), () => svgNestConfig.UseHoles, v => svgNestConfig.UseHoles = v, value);
    }

    /// <inheritdoc />
    [Category("Experimental")]
    public bool UseParallel
    {
      get => svgNestConfig.UseParallel;
      set => SetProperty(nameof(UseParallel), () => svgNestConfig.UseParallel, v => svgNestConfig.UseParallel = v, value);
    }

    /// <inheritdoc />
    [Category("Experimental")]
    public AnglesEnum StrictAngles
    {
      get => svgNestConfig.StrictAngles;
      set => SetProperty(nameof(StrictAngles), () => svgNestConfig.StrictAngles, v => svgNestConfig.StrictAngles = v, value);
    }

    /// <inheritdoc />
    [Category("Nest Settings")]
    public int Multiplier
    {
      get => svgNestConfig.Multiplier;
      set => SetProperty(nameof(Multiplier), () => svgNestConfig.Multiplier, v => svgNestConfig.Multiplier = v, value);
    }

    /// <inheritdoc />
    [Category("Experimental")]
    public int ParallelNests
    {
      get => svgNestConfig.ParallelNests;
      set => SetProperty(nameof(ParallelNests), () => svgNestConfig.ParallelNests, v => svgNestConfig.ParallelNests = v, value);
    }

    /// <inheritdoc />
    [Category("Unimplemented")]
    public bool ShowPartPositions
    {
      get => svgNestConfig.ShowPartPositions;
      set => SetProperty(nameof(ShowPartPositions), () => svgNestConfig.ShowPartPositions, v => svgNestConfig.ShowPartPositions = v, value);
    }

    public string ToJson()
    {
      return svgNestConfig.ToJson();
    }
  }
}
