namespace DeepNestLib
{
  public class RectangleSheet : Sheet
  {
    internal void Build(double width, double height)
    {
      this.ReplacePoints(new SvgPoint[5]
      {
        new SvgPoint(this.X, this.Y),
        new SvgPoint(this.X, this.Y + height),
        new SvgPoint(this.X + width, this.Y + height),
        new SvgPoint(this.X + width, this.Y),
        new SvgPoint(this.X, this.Y),
      });
    }
  }
}
