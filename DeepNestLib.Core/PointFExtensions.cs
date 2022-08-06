namespace DeepNestLib
{
  using System;
  using System.Drawing;

  public static class PointFExtensions
  {
    public static double DistTo(this PointF p, PointF p2)
    {
      return Math.Sqrt(Math.Pow(p.X - p2.X, 2) + Math.Pow(p.Y - p2.Y, 2));
    }
  }
}
