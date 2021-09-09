[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DeepNestLib.CiTests")]

namespace DeepNestLib
{
  using System;
  using DeepNestLib.NestProject;

  public class SvgNestConfig : ISvgNestConfig
  {
    public const int PopulationMin = 50;
    public const int PopulationMax = 800;
    public const int MultiplierMin = 1;
    public const int MultiplierMax = 100;
    public const int ParallelNestsMin = 1;
    public const int ParallelNestsMax = 30;

    public SvgNestConfig()
    {
#if NCRUNCH
      throw new NotImplementedException();
#endif
    }

    /// <inheritdoc />
    public double Scale { get; set; } = 25;

    /// <inheritdoc />
    public double ClipperScale { get; set; } = 10000000;

    /// <inheritdoc />
    public bool ExploreConcave { get; set; } = false;

    /// <inheritdoc />
    public int Rotations { get; set; } = 4;

    /// <inheritdoc />
    public double SheetSpacing { get; set; } = 0;

    /// <inheritdoc />
    public bool UseHoles { get; set; } = false;

    /// <summary>
    /// Max bound for bezier->line segment conversion, in native SVG units.
    /// </summary>
    public double Tolerance { get; set; } = 2;

    /// <summary>
    /// Fudge factor for browser inaccuracy in SVG unit handling.
    /// </summary>
    public double ToleranceSvg { get; set; } = 0.005;

    /// <inheritdoc />
    public double TimeRatio { get; set; } = 0.5;

    /// <inheritdoc />
    public bool MergeLines { get; set; } = false;

    /// <inheritdoc />
    public bool ClipByHull
    {
      get
      {
        return (bool)Settings.Default["ClipByHull"];
      }

      set
      {
        Settings.Default["ClipByHull"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public double CurveTolerance
    {
      get
      {
        return (double)Settings.Default["CurveTolerance"];
      }

      set
      {
        Settings.Default["CurveTolerance"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public bool DrawSimplification
    {
      get
      {
        return (bool)Settings.Default["DrawSimplification"];
      }

      set
      {
        Settings.Default["DrawSimplification"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public bool ExportExecutions
    {
      get
      {
        return (bool)Settings.Default["ExportExecutions"];
      }

      set
      {
        Settings.Default["ExportExecutions"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public string ExportExecutionPath
    {
      get
      {
        return (string)Settings.Default["ExportExecutionPath"];
      }

      set
      {
        Settings.Default["ExportExecutionPath"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    public string LastDebugFilePath
    {
      get
      {
        return (string)Settings.Default["LastDebugFilePath"];
      }

      set
      {
        Settings.Default["LastDebugFilePath"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    public string LastNestFilePath
    {
      get
      {
        return (string)Settings.Default["LastNestFilePath"];
      }

      set
      {
        Settings.Default["LastNestFilePath"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public int SaveAsFileTypeIndex
    {
      get
      {
        return (int)Settings.Default["SaveAsFileTypeIndex"];
      }

      set
      {
        Settings.Default["SaveAsFileTypeIndex"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public int SheetWidth
    {
      get
      {
        return (int)Settings.Default["SheetWidth"];
      }

      set
      {
        Settings.Default["SheetWidth"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public int SheetHeight
    {
      get
      {
        return (int)Settings.Default["SheetHeight"];
      }

      set
      {
        Settings.Default["SheetHeight"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public int SheetQuantity
    {
      get
      {
        return (int)Settings.Default["SheetQuantity"];
      }

      set
      {
        Settings.Default["SheetQuantity"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public PlacementTypeEnum PlacementType
    {
      get
      {
        return (PlacementTypeEnum)Settings.Default["PlacementType"];
      }

      set
      {
        Settings.Default["PlacementType"] = (int)value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public bool Simplify
    {
      get
      {
        return (bool)Settings.Default["Simplify"];
      }

      set
      {
        Settings.Default["Simplify"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public bool OffsetTreePhase
    {
      get
      {
        return (bool)Settings.Default["OffsetTreePhase"];
      }

      set
      {
        Settings.Default["OffsetTreePhase"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public double Spacing
    {
      get
      {
        return (double)Settings.Default["Spacing"];
      }

      set
      {
        Settings.Default["Spacing"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public int PopulationSize
    {
      get
      {
        var result = (int)Settings.Default["PopulationSize"];
        if (result < PopulationMin) return PopulationMin;
        if (result > PopulationMax) return PopulationMax;
        return result;
      }

      set
      {
        Settings.Default["PopulationSize"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public int ProcreationTimeout
    {
      get
      {
        var result = (int)Settings.Default["ProcreationTimeout"];
        return result;
      }

      set
      {
        Settings.Default["ProcreationTimeout"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public int MutationRate
    {
      get
      {
        var result = (int)Settings.Default["MutationRate"];
        if (result < MutationRateMin)
        {
          return MutationRateMin;
        }

        if (result > MutationRateMax)
        {
          return MutationRateMax;
        }

        return result;
      }

      set
      {
        Settings.Default["MutationRate"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    public int MutationRateMin => 1;

    public int MutationRateMax => 60;

    /// <inheritdoc />
    public int Multiplier
    {
      get
      {
        var result = (int)Settings.Default["Multiplier"];
        if (result < MutationRateMin)
        {
          return MultiplierMin;
        }

        if (result > MutationRateMax)
        {
          return MultiplierMax;
        }

        return result;
      }

      set
      {
        Settings.Default["Multiplier"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public AnglesEnum StrictAngles
    {
      get
      {
        try
        {
          return (AnglesEnum)Settings.Default["StrictAngles"];
        }
        catch (System.Exception)
        {
          return AnglesEnum.None;
        }
      }

      set
      {
        Settings.Default["StrictAngles"] = (int)value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public bool UseParallel
    {
      get
      {
        return (bool)Settings.Default["UseParallel"];
      }

      set
      {
        Settings.Default["UseParallel"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public int ParallelNests
    {
      get
      {
        var result = (int)Settings.Default["ParallelNests"];
        if (result < ParallelNestsMin)
        {
          return ParallelNestsMin;
        }

        if (result > ParallelNestsMax)
        {
          return ParallelNestsMax;
        }

        return result;
      }

      set
      {
        Settings.Default["ParallelNests"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public bool ShowPartPositions
    {
      get
      {
        return (bool)Settings.Default["ShowPartPositions"];
      }

      set
      {
        Settings.Default["ShowPartPositions"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public bool UseDllImport
    {
      get
      {
        return (bool)Settings.Default["UseDllImport"];
      }

      set
      {
        Settings.Default["UseDllImport"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public bool UseMinkowskiCache
    {
      get
      {
        return (bool)Settings.Default["UseMinkowskiCache"];
      }

      set
      {
        Settings.Default["UseMinkowskiCache"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    /// <inheritdoc />
    public bool UsePriority
    {
      get
      {
        return (bool)Settings.Default["UsePriority"];
      }

      set
      {
        Settings.Default["UsePriority"] = value;
        Settings.Default.Save();
        Settings.Default.Upgrade();
      }
    }

    public string ToJson()
    {
      return SvgNestConfigJsonConverter.ToJson(this);
    }

    internal static ISvgNestConfig FromJson(string json)
    {
      return SvgNestConfigJsonConverter.FromJson(json);
    }
  }
}
