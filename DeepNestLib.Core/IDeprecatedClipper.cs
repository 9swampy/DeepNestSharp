namespace DeepNestLib
{
  public interface IDeprecatedClipper
  {
    ClipperLib.IntPoint[] ScaleUpPathsOriginal(NoFitPolygon p, double scale);

    ClipperLib.IntPoint[] ScaleUpPathsSlowerParallel(SvgPoint[] points, double scale = 1);
  }
}
