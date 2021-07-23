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
  }

  public class DxfGenerator
  {
    public RawDetail GenerateSquare(string name, double size, RectangleType type)
    {
      return GenerateRectangle(name, size, size, type);
    }

    public RawDetail GenerateRectangle(string name, double sideA, double sideB, RectangleType type)
    {
      return DxfParser.ConvertDxfToRawDetail(name, new List<DxfEntity>() { Rectangle(sideA, sideB, type) });
    }

    public DxfPolyline Rectangle(double side, RectangleType rectangleType = RectangleType.FileLoad)
    {
      return Rectangle(side, side, rectangleType);
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

    public DxfPolyline Rectangle(double sideA, double sideB, RectangleType rectangleType = RectangleType.FileLoad)
    {
      switch (rectangleType)
      {
        case RectangleType.FileLoad:
          return new DxfPolyline(new DxfVertex[]
          {
            new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, sideB, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(sideA, sideB, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(sideA, 0D, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, 0D, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, sideB, 0D)),
          });
        case RectangleType.Normal:
          return new DxfPolyline(new DxfVertex[]
          {
            new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, sideB, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(sideA, sideB, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(sideA, 0D, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, 0D, 0D)),
          });
        case RectangleType.FitFour:
          return new DxfPolyline(new DxfVertex[]
          {
            new DxfVertex(new IxMilia.Dxf.DxfPoint(sideA, sideB, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, sideB, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, 0D, 0D)),
            new DxfVertex(new IxMilia.Dxf.DxfPoint(sideA, 0D, 0D)),
          });
        default:
          throw new NotImplementedException();
      }
    }
  }
}
