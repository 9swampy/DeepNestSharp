namespace DeepNestLib.CiTests.IO
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using DeepNestLib.IO;
  using IxMilia.Dxf;
  using IxMilia.Dxf.Entities;

  public class DxfGenerator
  {
    public IRawDetail GenerateSquare(string name, double size, RectangleType type, bool isClosed = false)
    {
      return GenerateRectangle(name, size, size, type, isClosed);
    }

    public IRawDetail GenerateRectangle(string name, double width, double height, RectangleType type, bool isClosed = false)
    {
      return DxfParser.ConvertDxfToRawDetail(name, new List<DxfEntity>() { Rectangle(width, height, type, isClosed) });
    }

    public DxfFile GenerateRectangle(double width, double height, RectangleType type, bool isClosed = false)
    {
      var result = new DxfFile();
      result.Entities.Add(Rectangle(width, height, type, isClosed));
      return result;
    }

    public DxfPolyline Square(double side, RectangleType rectangleType = RectangleType.FileLoad, bool isClosed = false)
    {
      return Rectangle(side, side, rectangleType, isClosed);
    }

    internal DxfPolyline IsoscelesTriangle(int side)
    {
      return new DxfPolyline(new DxfVertex[]
      {
        new DxfVertex(new DxfPoint(0D, side, 0D)),
        new DxfVertex(new DxfPoint(side, side, 0D)),
        new DxfVertex(new DxfPoint(0D, 0D, 0D)),
        new DxfVertex(new DxfPoint(0D, side, 0D)),
      });
    }

    internal DxfLine Line(int length)
    {
      return new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(length, 0, 0));
    }

    public DxfPolyline Rectangle(double width, double height, RectangleType rectangleType = RectangleType.FileLoad, bool isClosed = false)
    {
      return new DxfPolyline(RectanglePoints(width, height, rectangleType, isClosed).Select(o => new DxfVertex(o)));
    }

    public IEnumerable<DxfPoint> RectanglePoints(double width, double height, RectangleType rectangleType = RectangleType.FileLoad, bool isClosed = false)
    {
      List<DxfPoint> result;
      switch (rectangleType)
      {
        case RectangleType.FileLoad:
          return RectanglePoints(width, height, RectangleType.Normal, true);
        case RectangleType.Normal:
        case RectangleType.TopLeftClockwise:
          result = new List<DxfPoint>
          {
            new DxfPoint(0D, height, 0D),
            new DxfPoint(width, height, 0D),
            new DxfPoint(width, 0D, 0D),
            new DxfPoint(0D, 0D, 0D),
          };
          break;
        case RectangleType.FitFour:
        case RectangleType.TopRightAntiClockwise:
          result = new List<DxfPoint>
          {
            new DxfPoint(width, height, 0D),
            new DxfPoint(0D, height, 0D),
            new DxfPoint(0D, 0D, 0D),
            new DxfPoint(width, 0D, 0D),
          };
          break;
        case RectangleType.BottomLeftClockwise:
          result = new List<DxfPoint>
          {
            new DxfPoint(0D, 0D, 0D),
            new DxfPoint(0D, height, 0D),
            new DxfPoint(width, height, 0D),
            new DxfPoint(width, 0D, 0D),
          };
          break;
        default:
          throw new NotImplementedException();
      }

      if (isClosed)
      {
        result.Add(result[0]);
      }

      return result;
    }
  }
}
