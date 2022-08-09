namespace DeepNestLib.Geometry
{
  public class PolygonBounds
  {
    public PolygonBounds(double x, double y, double w, double h)
    {
      X = x;
      Y = y;
      Width = w;
      Height = h;
    }

    public double X { get; set; }

    public double Y { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public double Area => Width * Height;
  }
}
