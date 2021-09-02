namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;

  public class LocalContour
  {
    public double Len
    {
      get
      {
        double len = 0;
        for (int i = 1; i <= Points.Count; i++)
        {
          var p1 = Points[i - 1];
          var p2 = Points[i % Points.Count];
          len += Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        return len;
      }
    }

    public List<PointF> Points = new List<PointF>();
    public bool Enable = true;
  }
}
