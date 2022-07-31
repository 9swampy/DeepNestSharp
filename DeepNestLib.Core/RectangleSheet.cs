namespace DeepNestLib
{
  using System;

  public class RectangleSheet : Sheet
  {
    public void Rebuild()
    {
      this.ReplacePoints(new SvgPoint[4]
      {
        new SvgPoint(X, Y),
        new SvgPoint(X + WidthCalculated, Y),
        new SvgPoint(X + WidthCalculated, Y + HeightCalculated),
        new SvgPoint(X, Y + HeightCalculated),
      });
    }

    internal void Build(int width, int height)
    {
      this.ReplacePoints(new SvgPoint[4]
      {
        new SvgPoint(X, Y),
        new SvgPoint(X + width, Y),
        new SvgPoint(X + width, Y + height),
        new SvgPoint(X, Y + height),
      });
    }
    }
}
