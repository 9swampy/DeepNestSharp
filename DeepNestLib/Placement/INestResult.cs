namespace DeepNestLib.Placement
{
  using System.Collections.Generic;

  public interface INestResult
  {
    double Area { get; }

    double? Fitness { get; }

    double FitnessAlt { get; }

    double FitnessBounds { get; }

    double FitnessSheets { get; }

    double FitnessUnplaced { get; }

    double MergedLength { get; }

    int NestIndex { get; }

    PlacementTypeEnum PlacementType { get; }

    long PlacePartTime { get; }

    float[] Rotation { get; set; }

    IList<NFP> UnplacedParts { get; }

    SheetPlacementCollection UsedSheets { get; }
  }
}