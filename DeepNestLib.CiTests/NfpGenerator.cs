namespace DeepNestLib.CiTests
{
  using DeepNestLib.CiTests.IO;
  using System.Linq;

  public class NfpGenerator
  {
    public INfp GenerateRectangle(string name, double width, double height, RectangleType type, bool isClosed = false)
    {
      DxfGenerator generator = new DxfGenerator();
      System.Collections.Generic.IEnumerable<IxMilia.Dxf.DxfPoint> points = generator.RectanglePoints(width, height, type, isClosed);
      NoFitPolygon result = new NoFitPolygon(points.Select(o => new SvgPoint(o.X, o.Y)));
      result.Name = name;
      return result;
    }
  }
}
