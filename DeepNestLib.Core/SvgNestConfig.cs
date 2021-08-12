namespace DeepNestLib
{
  using DeepNestLib.NestProject;
  using Properties = DeepNestLib.Core;

  public class SvgNestConfig : ISvgNestConfig
  {
    public const int PopulationMin = 50;
    public const int PopulationMax = 800;
    public const int MutationRateMin = 1;
    public const int MutationRateMax = 5000;
    public const int MultiplierMin = 1;
    public const int MultiplierMax = 100;
    public const int ParallelNestsMin = 1;
    public const int ParallelNestsMax = 30;

    public double Scale { get; set; } = 25;

    public double ClipperScale { get; set; } = 10000000;

    public bool ExploreConcave { get; set; } = false;

    public int Rotations { get; set; } = 4;

    public double SheetSpacing { get; set; } = 0;

    public bool UseHoles { get; set; } = false;

    /// <summary>
    /// Max bound for bezier->line segment conversion, in native SVG units.
    /// </summary>
    public double Tolerance { get; set; } = 2;

    /// <summary>
    /// Fudge factor for browser inaccuracy in SVG unit handling.
    /// </summary>
    public double ToleranceSvg { get; set; } = 0.005;

    public double TimeRatio { get; set; } = 0.5;

    public bool MergeLines { get; set; } = false;

    /// <inheritdoc />
    public bool ClipByHull
    {
      get
      {
        return (bool)Properties.Settings.Default["ClipByHull"];
      }

      set
      {
        Properties.Settings.Default["ClipByHull"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public double CurveTolerance
    {
      get
      {
        return (double)Properties.Settings.Default["CurveTolerance"];
      }

      set
      {
        Properties.Settings.Default["CurveTolerance"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public int SaveAsFileTypeIndex
    {
      get
      {
        return (int)Properties.Settings.Default["SaveAsFileTypeIndex"];
      }

      set
      {
        Properties.Settings.Default["SaveAsFileTypeIndex"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public int SheetWidth
    {
      get
      {
        return (int)Properties.Settings.Default["SheetWidth"];
      }

      set
      {
        Properties.Settings.Default["SheetWidth"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public int SheetHeight
    {
      get
      {
        return (int)Properties.Settings.Default["SheetHeight"];
      }

      set
      {
        Properties.Settings.Default["SheetHeight"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public int SheetQuantity
    {
      get
      {
        return (int)Properties.Settings.Default["SheetQuantity"];
      }

      set
      {
        Properties.Settings.Default["SheetQuantity"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public PlacementTypeEnum PlacementType
    {
      get
      {
        return (PlacementTypeEnum)Properties.Settings.Default["PlacementType"];
      }

      set
      {
        Properties.Settings.Default["PlacementType"] = (int)value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public bool Simplify
    {
      get
      {
        return (bool)Properties.Settings.Default["Simplify"];
      }

      set
      {
        Properties.Settings.Default["Simplify"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public bool OffsetTreePhase
    {
      get
      {
        return (bool)Properties.Settings.Default["OffsetTreePhase"];
      }

      set
      {
        Properties.Settings.Default["OffsetTreePhase"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public double Spacing
    {
      get
      {
        return (double)Properties.Settings.Default["Spacing"];
      }

      set
      {
        Properties.Settings.Default["Spacing"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public int PopulationSize
    {
      get
      {
        var result = (int)Properties.Settings.Default["PopulationSize"];
        if (result < PopulationMin) return PopulationMin;
        if (result > PopulationMax) return PopulationMax;
        return result;
      }

      set
      {
        Properties.Settings.Default["PopulationSize"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public int MutationRate
    {
      get
      {
        var result = (int)Properties.Settings.Default["MutationRate"];
        if (result < MutationRateMin) return MutationRateMin;
        if (result > MutationRateMax) return MutationRateMax;
        return result;
      }

      set
      {
        Properties.Settings.Default["MutationRate"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public int Multiplier
    {
      get
      {
        var result = (int)Properties.Settings.Default["Multiplier"];
        if (result < MutationRateMin) return MultiplierMin;
        if (result > MutationRateMax) return MultiplierMax;
        return result;
      }

      set
      {
        Properties.Settings.Default["Multiplier"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public bool DrawSimplification
    {
      get
      {
        return (bool)Properties.Settings.Default["DrawSimplification"];
      }

      set
      {
        Properties.Settings.Default["DrawSimplification"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public AnglesEnum StrictAngles
    {
      get
      {
        try
        {
          return (AnglesEnum)Properties.Settings.Default["StrictAngles"];
        }
        catch (System.Exception)
        {
          return AnglesEnum.None;
        }
      }

      set
      {
        Properties.Settings.Default["StrictAngles"] = (int)value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public bool UseParallel
    {
      get
      {
        return (bool)Properties.Settings.Default["UseParallel"];
      }

      set
      {
        Properties.Settings.Default["UseParallel"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public int ParallelNests
    {
      get
      {
        var result = (int)Properties.Settings.Default["ParallelNests"];
        if (result < ParallelNestsMin) return ParallelNestsMin;
        if (result > ParallelNestsMax) return ParallelNestsMax;
        return result;
      }

      set
      {
        Properties.Settings.Default["ParallelNests"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public bool ShowPartPositions
    {
      get
      {
        return (bool)Properties.Settings.Default["ShowPartPositions"];
      }

      set
      {
        Properties.Settings.Default["ShowPartPositions"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public string ToJson()
    {
      return SvgNestConfigJsonConverter.ToJson(this);
    }
  }
}
