namespace DeepNestSharp.Domain.Models
{
  using System;
  using System.ComponentModel;
  using DeepNestLib;
  using DeepNestLib.NestProject;
  using DeepNestLib.Placement;

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
      get => this.svgNestConfig.ClipperScale;
      set => this.SetProperty(nameof(this.ClipperScale), () => this.svgNestConfig.ClipperScale, v => this.svgNestConfig.ClipperScale = v, value);
    }

    [Description("Gets or sets whether to clip the simplified polygon used in nesting by the hull. This often improves the fit to the original part but may slightly increase the number of points in the simplification and accordingly may marginally slow the nest. Requires a restart of the application because it's not a part of the cache key so you have to restart to reinitialise the cache.")]
    [Category("Simplifications")]
    /// <inheritdoc />
    public bool ClipByHull
    {
      get => this.svgNestConfig.ClipByHull;
      set => this.SetProperty(nameof(this.ClipByHull), () => this.svgNestConfig.ClipByHull, v => this.svgNestConfig.ClipByHull = v, value);
    }

    /// <inheritdoc />
    [Category("Simplifications")]
    public double CurveTolerance
    {
      get => this.svgNestConfig.CurveTolerance;
      set => this.SetProperty(nameof(this.CurveTolerance), () => this.svgNestConfig.CurveTolerance, v => this.svgNestConfig.CurveTolerance = v, value);
    }

    /// <inheritdoc />
    [Description("Differentiate children when exporting.")]
    [Category("File Settings")]
    public bool DifferentiateChildren
    {
      get => this.svgNestConfig.DifferentiateChildren;
      set => this.SetProperty(nameof(this.DifferentiateChildren), () => this.svgNestConfig.DifferentiateChildren, v => this.svgNestConfig.DifferentiateChildren = v, value);
    }

    /// <inheritdoc />
    [Category("Simplifications")]
    public bool DrawSimplification
    {
      get => this.svgNestConfig.DrawSimplification;
      set => this.SetProperty(nameof(this.DrawSimplification), () => this.svgNestConfig.DrawSimplification, v => this.svgNestConfig.DrawSimplification = v, value);
    }

    /// <inheritdoc />
    [Category("File Settings")]
    public bool ExportExecutions
    {
      get => this.svgNestConfig.ExportExecutions;
      set => this.SetProperty(nameof(this.ExportExecutions), () => this.svgNestConfig.ExportExecutions, v => this.svgNestConfig.ExportExecutions = v, value);
    }

    /// <inheritdoc />
    [Category("File Settings")]
    public string ExportExecutionPath
    {
      get => this.svgNestConfig.ExportExecutionPath;
      set => this.SetProperty(nameof(this.ExportExecutionPath), () => this.svgNestConfig.ExportExecutionPath, v => this.svgNestConfig.ExportExecutionPath = v, value);
    }

    /// <inheritdoc />
    [Category("Unimplemented")]
    public bool ExploreConcave
    {
      get => this.svgNestConfig.ExploreConcave;
      set => this.SetProperty(nameof(this.ExploreConcave), () => this.svgNestConfig.ExploreConcave, v => this.svgNestConfig.ExploreConcave = v, value);
    }

    /// <inheritdoc />
    [Browsable(false)]
    public override bool IsDirty => true;

    /// <inheritdoc />
    [Category("File Settings")]
    public string LastDebugFilePath
    {
      get => this.svgNestConfig.LastDebugFilePath;
      set => this.SetProperty(nameof(this.LastDebugFilePath), () => this.svgNestConfig.LastDebugFilePath, v => this.svgNestConfig.LastDebugFilePath = v, value);
    }

    /// <inheritdoc />
    [Category("File Settings")]
    public string LastNestFilePath
    {
      get => this.svgNestConfig.LastNestFilePath;
      set => this.SetProperty(nameof(this.LastNestFilePath), () => this.svgNestConfig.LastNestFilePath, v => this.svgNestConfig.LastNestFilePath = v, value);
    }

    [Description("Merge coaligned and coincident lines when exporting to Dxf so they'll only get cut once (no effect if you're exporting Svg, and of course Spacing setting needs to be 0).")]
    /// <inheritdoc />
    [Category("File Settings")]
    public bool MergeLines
    {
      get => this.svgNestConfig.MergeLines;
      set => this.SetProperty(nameof(this.MergeLines), () => this.svgNestConfig.MergeLines, v => this.svgNestConfig.MergeLines = v, value);
    }

    /// <inheritdoc />
    [Browsable(false)]
    public int MutationRate
    {
      get => this.svgNestConfig.MutationRate;
      set => this.SetProperty(nameof(this.MutationRate), () => this.svgNestConfig.MutationRate, v => this.svgNestConfig.MutationRate = v, value);
    }

    /// <inheritdoc />
    [Browsable(false)]
    public int MutationRateMin => this.svgNestConfig.MutationRateMin;

    /// <inheritdoc />
    [Browsable(false)]
    public int MutationRateMax => this.svgNestConfig.MutationRateMax;

    [Browsable(false)]
    public double MutationRateMinAsPercent => this.MutationRateMin / 100D;

    [Browsable(false)]
    public double MutationRateMaxAsPercent => this.MutationRateMax / 100D;

    [Description("Percentage chance that a gene will mutate during procreation. Set it too low and the nest could stagnate. Set it too high and fittest gene sequences may not get inherited.")]
    /// <inheritdoc path="MutationRate">
    [Category("Genetic Algorithm")]
    [DisplayName("MutationRate")]
    public double MutationRateAsPercent
    {
      get => this.svgNestConfig.MutationRate / 100D;
      set => this.SetProperty(nameof(this.MutationRate), () => this.svgNestConfig.MutationRate, v => this.svgNestConfig.MutationRate = (int)v, Math.Round(value * 100D));
    }

    /// <inheritdoc />
    [Category("Nest Settings")]
    public bool OffsetTreePhase
    {
      get => this.svgNestConfig.OffsetTreePhase;
      set => this.SetProperty(nameof(this.OffsetTreePhase), () => this.svgNestConfig.OffsetTreePhase, v => this.svgNestConfig.OffsetTreePhase = v, value);
    }

    /// <inheritdoc />
    [Category("Nest Settings")]
    public bool OverlapDetection
    {
      get => this.svgNestConfig.OverlapDetection;
      set => this.SetProperty(nameof(this.OverlapDetection), () => this.svgNestConfig.OverlapDetection, v => this.svgNestConfig.OverlapDetection = v, value);
    }

    /// <inheritdoc />
    [Category("Nest Settings")]
    public PlacementTypeEnum PlacementType
    {
      get => this.svgNestConfig.PlacementType;
      set => this.SetProperty(nameof(this.PlacementType), () => this.svgNestConfig.PlacementType, v => this.svgNestConfig.PlacementType = v, value);
    }

    /// <inheritdoc />
    [Description("Gets or sets the maximum total population per Genetic algorithm generation.")]
    [Category("Genetic Algorithm")]
    public int PopulationSize
    {
      get => this.svgNestConfig.PopulationSize;
      set => this.SetProperty(nameof(this.PopulationSize), () => this.svgNestConfig.PopulationSize, v => this.svgNestConfig.PopulationSize = v, value);
    }

    /// <inheritdoc />
    [Category("Nest Settings")]
    public int ProcreationTimeout
    {
      get => this.svgNestConfig.ProcreationTimeout;
      set => this.SetProperty(nameof(this.ProcreationTimeout), () => this.svgNestConfig.ProcreationTimeout, v => this.svgNestConfig.ProcreationTimeout = v, value);
    }

    /// <inheritdoc />
    [Category("Nest Settings")]
    public int Rotations
    {
      get => this.svgNestConfig.Rotations;
      set => this.SetProperty(nameof(this.Rotations), () => this.svgNestConfig.Rotations, v => this.svgNestConfig.Rotations = v, value);
    }

    /// <inheritdoc />
    [Browsable(false)]
    public int SaveAsFileTypeIndex
    {
      get => this.svgNestConfig.SaveAsFileTypeIndex;
      set => this.SetProperty(nameof(this.SaveAsFileTypeIndex), () => this.svgNestConfig.SaveAsFileTypeIndex, v => this.svgNestConfig.SaveAsFileTypeIndex = v, value);
    }

    /// <inheritdoc />
    [Category("Unimplemented")]
    public double Scale
    {
      get => this.svgNestConfig.Scale;
      set => this.SetProperty(nameof(this.Scale), () => this.svgNestConfig.Scale, v => this.svgNestConfig.Scale = v, value);
    }

    /// <inheritdoc />
    [Category("Sheet Defaults")]
    public double SheetHeight
    {
      get => this.svgNestConfig.SheetHeight;
      set => this.SetProperty(nameof(this.SheetHeight), () => this.svgNestConfig.SheetHeight, v => this.svgNestConfig.SheetHeight = v, value);
    }

    /// <inheritdoc />
    [Category("Sheet Defaults")]
    public int SheetQuantity
    {
      get => this.svgNestConfig.SheetQuantity;
      set => this.SetProperty(nameof(this.SheetQuantity), () => this.svgNestConfig.SheetQuantity, v => this.svgNestConfig.SheetQuantity = v, value);
    }

    /// <inheritdoc />
    [Description("Space between parts and the sheet edge.")]
    [Category("File Settings")]
    public double SheetSpacing
    {
      get => this.svgNestConfig.SheetSpacing;
      set => this.SetProperty(nameof(this.SheetSpacing), () => this.svgNestConfig.SheetSpacing, v => this.svgNestConfig.SheetSpacing = v, value);
    }

    /// <inheritdoc />
    [Category("Sheet Defaults")]
    public double SheetWidth
    {
      get => this.svgNestConfig.SheetWidth;
      set => this.SetProperty(nameof(this.SheetWidth), () => this.svgNestConfig.SheetWidth, v => this.svgNestConfig.SheetWidth = v, value);
    }

    /// <inheritdoc />
    [Category("Simplifications")]
    public bool Simplify
    {
      get => this.svgNestConfig.Simplify;
      set => this.SetProperty(nameof(this.Simplify), () => this.svgNestConfig.Simplify, v => this.svgNestConfig.Simplify = v, value);
    }

    /// <inheritdoc />
    [Description("Space between parts. When laser cutting this should be 0 so you can benefit from the merge lines functionality.")]
    [Category("Nest Settings")]
    public double Spacing
    {
      get => this.svgNestConfig.Spacing;
      set => this.SetProperty(nameof(this.Spacing), () => this.svgNestConfig.Spacing, v => this.svgNestConfig.Spacing = v, value);
    }

    /// <inheritdoc />
    [Category("Unimplemented")]
    public double TimeRatio
    {
      get => this.svgNestConfig.TimeRatio;
      set => this.SetProperty(nameof(this.TimeRatio), () => this.svgNestConfig.TimeRatio, v => this.svgNestConfig.TimeRatio = v, value);
    }

    /// <inheritdoc />
    [Category("Unimplemented")]
    public double Tolerance
    {
      get => this.svgNestConfig.Tolerance;
      set => this.SetProperty(nameof(this.Tolerance), () => this.svgNestConfig.Tolerance, v => this.svgNestConfig.Tolerance = v, value);
    }

    /// <inheritdoc />
    [Category("File Settings")]
    public double ToleranceSvg
    {
      get => this.svgNestConfig.ToleranceSvg;
      set => this.SetProperty(nameof(this.ToleranceSvg), () => this.svgNestConfig.ToleranceSvg, v => this.svgNestConfig.ToleranceSvg = v, value);
    }

    /// <inheritdoc />
    [Description("If set then parts will be restricted. If also set on an individual part, part setting wins.")]
    [Category("Experimental")]
    public AnglesEnum StrictAngles
    {
      get => this.svgNestConfig.StrictAngles;
      set => this.SetProperty(nameof(this.StrictAngles), () => this.svgNestConfig.StrictAngles, v => this.svgNestConfig.StrictAngles = v, value);
    }

    /// <inheritdoc />
    [Category("Nest Settings")]
    public int Multiplier
    {
      get => this.svgNestConfig.Multiplier;
      set => this.SetProperty(nameof(this.Multiplier), () => this.svgNestConfig.Multiplier, v => this.svgNestConfig.Multiplier = v, value);
    }

    /// <inheritdoc />
    [Category("Experimental")]
    public int ParallelNests
    {
      get => this.svgNestConfig.ParallelNests;
      set => this.SetProperty(nameof(this.ParallelNests), () => this.svgNestConfig.ParallelNests, v => this.svgNestConfig.ParallelNests = v, value);
    }

    /// <inheritdoc />
    [Category("Unimplemented")]
    public bool ShowPartPositions
    {
      get => this.svgNestConfig.ShowPartPositions;
      set => this.SetProperty(nameof(this.ShowPartPositions), () => this.svgNestConfig.ShowPartPositions, v => this.svgNestConfig.ShowPartPositions = v, value);
    }

    /// <inheritdoc />
    [Category("Unimplemented")]
    public bool UseHoles
    {
      get => this.svgNestConfig.UseHoles;
      set => this.SetProperty(nameof(this.UseHoles), () => this.svgNestConfig.UseHoles, v => this.svgNestConfig.UseHoles = v, value);
    }

    /// <inheritdoc />
    [Description("A cache wrapping the C++ MinkowskiSum appears complicit in some invalid overlaying part behaviours.")]
    [Category("Experimental")]
    public bool UseMinkowskiCache
    {
      get => this.svgNestConfig.UseMinkowskiCache;
      set => this.SetProperty(nameof(this.UseMinkowskiCache), () => this.svgNestConfig.UseMinkowskiCache, v => this.svgNestConfig.UseMinkowskiCache = v, value);
    }

    /// <inheritdoc />
    [Category("Experimental")]
    public bool UseParallel
    {
      get => this.svgNestConfig.UseParallel;
      set => this.SetProperty(nameof(this.UseParallel), () => this.svgNestConfig.UseParallel, v => this.svgNestConfig.UseParallel = v, value);
    }

    /// <inheritdoc />
    [Description("Priority is the notion that some parts should be placed first before any others. This has worked well where all parts can fit on a single sheet, but it's been problematic and can cause parts to overlay on top of each other. Use with caution...")]
    [Category("Experimental")]
    public bool UsePriority
    {
      get => this.svgNestConfig.UsePriority;
      set => this.SetProperty(nameof(this.UsePriority), () => this.svgNestConfig.UsePriority, v => this.svgNestConfig.UsePriority = v, value);
    }

    /// <inheritdoc />
    [Description("Legacy only used the DllImport. Turn this off with caution... and please do give feedback if you try turning it off any experience repeatable problems.")]
    [Category("Experimental")]
    public bool UseDllImport
    {
      get => this.svgNestConfig.UseDllImport;
      set => this.SetProperty(nameof(this.UseDllImport), () => this.svgNestConfig.UseDllImport, v => this.svgNestConfig.UseDllImport = v, value);
    }

    /// <inheritdoc />
    [Description("Gets or sets the percentage difference between an existing TopNest and a new candidate needed for insertion in to Top collection. Diversity of the Tops will help keep the Genetic Algorithm innovating at the expense of potentially excluding a novel Top performer. 1 = 100% which would kill the nest; anecdotally we've found the best is around 0.0001 but YMMV.")]
    [Category("Genetic Algorithm")]
    public double TopDiversity
    {
      get => this.svgNestConfig.TopDiversity;
      set => this.SetProperty(nameof(this.TopDiversity), () => this.svgNestConfig.TopDiversity, v => this.svgNestConfig.TopDiversity = v, value);
    }

    ISvgNestConfig IExportableConfig.ExportableInstance => this.svgNestConfig;

    public string ToJson()
    {
      return this.svgNestConfig.ToJson();
    }
  }
}
