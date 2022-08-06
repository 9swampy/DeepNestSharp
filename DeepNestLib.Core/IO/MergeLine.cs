namespace DeepNestLib.IO
{
  using System;
  using IxMilia.Dxf;
  using IxMilia.Dxf.Entities;

  public class MergeLine
  {
    private const int FractionalDigits = 4;

    private decimal? slope;
    private decimal? intercept;
    private DxfPoint? left;
    private DxfPoint? right;

    public MergeLine(DxfLine line)
    {
      Line = line;
    }

    public decimal Slope => slope ?? (slope = CalcSlope()).Value;

    public decimal Intercept => intercept ?? (intercept = CalcIntercept(Line)).Value;

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

    public bool IsVertical => Math.Round(Line.P1.X, FractionalDigits) == Math.Round(Line.P2.X, FractionalDigits);

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

    private decimal CalcSlope()
    {
      if (IsVertical)
      {
        return decimal.MaxValue;
      }
      else
      {
        return (decimal)Math.Round((Right.Y - Left.Y) / (Right.X - Left.X), FractionalDigits);
      }
    }

    private decimal CalcIntercept(DxfLine line)
    {
      if (IsVertical)
      {
        return (decimal)Math.Round(line.P1.X, FractionalDigits);
      }
      else
      {
        return (decimal)Math.Round(line.P1.Y - (double)Slope * line.P1.X, FractionalDigits);
      }
    }
  }
}