namespace DeepNestSharp.Domain.Models
{
  using System;
  using System.ComponentModel;
  using DeepNestLib;
  using DeepNestLib.NestProject;

  public class ObservableSvgNestConfig : ObservablePropertyObject, ISvgNestConfig, IExportableConfig
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
    [Category("File Settings")]
    public bool ExportExecutions
    {
      get => svgNestConfig.ExportExecutions;
      set => SetProperty(nameof(ExportExecutions), () => svgNestConfig.ExportExecutions, v => svgNestConfig.ExportExecutions = v, value);
    }

    /// <inheritdoc />
    [Category("File Settings")]
    public string ExportExecutionPath
    {
      get => svgNestConfig.ExportExecutionPath;
      set => SetProperty(nameof(ExportExecutionPath), () => svgNestConfig.ExportExecutionPath, v => svgNestConfig.ExportExecutionPath = v, value);
    }

    /// <inheritdoc />
    [Category("Unimplemented")]
    public bool ExploreConcave
    {
      get => svgNestConfig.ExploreConcave;
      set => SetProperty(nameof(ExploreConcave), () => svgNestConfig.ExploreConcave, v => svgNestConfig.ExploreConcave = v, value);
    }

    /// <inheritdoc />
    [Browsable(false)]
    public override bool IsDirty => true;

    /// <inheritdoc />
    [Category("File Settings")]
    public string LastDebugFilePath
    {
      get => svgNestConfig.LastDebugFilePath;
      set => SetProperty(nameof(LastDebugFilePath), () => svgNestConfig.LastDebugFilePath, v => svgNestConfig.LastDebugFilePath = v, value);
    }

    /// <inheritdoc />
    [Category("File Settings")]
    public string LastNestFilePath
    {
      get => svgNestConfig.LastNestFilePath;
      set => SetProperty(nameof(LastNestFilePath), () => svgNestConfig.LastNestFilePath, v => svgNestConfig.LastNestFilePath = v, value);
    }

    [Description("Experimental feature merging coaligned and coincident lines when exporting to Dxf so they'll only get cut once (no effect if you're exporting Svg).")]
    /// <inheritdoc />
    [Category("Experimental")]
    public bool MergeLines
    {
      get => svgNestConfig.MergeLines;
      set => SetProperty(nameof(MergeLines), () => svgNestConfig.MergeLines, v => svgNestConfig.MergeLines = v, value);
    }

    /// <inheritdoc />
    [Browsable(false)]
    public int MutationRate
    {
      get => svgNestConfig.MutationRate;
      set => SetProperty(nameof(MutationRate), () => svgNestConfig.MutationRate, v => svgNestConfig.MutationRate = v, value);
    }

    /// <inheritdoc />
    [Browsable(false)]
    public int MutationRateMin => this.svgNestConfig.MutationRateMin;

    /// <inheritdoc />
    [Browsable(false)]
    public int MutationRateMax => this.svgNestConfig.MutationRateMax;

    [Browsable(false)]
    public double MutationRateMinAsPercent => MutationRateMin / 100D;

    [Browsable(false)]
    public double MutationRateMaxAsPercent => MutationRateMax / 100D;

    [Description("Percentage chance that a gene will mutate during procreation. Set it too low and the nest could stagnate. Set it too high and fittest gene sequences may not get inherited.")]
    /// <inheritdoc path="MutationRate">
    [Category("Genetic Algorithm")]
    [DisplayName("Mutation Rate")]
    public double MutationRateAsPercent
    {
      get => svgNestConfig.MutationRate / 100D;
      set => SetProperty(nameof(MutationRate), () => svgNestConfig.MutationRate, v => svgNestConfig.MutationRate = (int)v, Math.Round(value * 100D));
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
    [Description("Gets or sets the maximum total population per Genetic algorithm generation.")]
    [Category("Genetic Algorithm")]
    public int PopulationSize
    {
      get => svgNestConfig.PopulationSize;
      set => SetProperty(nameof(PopulationSize), () => svgNestConfig.PopulationSize, v => svgNestConfig.PopulationSize = v, value);
    }

    /// <inheritdoc />
    [Category("Nest Settings")]
    public int ProcreationTimeout
    {
      get => svgNestConfig.ProcreationTimeout;
      set => SetProperty(nameof(ProcreationTimeout), () => svgNestConfig.ProcreationTimeout, v => svgNestConfig.ProcreationTimeout = v, value);
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
    [Description("Space between parts. When laser cutting this should be 0 so you can benefit from the merge lines functionality.")]
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
    [Description("If set then parts will be restricted. If also set on an individual part, part setting wins.")]
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

    /// <inheritdoc />
    [Category("Unimplemented")]
    public bool UseHoles
    {
      get => svgNestConfig.UseHoles;
      set => SetProperty(nameof(UseHoles), () => svgNestConfig.UseHoles, v => svgNestConfig.UseHoles = v, value);
    }

    /// <inheritdoc />
    [Description("A cache wrapping the C++ MinkowskiSum appears complicit in some invalid overlaying part behaviours.")]
    [Category("Experimental")]
    public bool UseMinkowskiCache
    {
      get => svgNestConfig.UseMinkowskiCache;
      set => SetProperty(nameof(UseMinkowskiCache), () => svgNestConfig.UseMinkowskiCache, v => svgNestConfig.UseMinkowskiCache = v, value);
    }

    /// <inheritdoc />
    [Category("Experimental")]
    public bool UseParallel
    {
      get => svgNestConfig.UseParallel;
      set => SetProperty(nameof(UseParallel), () => svgNestConfig.UseParallel, v => svgNestConfig.UseParallel = v, value);
    }

    /// <inheritdoc />
    [Description("Priority is the notion that some parts should be placed first before any others. " +
      "This has worked well where all parts can fit on a single sheet but it's bee problematic and " +
      "can cause parts to overlay on top of each other. Use with caution. .")]
    [Category("Experimental")]
    public bool UsePriority
    {
      get => svgNestConfig.UsePriority;
      set => SetProperty(nameof(UsePriority), () => svgNestConfig.UsePriority, v => svgNestConfig.UsePriority = v, value);
    }

    /// <inheritdoc />
    [Description("Legacy only used the DllImport. Turn this off with caution. . . and please do give feedback if you" +
      " try turning it off any experience repeatable problems.")]
    [Category("Experimental")]
    public bool UseDllImport
    {
      get => svgNestConfig.UseDllImport;
      set => SetProperty(nameof(UseDllImport), () => svgNestConfig.UseDllImport, v => svgNestConfig.UseDllImport = v, value);
    }

    /// <inheritdoc />
    [Description("Gets or sets the percentage difference between an existing TopNest and a new candidate needed for insertion in to Top collection." +
    "Diversity of the Tops will help keep the Genetic Algorithm innovating at the expense of potentially excluding a novel Top performer." +
    "1=100% which would kill the nest; anecdotally we've found the best is around 0.0001 but YMMV.")]
    [Category("Genetic Algorithm")]
    public double TopDiversity
    {
      get => svgNestConfig.TopDiversity;
      set => SetProperty(nameof(TopDiversity), () => svgNestConfig.TopDiversity, v => svgNestConfig.TopDiversity = v, value);
    }

      ISvgNestConfig IExportableConfig.ExportableInstance => svgNestConfig;

    public string ToJson()
    {
      return svgNestConfig.ToJson();
    }
  }
}
