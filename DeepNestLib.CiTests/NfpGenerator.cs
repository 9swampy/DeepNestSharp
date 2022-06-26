namespace DeepNestLib.CiTests
{
  using System.Linq;

  public class NfpGenerator
  {
    public INfp GenerateRectangle(string name, double width, double height, RectangleType type, bool isClosed = false)
    {
      var generator = new DxfGenerator();
      var points = generator.RectanglePoints(width, height, type, isClosed);
      var result = new NoFitPolygon(points.Select(o => new SvgPoint(o.X, o.Y)));
      result.Name = name;
      return result;
    }
  }
}
