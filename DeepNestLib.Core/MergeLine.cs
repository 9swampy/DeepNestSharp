namespace DeepNestLib
{
  using IxMilia.Dxf;
  using IxMilia.Dxf.Entities;
  using System;

  public class MergeLine
  {
    private const int FractionalDigits = 4;

    private double? slope;
    private double? intercept;
    private DxfPoint? left;
    private DxfPoint? right;

    public MergeLine(DxfLine line)
    {
      this.Line = line;
    }

    public double Slope => slope ?? (slope = CalcSlope()).Value;

    public double Intercept => intercept ?? (intercept = CalcIntercept(Line)).Value;

    public DxfPoint Left
    {
      get
      {
        if (!left.HasValue)
        {
          SetLeftRight();
        }

        return left.Value;
      }
    }

    public DxfPoint Right
    {
      get
      {
        if (!right.HasValue)
        {
          SetLeftRight();
        }

        return right.Value;
      }
    }

    public DxfLine Line { get; }

    public bool IsVertical => Line.P1.X == Line.P2.X;

    private void SetLeftRight()
    {
      if (IsVertical)
      {
        if (Line.P1.Y < Line.P2.Y)
        {
          left = Line.P1;
          right = Line.P2;
        }
        else
        {
          left = Line.P2;
          right = Line.P1;
        }
      }
      else if (Line.P1.X < Line.P2.X)
      {
        left = Line.P1;
        right = Line.P2;
      }
      else
      {
        left = Line.P2;
        right = Line.P1;
      }
    }

    private double CalcSlope()
    {
      if (IsVertical)
      {
        return double.PositiveInfinity;
      }
      else
      {
        return Math.Round((Right.Y - Left.Y) / (Right.X - Left.X), FractionalDigits);
      }
    }

    private double CalcIntercept(DxfLine line)
    {
      if (IsVertical)
      {
        return line.P1.X;
      }
      else
      {
        return Math.Round(line.P1.Y - (Slope * line.P1.X), FractionalDigits);
      }
    }
  }
}