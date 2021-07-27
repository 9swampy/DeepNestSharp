namespace DeepNestLib.Placement
{
  public interface IPartPlacement
  {
    INfp Hull { get; set; }

    INfp HullSheet { get; set; }

    int Id { get; set; }

    double? MergedLength { get; }

    object MergedSegments { get; set; }

    INfp Part { get; }

    double Rotation { get; set; }

    int Source { get; set; }

    double X { get; set; }

    double Y { get; set; }
  }
}