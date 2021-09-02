namespace DeepNestLib
{
  public class RectangleSheet : Sheet
  {
    public void Rebuild()
    {
      this.ReplacePoints(new SvgPoint[4]
      {
        new SvgPoint(X, Y),
        new SvgPoint(X + Width, Y),
        new SvgPoint(X + Width, Y + Height),
        new SvgPoint(X, Y + Height),
      });
    }
  }
}
