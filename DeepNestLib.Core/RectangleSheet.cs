namespace DeepNestLib
{
  public class RectangleSheet : Sheet
  {
    internal void Build(double width, double height)
    {
      this.ReplacePoints(new SvgPoint[5]
      {
        new SvgPoint(X, Y),
        new SvgPoint(X, Y + height),
        new SvgPoint(X + width, Y + height),
        new SvgPoint(X + width, Y),
        new SvgPoint(X, Y),
      });
    }
  }
}
