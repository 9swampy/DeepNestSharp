namespace DeepNestLib.Geometry
{
  public class PolygonBounds
  {
    public PolygonBounds(double x, double y, double w, double h)
    {
      this.X = x;
      this.Y = y;
      this.Width = w;
      this.Height = h;
    }

    public double X { get; set; }

    public double Y { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }
  }
}
