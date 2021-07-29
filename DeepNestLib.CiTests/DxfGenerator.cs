namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using IxMilia.Dxf.Entities;

  public enum RectangleType
  {
    Normal,
    FileLoad,
    FitFour,
    TopRightAntiClockwise,
    TopLeftClockwise,
    BottomLeftClockwise,
  }

  public class DxfGenerator
  {
    public RawDetail GenerateSquare(string name, double size, RectangleType type, bool isClosed = false)
    {
      return GenerateRectangle(name, size, size, type, isClosed);
    }

    public RawDetail GenerateRectangle(string name, double width, double height, RectangleType type, bool isClosed = false)
    {
      return DxfParser.ConvertDxfToRawDetail(name, new List<DxfEntity>() { Rectangle(width, height, type, isClosed) });
    }

    public DxfPolyline Rectangle(double side, RectangleType rectangleType = RectangleType.FileLoad, bool isClosed = false)
    {
      return Rectangle(side, side, rectangleType, isClosed);
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

    public DxfPolyline Rectangle(double width, double height, RectangleType rectangleType = RectangleType.FileLoad, bool isClosed = false)
    {
      DxfPolyline result;
      switch (rectangleType)
      {
        case RectangleType.FileLoad:
          return Rectangle(width, height, RectangleType.Normal, true);
        case RectangleType.Normal:
        case RectangleType.TopLeftClockwise:
          result = new DxfPolyline(new DxfVertex[]
          {
            new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, height, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(width, height, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(width, 0D, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, 0D, 0D)),
          });
          break;
        case RectangleType.FitFour:
        case RectangleType.TopRightAntiClockwise:
          result = new DxfPolyline(new DxfVertex[]
          {
            new DxfVertex(new IxMilia.Dxf.DxfPoint(width, height, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, height, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, 0D, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(width, 0D, 0D)),
          });
          break;
        case RectangleType.BottomLeftClockwise:
          result = new DxfPolyline(new DxfVertex[]
          {
            new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, 0D, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, height, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(width, height, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(width, 0D, 0D)),
          });
          break;
        default:
          throw new NotImplementedException();
      }

      if (isClosed)
      {
        result.Vertices.Add(result.Vertices[0]);
      }

      return result;
    }
  }
}
