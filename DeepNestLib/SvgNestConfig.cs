namespace DeepNestLib
{
  public class SvgNestConfig
  {
    public double Scale = 25;
    public double ClipperScale = 10000000;
    public bool ExploreConcave = false;
    public int Rotations = 4;
    public double SheetSpacing = 0;
    public bool UseHoles = false;
    public double TimeRatio = 0.5;
    public bool MergeLines = false;

    // port features (don't exist in the original DeepNest project)
    public bool clipByHull = false;

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
        return (int)Properties.Settings.Default["PopulationSize"];
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
        return (int)Properties.Settings.Default["MutationRate"];
      }

      set
      {
        Properties.Settings.Default["MutationRate"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }
  }
}
