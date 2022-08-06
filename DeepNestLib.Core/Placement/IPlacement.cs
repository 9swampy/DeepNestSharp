namespace DeepNestLib.Placement
{
  using DeepNestLib.NestProject;

  public interface IPlacement
  {
    bool IsPriority { get; set; }

    double? OffsetX { get; set; }

    double? OffsetY { get; set; }

    int PlacementOrder { get; set; }

    INfp Sheet { get; set; }

    AnglesEnum StrictAngle { get; set; }

    double X { get; set; }

    double Y { get; set; }
  }
}