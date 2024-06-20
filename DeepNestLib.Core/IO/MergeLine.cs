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
      this.Line = line;
    }

    public decimal Slope => this.slope ?? (this.slope = this.CalcSlope()).Value;

    public decimal Intercept => this.intercept ?? (this.intercept = this.CalcIntercept(this.Line)).Value;

    public DxfPoint Left
    {
      get
      {
        if (!this.left.HasValue)
        {
          this.SetLeftRight();
        }

        return this.left.Value;
      }
    }

    public DxfPoint Right
    {
      get
      {
        if (!this.right.HasValue)
        {
          this.SetLeftRight();
        }

        return this.right.Value;
      }
    }

    public DxfLine Line { get; }

    public bool IsVertical => Math.Round(this.Line.P1.X, FractionalDigits) == Math.Round(this.Line.P2.X, FractionalDigits);

    private void SetLeftRight()
    {
      if (this.IsVertical)
      {
        if (this.Line.P1.Y < this.Line.P2.Y)
        {
          this.left = this.Line.P1;
          this.right = this.Line.P2;
        }
        else
        {
          this.left = this.Line.P2;
          this.right = this.Line.P1;
        }
      }
      else if (this.Line.P1.X < this.Line.P2.X)
      {
        this.left = this.Line.P1;
        this.right = this.Line.P2;
      }
      else
      {
        this.left = this.Line.P2;
        this.right = this.Line.P1;
      }
    }

    private decimal CalcSlope()
    {
      if (this.IsVertical)
      {
        return decimal.MaxValue;
      }
      else
      {
        return (decimal)Math.Round((this.Right.Y - this.Left.Y) / (this.Right.X - this.Left.X), FractionalDigits);
      }
    }

    private decimal CalcIntercept(DxfLine line)
    {
      if (this.IsVertical)
      {
        return (decimal)Math.Round(line.P1.X, FractionalDigits);
      }
      else
      {
        return (decimal)Math.Round(line.P1.Y - (double)this.Slope * line.P1.X, FractionalDigits);
      }
    }
  }
}