namespace DeepNestLib
{
  using System.Collections.Generic;

  public interface IDeprecatedClipper
  {
    List<ClipperLib.IntPoint> ScaleUpPathsOriginal(NoFitPolygon p, double scale);

    ClipperLib.IntPoint[] ScaleUpPathsSlowerParallel(SvgPoint[] points, double scale = 1);
  }
}
