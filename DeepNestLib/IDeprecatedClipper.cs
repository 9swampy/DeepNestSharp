namespace DeepNestLib
{
  public interface IDeprecatedClipper
  {
    ClipperLib.IntPoint[] ScaleUpPathsOriginal(NFP p, double scale);

    ClipperLib.IntPoint[] ScaleUpPathsSlowerParallel(SvgPoint[] points, double scale = 1);
  }
}
