namespace DeepNestLib
{
  public abstract class PolygonBase
  {
    protected SvgPoint[] points;

    protected PolygonBase(SvgPoint[] points)
    {
      this.points = points;
    }
  }
}
