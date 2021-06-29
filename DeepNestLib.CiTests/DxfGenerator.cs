namespace DeepNestLib.CiTests
{
  using IxMilia.Dxf.Entities;

  public class DxfGenerator
  {
    public DxfPolyline Rectangle(double side)
    {
      return Rectangle(side, side);
    }

    internal DxfPolyline IsoscelesTriangle(int side)
    {
      return new DxfPolyline(new DxfVertex[]
      {
        new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, side, 0D)),
        new DxfVertex(new IxMilia.Dxf.DxfPoint(side, side, 0D)),
        new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, 0D, 0D)),
        new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, side, 0D)),
      });
    }

    public DxfPolyline Rectangle(double sideA, double sideB)
    {
      return new DxfPolyline(new DxfVertex[]
      {
        new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, sideB, 0D)),
        new DxfVertex(new IxMilia.Dxf.DxfPoint(sideA, sideB, 0D)),
        new DxfVertex(new IxMilia.Dxf.DxfPoint(sideA, 0D, 0D)),
        new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, 0D, 0D)),
        new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, sideB, 0D)),
      });
    }
  }
}
